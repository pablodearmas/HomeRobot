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
    public class CleanerRobot: RandomRobot
    {
        public CleanerRobot(Environment env) : base(env)
        {

        }
        public override void Reset(Point pos)
        {
            base.Reset(pos);
        }

        //PlayCleaningFirst prioriza la limpieza antes que el movimiento
        //Obtiene las posibles operaciones a realizar que prioricen la limpieza: limpiar si la 
        //casilla está sucia y moverse a una casilla sucia. Si no hay casillas sucias alrededor 
        //del robot y carga un niño, debe moverse a la que más vecinas sucias tenga
        public override void Play()
        {
            if (Environment[CurrentPos].IsDirty())
                Clean();
            else
            {
                //Si el robot está en el corral y soltó al niño, salir del corral
                var adjacents = Environment.AdjacentCells(CurrentPos, p => Environment[CurrentPos].IsChild() &&
                                                            Environment[CurrentPos].IsPlaypen() &&
                                                            !Environment[p].IsPlaypen() &&
                                                            !Environment[p].IsObstacled());
                if (adjacents.Count > 0)
                    RandomMove(adjacents);
                else
                {
                    adjacents = Environment.AdjacentCells(CurrentPos, p => Environment[p].IsEmpty() && Environment[p].IsPlaypen());

                    if (IsCarryingChild() && adjacents.Count > 0)
                        RandomMove(adjacents);
                    else
                    {
                        adjacents = Environment.AdjacentCells(CurrentPos,
                                    p => Environment[p].IsDirty());

                        if (adjacents.Count > 0)
                            RandomMove(adjacents);
                        else
                        //Si no hay ninguna casilla adyacente sucia 
                        //se verifica si en el próximo paso puede caer en una casilla sucia     
                        {
                            //
                            var carrying = IsCarryingChild();
                            //Obtener las casillas adyacentes hacia donde se puede mover
                            adjacents = Environment.AdjacentCells(CurrentPos,
                                        p => !Environment[p].IsObstacled() &&
                                        (!Environment[p].IsChild() || !IsCarryingChild()) &&
                                        (!Environment[CurrentPos].IsPlaypen() || !IsCarryingChild() || Environment[p].IsPlaypen()));

                            var adjacentsArray = adjacents.ToArray<Tuple<MovementDirection, Point>>();

                            var dirtyNeighbors = new HashSet<Tuple<MovementDirection, Point>>[adjacentsArray.Length];

                            var maxDirt = 0;
                            var maxIndex = -1;

                            //Busca las casillas sucias alrededor de cada una de las casillas adyacentes
                            for (var i = 0; i < adjacentsArray.Length; i++)
                            {
                                var dirtyAdjacents = Environment.AdjacentCells(adjacentsArray[i].Item2,
                                        p => Environment[p].IsDirty());
                                dirtyNeighbors[i] = dirtyAdjacents;
                                if (dirtyNeighbors[i].Count > maxDirt)
                                {
                                    maxDirt = dirtyNeighbors[i].Count;
                                    maxIndex = i;
                                }
                            }

                            if (maxDirt > 0)
                            {
                                MoveTo(adjacentsArray[maxIndex].Item2); //Moverse a la casilla que tiene más vecinas sucias
                                MovementCount++;
                                if (carrying && IsCarryingChild())
                                    RandomMove(dirtyNeighbors[maxIndex], null); //Moverse aleatoriamente a una de las casillas sucias
                            }
                            else
                            {
                                //Si no había vecinas sucias para ninguno de los posibles movimientos
                                //se mueve aleatoriamente, vuelve a obtener los posibles movimientos
                                //y se vuelve a mover aleatoriamente excluyendo la posición de la que partió
                                RandomMove(adjacents, null);
                                if (carrying && IsCarryingChild())
                                {
                                    adjacents = Environment.AdjacentCells(CurrentPos, p => (p != PreviousPos) &&
                                        !Environment[p].IsObstacled() &&
                                        (!Environment[p].IsChild() || !IsCarryingChild()) &&
                                        (!Environment[CurrentPos].IsPlaypen() || !IsCarryingChild() || Environment[p].IsPlaypen()));
                                    RandomMove(adjacents, null);
                                }

                            }
                        }
                    }
                }
            }
        }

    }
}
