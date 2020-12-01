using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace HomeRobot.Core
{
    public enum ChildStatus { Released = 0, Carried, Penned }

    public class Child : MobileObject
    {
        public ChildStatus Status { get; set; }

        public Child(Environment env, Point pos) : base(env)
        {
            Reset(pos);
        }

        public override void Reset(Point pos)
        {
            base.Reset(pos);
            Status = ChildStatus.Released;
        }

        protected override void SetObject(Point pos)
        {
            Environment[pos].SetMobileObject(CellStatus.Child, this);
            Environment[CurrentPos].SetMobileObject(CellStatus.Child, null);
        }

        private bool PushObstacle(Point pos, MovementDirection dir)
        {
            var moved = Environment[pos].GetMobileObject(CellStatus.Obstacle).MoveTo(dir,
                p => !Environment.OutOfBoundaries(p) && !Environment[p].IsPlaypen() &&
                     (Environment[p].IsEmpty() || (Environment[p].IsObstacled() && PushObstacle(p, dir))));
            //if (moved)
            //    return true;
            //var newPos = new Point(pos.X + Environment.offsets[(int)dir].X, pos.Y + Environment.offsets[(int)dir].Y);
            //if (environment[newPos].IsObstacled())
            //    return PushObstacle(newPos, dir);
            //else
            //    return false;
            return moved;
        }

        //private bool PushObstacle(Point pos, MovementDirection dir)
        //{
        //    Point? newPos = NewPos(pos, dir);
        //    if (newPos == null) //Fuera del ambiente
        //        return false;

        //    var origin = environment[pos];
        //    var destination = environment[(Point)newPos];

        //    if (destination.IsEmpty() && !environment.InPlaypen((Point)newPos))
        //    {
        //        destination.CurrentStatus = CellStatus.Obstacle;
        //        origin.CurrentStatus = CellStatus.Empty;
        //        return true;
        //    }
        //    else if (destination.IsObstacled())
        //        //Llamar recursivamente a la función hasta llegar al último obstáculo
        //        //en esa dirección que pueda moverse a una casilla vacía o hasta llegar 
        //        //a las fronteras del ambiente o a una casilla para donde no puede moverse
        //        return PushObstacle((Point)newPos, dir); 
        //    else
        //        return false; //Si la casilla de destino no está vacía ni tiene un obstáculo no se puede mover
        //}

        public void TrackRobot(object sender, EventArgs args)
        {
            var robot = (HomeRobot)sender;
            CurrentPos = robot.CurrentPos;
        }

        public event EventHandler Playing;
        public event EventHandler Played;

        //Jugada del niño en la que escoge hacia dónde se mueve y ejecuta el movimiento
        public void Play()
        {
            //Si está cargado o en el corral no puede moverse
            if ((Status == ChildStatus.Carried) || (Status == ChildStatus.Penned))
                return;

            if (Playing != null)
                Playing(this, null);

            //Direcciones hacia donde se puede mover
            var adjacents = Environment.AdjacentCells(CurrentPos, p =>
                                !Environment[p].IsPlaypen() &&
                                (Environment[p].IsEmpty() ||
                                 Environment[p].IsObstacled()));
            int n;
            if (adjacents.Count == 0)
                return;
            n = randomMov.Next();
            if (adjacents.Count <= n)
                return;

            //Escoger aleatoriamente una dirección 
            var tuple = adjacents.ToArray<Tuple<MovementDirection, Point>>()[n];
            
            var moved = MoveTo(tuple.Item2, p => !Environment[p].IsObstacled() || PushObstacle(p, tuple.Item1));

            if (!moved)
                return;

            //Obtener la cuadrícula de la posición anterior
            var grid = Environment.Grid(PreviousPos);

            //Obtener el número de niños dentro de la cuadrícula
            var ch = Environment.ChildrenInGrid(grid);

             //Eliminar las casillas que no están limpias
            grid = Environment.CleanGrid(grid);          
            
            //Obtener el número máximo de suciedad que puede ser generada
            //dependiendo de la cantidad de casillas vacías y de niños 
            var dirt = Math.Min(grid.Count, ch <= 3 ? Environment.dirtPerGrid[ch] : Environment.dirtPerGrid[3]);
 
            //Generar aleatoriamente de 0-dirt casillas sucias
            Environment.GenDirt(dirt, grid);

            if (Played != null)
                Played(this, null);
        }
    }

}
