using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Troschuetz.Random.Distributions.Discrete;

namespace HomeRobot.Core
{
    public class RandomRobot: HomeRobot
    {
        public RandomRobot(Environment env) : base(env)
        {

        }
        protected void ExecuteRandomOperation(HashSet<RobotOperations> opers, HashSet<Tuple<MovementDirection, Point>> adjacents)
        {
            if (Environment[CurrentPos].IsDirty())
                Clean();
            else if (adjacents.Count > 0)
                RandomMove(adjacents);
        }

        public override void Reset(Point pos)
        {
            base.Reset(pos);
        }

        //PlayRandom va a obtener las posibles operaciones a realizar (movimientos o limpieza)
        //aleatoriamente va a ejecutar una
        //Si es un movimiento y el robot carga un niño, puede aleatoriamente volver a ejecutar 
        //un movimiento o no hacer nada.      
        public override void Play()
        {
            var carrying = IsCarryingChild();
            var adjacents = Environment.AdjacentCells(CurrentPos,
                p => !Environment[p].IsObstacled() &&
                (!Environment[p].IsChild() || !IsCarryingChild()) &&
                (!Environment[CurrentPos].IsPlaypen() || !IsCarryingChild() || Environment[p].IsPlaypen()));

            if (adjacents.Count == 0)
                adjacents = Environment.AdjacentCells(CurrentPos,
                            p => !Environment[p].IsObstacled() &&
                            (!Environment[p].IsChild() || !IsCarryingChild()));

            //Si hay otro lugar a donde ir que no regrese al punto anterior
            if (adjacents.Count > 1)
                adjacents.RemoveWhere(x => x.Item2 == PreviousPos);

            var opers = AllowedOperations(adjacents);

            if (opers == null || opers.Count == 0)
                return;

            ExecuteRandomOperation(opers, adjacents);

            if (carrying && IsCarryingChild())
            {
                adjacents = Environment.AdjacentCells(CurrentPos,
                            p => (PreviousPos != p) &&
                            !Environment[p].IsObstacled() &&
                            (!Environment[p].IsChild() || !IsCarryingChild()) &&
                            (!Environment[CurrentPos].IsPlaypen() || !IsCarryingChild() || Environment[p].IsPlaypen()));

                opers = AllowedOperations(adjacents);

                if (opers == null || opers.Count == 0)
                    return;

                ExecuteRandomOperation(opers, adjacents);

            }
            isMoving = false;
        }
    }
}
