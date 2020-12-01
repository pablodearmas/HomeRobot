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
    public class CatcherRobot: CleanerRobot
    {
        public CatcherRobot(Environment env) : base(env)
        {

        }

        public override void Reset(Point pos)
        {
            base.Reset(pos);
            minPathToDirty = null;
            minPathToDirtyAcrossPlaypen = null;
            maxPathInsidePlaypen = null;
        }

        //Camino para pasar por todas las celdas sucias después de poner a los niños en el corral
        protected List<Point> minPathToDirty;

        protected List<Point> minPathToDirtyAcrossPlaypen;

        private List<Point> MinPath(List<List<Point>> paths)
        {
            var min = Environment.Length;
            var index = -1;
            //Recorrer la lista para quedarse con el camino más corto o sea con la lista de menor cantidad de elementos
            for (var i = 0; i < paths.Count; i++)
                if (min > paths[i].Count)
                {
                    min = paths[i].Count;
                    index = i;
                }
            if (index == -1)
                return null;
            return paths[index]; //Retorna el camino más corto
        }

        private List<List<Point>> GetDirtyPaths()
        {
            var dirtyPositions = new List<Point>(Environment.DirtyCells);
            return Environment.Paths(CurrentPos, dirtyPositions, p => !Environment[p].IsObstacled() &&
                                    (!Environment[p].IsPlaypen() || !Environment[p].IsChild()));
        }

        protected List<Point> GetMinPathToDirty(Func<Point, List<Point>, List<List<Point>>> paths)
        {
            var dirtyPaths = GetDirtyPaths();
            var dirtyPositions = dirtyPaths.Select(x => x.Last()).ToList();

            var minpath = MinPath(dirtyPaths);
            if (minpath != null)
            {
                dirtyPositions.Remove(minpath.Last());

                while (dirtyPositions.Count > 0)
                {
                    var leg = MinPath(paths(minpath.Last(), dirtyPositions));
                    //Environment.SmartDirtyPaths(minpath.Last(), dirtyPositions) :
                    //Environment.Paths(minpath.Last(), dirtyPositions));
                    if (leg != null)
                    {
                        minpath.AddRange(leg);
                        dirtyPositions.Remove(minpath.Last());
                    }
                    else break;
                }
            }
            return minpath;
        }

        /// <summary>
        /// Determina el niño que en menor cantidad de pasos será alcanzado por el robot y llevado al corral
        /// </summary>
        /// <param name="robotPos"> Posición del robot</param>
        /// <returns></returns>
        private List<Point> NearestChild(List<List<Point>> paths)
        {
            var fullPaths = new List<List<Point>>();

            //Buscar el camino más corto de cada niño al corral
            foreach (var path in paths)
            {
                //Obtiene el primero de los caminos más cortos al corral
                if (path.Count > 0)
                {
                    var ppPath = MinPathToPlaypen(path.Last());
                    if (ppPath != null && ppPath.Count > 0)
                    {
                        path.AddRange(ppPath);
                        fullPaths.Add(path);
                    }
                }
            }

            //Ordenar la lista de caminos según la cantidad de nodos
            var orderPaths = fullPaths.OrderBy(x => x.Count);

            return orderPaths.FirstOrDefault();
        }

        /// <summary>
        /// Caminos del robot a cada uno de los niños
        /// </summary>
        /// <param name="robotPos"> Posición del robot </param>
        /// <returns> Lista de lista de nodos. Cada camino del robot a un niño es una lista de nodos </returns>
        private List<List<Point>> PathToChildren()//Point robotPos)
        {
            var chPositions = Environment.GetChildrenPos();
            return Environment.Paths(CurrentPos, chPositions);
        }

        /// <summary>
        /// Camino más corto desde el niño al corral
        /// </summary>
        /// <param name="chPos"></param>
        /// <returns></returns>
        protected List<Point> MinPathToPlaypen(Point chPos, Func<Point, bool> rule = null)
        {
            var emptyPlaypen = Environment.GetEmptyPlaypen(p =>
                Environment[p].CurrentStatus == CellStatus.Empty ||
                Environment[p].CurrentStatus == CellStatus.Robot); //Lista de las casillas del corral que están vacías
            var paths = (List<List<Point>>)Environment.Paths(chPos, emptyPlaypen, rule);
            return MinPath(paths);
        }

        protected List<Point> MaxPath(List<List<Point>> paths)
        {
            var max = 0;
            var index = -1;
            //Recorrer la lista para quedarse con el camino más largo o sea con la lista de mayor cantidad de elementos
            for (var i = 0; i < paths.Count; i++)
                if (max < paths[i].Count)
                {
                    max = paths[i].Count;
                    index = i;
                }
            if (index == -1)
                return null;
            return paths[index]; //Retorna el camino más largo
        }

        private List<Point> maxPathInsidePlaypen;

        /// <summary>
        /// Camino desde la posición del corral en que está el robot y las celdas bloqueadas del corral
        /// </summary>
        /// <param name="chPos"></param>
        /// <returns></returns>
        private List<Point> MaxPathInsidePlaypen
        {
            get
            {
                if (maxPathInsidePlaypen == null)
                {
                    var playpen = Environment.GetEmptyPlaypen();
                    var paths = (List<List<Point>>)Environment.Paths(CurrentPos, playpen, p => Environment[p].IsPlaypen() && Environment[p].IsEmpty());
                    maxPathInsidePlaypen = MaxPath(paths);
                }
                return maxPathInsidePlaypen;
            }

        }

        protected bool PrepareFinalClean(Func<List<Point>> getMinPathToDirty)
        {
            if (minPathToDirty == null)
                minPathToDirty = getMinPathToDirty();

            if (minPathToDirty != null && minPathToDirty.Count > 0)
            {
                MoveTo(minPathToDirty[0]);
                MovementCount++;
                minPathToDirty.RemoveAt(0);
                if (minPathToDirty.Count == 0)
                    minPathToDirty = null;
            }
            else
                return false;
            return true;
        }

        protected void CrossPlaypen()
        {
            var inPlaypenPos = CurrentPos;
            MoveTo(minPathToDirty[0]);
            MoveTo(inPlaypenPos);
            ReleaseChild();
            MovementCount += 2;
        }

        //Busca el niño más cerca y da el primer paso hacia él
        protected void InitiateMovementToNearChild()
        {
            List<Point> nearPath = null;
            nearPath = NearestChild(PathToChildren());

            if (nearPath != null && nearPath.Count > 0)
            {
                var pos = nearPath[0];
                MoveTo(pos);
                MovementCount++;
            }
        }

        //Prepara un camino dentro del corral para mover al niño que acaba de entrar a 
        //éste, a la posición más alejada del punto de entrada y realiza dos movimientos
        //debido a que este método se ejecuta cuando el robot está dentro del corral con el 
        //niño cargado
        protected void MoveInsidePlaypen()
        {
            //Si no existe el camino a la casilla del corral más alejada, lo calcula
            if (MaxPathInsidePlaypen?.Count > 0)
            {
                MoveTo(MaxPathInsidePlaypen[0]);
                MaxPathInsidePlaypen.RemoveAt(0);
                MovementCount++;
            }
            if (MaxPathInsidePlaypen?.Count > 0)
            {
                MoveTo(MaxPathInsidePlaypen[0]);
                MaxPathInsidePlaypen.RemoveAt(0);
                MovementCount++;
            }
            //Si no hay ninguna casilla del corral bloqueada, soltar al niño
            if (MaxPathInsidePlaypen == null || MaxPathInsidePlaypen.Count == 0)
            {
                ReleaseChild();
                maxPathInsidePlaypen = null;
            }
        }

        protected void MoveToPlaypen(List<Point> path)
        {
            MoveTo(path[0]);
            MovementCount++;

            var stepCount = 1;
            if (path.Count >= 2)
            { //No ha llegado al corral
                MoveTo(path[1]); 
                MovementCount++;
                ++stepCount;
            }
            if (Environment.InPlaypen(CurrentPos))
            {
                if (MaxPathInsidePlaypen?.Count > 0 && stepCount == 1)
                {
                    MoveTo(MaxPathInsidePlaypen[0]);
                    MaxPathInsidePlaypen.RemoveAt(0);
                    MovementCount++;
                }
                //Si no hay ninguna casilla del corral a la que moverse, soltar al niño
                if (MaxPathInsidePlaypen == null || MaxPathInsidePlaypen.Count == 0)
                {
                    ReleaseChild();
                    maxPathInsidePlaypen = null;
                }
            }
        }

        public override void Play()
        {
            if (Environment.AllChildrenInPlaypen())
            {
                //Limpiar la suciedad
                if (Environment[CurrentPos].IsDirty())
                    Clean();
                else
                {
                    //Se está pasando como parámetro una función que retorna la lista de puntos para 
                    //recorrer todos las casillas sucias. A su vez esta función llama a la función 
                    //GetMinPathToDirty que recibe también como parámetro una función con la estrategia 
                    //para escoger los caminos entre una casilla y una lista de casillas
                    PrepareFinalClean(() => GetMinPathToDirty((p, plist) => Environment.Paths(p, plist)));
                }
            }
            else if (minPathToDirty != null)
            {
                CrossPlaypen();
            }
            else if (!IsCarryingChild())
            {
                InitiateMovementToNearChild();
            }
            else
            {
                //Si tiene un niño cargado y está en el corral
                if (Environment.InPlaypen(CurrentPos))
                {
                    MoveInsidePlaypen();
                }
                else //Si tiene un niño cargado y no está en el corral
                {
                    //Moverse por el camino más cercano al corral dos pasos
                    var path = MinPathToPlaypen(CurrentPos, p => !Environment[p].IsObstacled() && !Environment[p].IsChild());
                    if (path != null)
                    {
                        MoveToPlaypen(path);
                    }
                    else //Si el camino está bloqueado ejecuta operación del robot limpiador
                        base.Play();
                }

            }
        }

    }
}
