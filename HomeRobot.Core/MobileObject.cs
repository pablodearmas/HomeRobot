using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Troschuetz.Random.Distributions.Discrete;

namespace HomeRobot.Core
{
    //public enum Operations
    //{
    //    DoNothing, Move, CleanCell, ReleaseBaby
    //};

    public enum MovementDirection { Up, Down, Left, Right};

    public class MovingArgs : EventArgs
    {
        public Point NewPosition { get; set; }
    }

    public abstract class MobileObject
    {
        protected DiscreteUniformDistribution randomMov;

        public MobileObject(Environment env)
        {
            Environment = env;
        }

        public virtual void Reset(Point pos)
        {
            randomMov = new DiscreteUniformDistribution(0, 4);
            CurrentPos = pos;
            PreviousPos = new Point();
        }

        public Point PreviousPos { get; protected set; }
        public Point CurrentPos { get; protected set; }

        public Environment Environment { get; protected set; }

        protected abstract void SetObject(Point pos);

        public event EventHandler Moved;
        public event EventHandler<MovingArgs> Moving;

        public bool MoveTo(MovementDirection op, Func<Point, bool> precondition = null)
        {
            var pos = new Point(CurrentPos.X + Environment.offsets[(int)op].X,
                                CurrentPos.Y + Environment.offsets[(int)op].Y);
            return MoveTo(pos,  precondition);
        }

        public bool MoveTo(Point pos, Func<Point, bool> precondition = null)
        {
            if (Moving != null)
                Moving(this, new MovingArgs() { NewPosition = pos });

            var origin = Environment[CurrentPos];
            var destination = Environment[pos];

            if (precondition != null && !precondition(pos))
                return false;

            SetObject(pos);

            PreviousPos = CurrentPos;
            CurrentPos = pos;

            if (Moved != null)
                Moved(this, null);
            
            return true;
        }

        protected virtual bool RandomMove(HashSet<Tuple<MovementDirection, Point>> adjacents, Func<Point, bool> precondition = null)
        {
            if (adjacents.Count > 0)
            {
                //Utilizar un generador para cada tipo de objeto
                var n = randomMov.Next() % adjacents.Count;
                var oper = adjacents.ToArray<Tuple<MovementDirection, Point>>()[n];
                return MoveTo(oper.Item2, precondition);
            }
            else
                return false;
        }
    }
}
