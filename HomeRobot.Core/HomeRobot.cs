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
    //Operaciones que pueden ser realizadas por el robot, dependiendo de el estado del ambiente 
    //a su alrededor
    public enum RobotOperations { DoNothing = -1, Move, CleanCell };

    public abstract class HomeRobot : MobileObject
    {
        public event EventHandler Playing;
        public event EventHandler Played;

        protected BernoulliDistribution operationRandom;

        private Child child;

        public Child CarryChild
        {
            get
            {
                return child;
            }
            set
            {
                if (child != null)
                    Moved -= child.TrackRobot;
                child = value;
                if (child != null)
                {
                    child.Status = ChildStatus.Carried;
                    Moved += child.TrackRobot;
                }

            }
        }

        public bool IsCarryingChild()
        {
            //var bb = environment[CurrentPos].Baby;
            //return (bb != null) && (bb.Status == BabyStatus.Carried);
            return child != null;
        }

        //Cuando el robot carga un niño, puede decidir moverse o limpiar la celda en que se encuentra
        //Si escoge moverse, puede hacerlo uno o dos pasos consecutivos que no tienen que ser
        //en la misma dirección. Este campo se utiliza como bandera para marcar que ya se movió 
        //un paso, lo que significa que pudiera moverse otro paso o quedarse en el lugar.
        protected bool isMoving;

        public int MovementCount { get; protected set; }

        public HomeRobot(Environment env) : base(env)
        {
            Reset(new Point());
        }

        public override void Reset(Point pos)
        {
            base.Reset(pos);
            CarryChild = null;
            isMoving = false;
            MovementCount = 0;
        }

        protected override void SetObject(Point pos)
        {
            var destination = Environment[pos];
            destination.SetMobileObject(CellStatus.Robot, this);
            if (destination.IsChild()) //Cargar al niño y poner el robot en la nueva posición
            {
                CarryChild = (Child)destination.GetMobileObject(CellStatus.Child);
                destination.SetMobileObject(CellStatus.Child, null);
            }
            Environment[CurrentPos].SetMobileObject(CellStatus.Robot, null);
        }

        //Suelta al niño en la casilla donde se encuentra y le cambia el estado en dependencia de la
        //ubicación de la casilla (dentro o fuera del corral)
        public void ReleaseChild()
        {
            //Si la posición actual del robot es dentro del corral 
            //al soltar el niño se queda en estado acorralado
            if (Environment[CurrentPos].IsPlaypen())
                this.child.Status = ChildStatus.Penned; //Estado acorralado (no se puede mover ni ensuciar)
            else
                this.child.Status = ChildStatus.Released; //Estado libre, no está cargado ni dentro del corral (se puede mover y ensuciar)
            Environment[CurrentPos].SetMobileObject(CellStatus.Child, child);
            CarryChild = null;
        }

        //Elimina la suciedad de la casilla en la que se enuentra
        public void Clean()
        {
            Environment.Clean(CurrentPos);
        }

        //Todas las operaciones que se pueden realizar desde la posición en que se encuentra el robot
        //excludePos se usa habitualmente para excluir la posición anterior
        // private HashSet<RobotOperations> AllowedOperations(Point pos, bool toDirty = false, Point? excludePos = null)
        //var adjacents = environment.AdjacentCells(CurrentPos,
        //         p => toDirty ? (environment[p].CurrentStatus == CellStatus.Dirt) :
        //         (excludePos != null) && (excludePos != p) &&
        //         !environment[pos].IsObstacled() &&
        //         (!environment[pos].IsChild() || !IsCarryingChild()));

        protected HashSet<RobotOperations> AllowedOperations(HashSet<Tuple<MovementDirection, Point>> adjacents)
        {
            var opers = new HashSet<RobotOperations>();

            if (adjacents != null && adjacents.Count > 0)
                opers.Add(RobotOperations.Move);

            if (isMoving)
                opers.Add(RobotOperations.DoNothing);
            //Permite que si ya se había movido un paso, no se mueva otro
            else if (Environment[CurrentPos].IsDirty())
                opers.Add(RobotOperations.CleanCell);

            return opers;
        }

        /// <summary>
        /// Se mueve aleatoriamente a una de las casillas adyacentes
        /// </summary>
        /// <param name="adjacents"> Casillas adyacentes </param>
        /// <param name="precondition"> Condiciones que tiene que cumplir la casilla para poder ejecutar el movimiento</param>
        /// <returns> True si pudo ejecutar el movimiento </returns>
        protected override bool RandomMove(HashSet<Tuple<MovementDirection, Point>> adjacents, Func<Point, bool> precondition = null)
        {
            var carrying = IsCarryingChild();
            isMoving = true;
            bool moved = false;

            var p = adjacents.FirstOrDefault(x => Environment[x.Item2].IsPlaypen());
            if (carrying && p != null) //Si tiene un niño cargado y una de las casillas adyacentes pertenece al corral
                moved = MoveTo(p.Item2);      
            else
                moved = base.RandomMove(adjacents, precondition);
            MovementCount++;
            if (carrying && Environment.InPlaypen(CurrentPos))
            {
                ReleaseChild();
                isMoving = false;
            }

            return moved;
        }

        public abstract void Play();
    }

}
