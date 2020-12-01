using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Reflection;
using Troschuetz.Random.Distributions.Discrete;

namespace HomeRobot.Core
{
    //Posibles direcciones para el movimiento
    //Los niños se mueven un solo paso: Up, Down, Left y Right
    //El robot si está solo se mueve 1 paso: Up, Down, Left y Right
    //y cuando carga un niño se puede mover hasta dos pasos: DoubleUp, DoubleDown, DoubleLeft y Double Right, 
    //además de Up, Down, Left y Right
   // public enum MovementDirection { Up, Down, Left, Right, DoubleUp, DoubleDown, DoubleLeft, DoubleRight };

    public class Environment
    {
        //Desplazamientos para el cálculo de las casillas adyacentes
        public static Point[] offsets = new Point[] {
            new Point(-1,0),
            new Point(1,0),
            new Point(0,-1),
            new Point(0,1)
        };

        protected DiscreteUniformDistribution randomXPos;
        protected DiscreteUniformDistribution randomYPos;
        protected DiscreteUniformDistribution randomDirt;
        protected DiscreteUniformDistribution randomMov;
        protected DiscreteUniformDistribution randomGrid;

        public const int MaxEnvironmentChanges = 100;

        #region Parámetros del ambiente

        //Tamaño de cuadrícula predefinido para crear suciedad después del movimiento de un niño
        private Point GridSize = new Point(3,3);

        public double[] dirtyPercentPerEnvChange = new double[MaxEnvironmentChanges];

        public int envChanges = 1;

        public double SimulationDirtPercent()
        {
            double acc = 0;
            if (envChanges > MaxEnvironmentChanges)
                envChanges = MaxEnvironmentChanges;

            for (var i = 0; i < envChanges; i++)
                acc += dirtyPercentPerEnvChange[i];
            return acc / (double)envChanges;
        }

        //Suciedad que se genera según el número de niños en una cuadrícula
        //0 si no hay niños, 0-1 cuando hay 1 niño, 0-3 cuando hay 2, 0-6 cuando hay 3 o más
        public static int[] dirtPerGrid = { 0, 1, 3, 6 };

        //Porciento de casillas vacías a partir del cual se cosidera que la casa está sucia
        public const int dirtTop = 60;

        //Porciento de suciedad al iniciar la simulación
        private double initialDirtPercent;

        //Cantidad de suciedad al iniciar la simulación. Se calcula a partir del initialDirtPercent
        private int initialDirtCount;

        public int DirtCount { get; private set; }

        public int CleanCount { get; private set; }

        public const double CriticalDirtyPercent = 45;

        //Porciento de casillas vacías que están sucias
        public double DirtPercent 
        {
            get
            {
                return dirtyCells.Count * 100 / (double) emptyCount;
            }
        }

        //Número de casillas vacías inicializa con el tamaño del ambiente - el tamaño del corral - # de obstáculos
        //se modifica cuando se insertan los niños y el robot y cuando se genera suciedad
        private int emptyCount;

        public int EmptyCount
        {
            get { return emptyCount; }
            private set { emptyCount = value; }
        }

        //Dimensiones del ambiente
        public Point Dimensions { get; private set; }

        //Número de turnos entre los cambios naturales del ambiente
        public int TurnsToChange { get; private set; }

        //Número de jugadas (turnos) realizadas
        private int turns;

        //Número de obstáculos en el ambiente, calculado a partir del porciento
        private int obstacleCount;

        //Porciento de la casillas del ambiente que tienen obstáculos
        public double ObstaclePercent { get; private set; }

        //Cantidad de niños en el ambiente
        public int ChildrenCount { get; private set; } 
        #endregion

        //Todas las casillas del ambiente
        private Cell[,] cells;

        //Indice de la clase para obtener la casilla i,j
        public Cell this[int i, int j]
        {
            get { return (i >= 0 && i < Dimensions.X && j >= 0 && j < Dimensions.Y) ? cells[i, j] : null; }
            set 
            { 
                if (i >= 0 && i < Dimensions.X && j >= 0 && j < Dimensions.Y) 
                    cells[i, j] = value; 
            }
        }

        //Indice de la clase para obtener la casilla a partir de un punto x,y
        public Cell this[Point pos]
        {
              get 
            { 
                var x = pos.X;
                var y = pos.Y;
                return (x >= 0 && x < Dimensions.X && y >= 0 && y < Dimensions.Y) ? cells[x,y] : null; }
            set
            {
                var x = pos.X;
                var y = pos.Y;
                if (x >= 0 && x < Dimensions.X && y >= 0 && y < Dimensions.Y)
                    cells[x, y] = value;
            }
        }
        
        //Arreglo de celdas que conforman la cuna
        private Point[] playpen;

        //Lista de niños
        private List<Child> children;

        public IEnumerable<Child> Children => children.AsEnumerable();

        private List<Point> dirtyCells;

        public List<Point> DirtyCells 
        { 
            get { return dirtyCells; } 
        }

        //Agente (Robot)
        public HomeRobot Robot { get; private set; }
 
        #region Miembros privados
        //Conjunto de las posibles direcciones para obtener una casilla adyacente
        private HashSet<MovementDirection> Directions(Point pos)
        {
            var dir = new HashSet<MovementDirection>();
            if ((pos.X > 0) && (!playpen.Contains<Point>(new Point(pos.X-1, pos.Y))))
                 dir.Add(MovementDirection.Up);

            if ((pos.X < Dimensions.X-1) && (!playpen.Contains<Point>(new Point(pos.X + 1, pos.Y))))
                dir.Add(MovementDirection.Down);
            
            if ((pos.Y > 0) && (!playpen.Contains<Point>(new Point(pos.X, pos.Y-1))))
                dir.Add(MovementDirection.Left);

            if ((pos.Y < Dimensions.Y - 1) && (!playpen.Contains<Point>(new Point(pos.X, pos.Y+1))))
                dir.Add(MovementDirection.Right);

            return dir;
        }

        //Genera aleatoriamente una casilla para situar algún elemento del ambiente
        private Point RandomPos()
        {
            return new Point(randomXPos.Next(), randomYPos.Next());
            //return new Point(n / Dimensions.Y, n % Dimensions.Y);
            //return new Point(randomObj.RandomNumber(0, Dimensions.X), randomObj.RandomNumber(0, Dimensions.Y));
        }

        #endregion

        public Environment(EnvironmentSettings settings, Type robotType): 
            this(settings.Rows, settings.Cols, settings.Turns, 
                settings.DirtPercent, settings.ObstaclePercent, 
                settings.ChildrenCount, robotType)
        {
        }

       /// <summary>
       /// Constructor que recibe los parámetros de inicialización del ambiente
       /// </summary>
       /// <param name="rows"> Número de filas </param>
       /// <param name="cols"> Número de columnas </param>
       /// <param name="t"> Número de turnos que transcurren entre un cambio del ambiente y el próximo </param>
       /// <param name="dirtPercent"> Porciento de celdas sucias </param>
       /// <param name="obstaclePercent"> Porciento de obstáculos </param>
       /// <param name="childrenCount"> Número de niños </param>
        public Environment(int rows, int cols, int t, double dirtPercent, double obstaclePercent, int childrenCount, Type robotType)
        {
            randomDirt = new DiscreteUniformDistribution(0, 6);
            randomMov = new DiscreteUniformDistribution(0, 3);
            randomGrid = new DiscreteUniformDistribution(0, 7);

            //Si dirtPercent >= 60% generar una excepción porque el ambiente inicial no sería factible
            if (dirtPercent >= dirtTop)
                throw new Exception("Los parámetros de inicialización no permiten crear un ambiente factible.");

            Dimensions = new Point(rows, cols);
            //Crear las cuadrículas del ambiente
            cells = new Cell[rows, cols];
            for (var i = 0; i < rows; i++)
                for (var j = 0; j < cols; j++)
                    cells[i, j] = new Cell(new Point(i,j));

            randomXPos = new DiscreteUniformDistribution(0, rows-1);
            randomYPos = new DiscreteUniformDistribution(0, cols-1);

            TurnsToChange = t;
            envChanges = 1;

            ChildrenCount = childrenCount;
            //Crear el corral
            CreatePlaypen();

            ObstaclePercent = obstaclePercent;
            //Calcular el número de obstáculos a partir del %. Se escoge el entero <= que el valor
            obstacleCount = (int)Math.Floor(obstaclePercent * (Length - playpen.Length) / 100);

            //Calcular el número de casillas vacías
            EmptyCount = Length - obstacleCount - playpen.Length;

            //Guardar el % inicial de suciedad por si se necesita para las estadísticas
            initialDirtPercent = dirtPercent;

            //Calcular el número de casillas sucias a partir del % y el número de casillas vacías.
            //Se escoge el entero <= que el valor
            initialDirtCount = (int)Math.Floor(dirtPercent * EmptyCount / 100);
            int cleanCount = EmptyCount - initialDirtCount;

            //La cantidad de casillas limpias debe permitir ubicar a los niños y poder hacer al menos un movimiento
            if (cleanCount - childrenCount <= 0)
                throw new Exception("Los parámetros de inicialización no permiten crear un ambiente factible.");

            //Generar los obstáculos
            GenerateInitialObstacles(obstacleCount);            
            
            //Generar la suciedad inicial a partir del parámetro
            GenerateInitialDirt(initialDirtCount);
            DirtCount = initialDirtCount;
            CleanCount = 0;

            //Generar posiciones aleatorias para los niños
            CreateChildren();

            var ctor = robotType.GetConstructor(new[] { typeof(Environment) });
            var robot = (HomeRobot)ctor.Invoke(new[] { this });
            //Generar aleatoriamente la posición del robot en una casilla vacía dentro de los límites del ambiente\
            SetRobot(robot);
        }

        //Transformaciones que sufre el ambiente cada t turnos
        public void Reset()
        {
            for (var i = 0; i < Dimensions.X; i++)
                for (var j = 0; j < Dimensions.Y; j++)
                    cells[i, j].Reset();

            CreatePlaypen();

            //Calcular el número de casillas vacías
            EmptyCount = Length - obstacleCount - playpen.Length;

            //Generar los obstáculos
            GenerateInitialObstacles(obstacleCount);

            //Generar la suciedad inicial a partir del parámetro
            GenerateInitialDirt(initialDirtCount);
            DirtCount = initialDirtCount;
            CleanCount = 0;

            //Generar posiciones aleatorias para los niños
            ResetChildren();

            //Generar aleatoriamente la posición del robot en una casilla vacía dentro de los límites del ambiente\
            ResetRobot();

            if (Changed != null)
                Changed(this, null);
        }

        //Para probar
        public Environment(int rows, int cols, int childrenCount)
        {
            Dimensions = new Point(rows, cols);
            //Crear las cuadrículas del ambiente
            cells = new Cell[rows, cols];
            for (var i = 0; i < rows; i++)
                for (var j = 0; j < cols; j++)
                    cells[i, j] = new Cell(new Point(i, j));

            ChildrenCount = childrenCount;

            children = new List<Child>();
            var bb = new Child(this, new Point(0, 0));
            children.Add(bb);
            this[0, 0].SetMobileObject(CellStatus.Child, bb);

            cells[0, 1].SetMobileObject(CellStatus.Obstacle, new Obstacle(this, new Point(0, 1)));
            cells[0, 2].SetMobileObject(CellStatus.Obstacle, new Obstacle(this, new Point(0, 2)));
            cells[1, 0].SetMobileObject(CellStatus.Obstacle, new Obstacle(this, new Point(1, 0)));
            cells[2, 0].SetMobileObject(CellStatus.Obstacle, new Obstacle(this, new Point(2, 0)));
            cells[3, 0].SetMobileObject(CellStatus.Obstacle, new Obstacle(this, new Point(3, 0)));

            //bb.Play();
        }

        /// <summary>
        /// Comprueba si la posición se encuentra dentro del ambiente
        /// </summary>
        /// <param name="pos"> Posición </param>
        /// <returns> true si está fuera del ambiente </returns>
        public bool OutOfBoundaries(Point pos)
        {
            if (pos.X < 0 || pos.X >= Dimensions.X || pos.Y < 0 || pos.Y >= Dimensions.Y)
                return true;
            return false;
        }

        public bool IsBoundary(Point pos)
        {
            if (pos.X == 0 || pos.X == Dimensions.X || pos.Y == 0 || pos.Y == Dimensions.Y)
                return true;
            return false;
        }

        //Determina si el ambiente está limpio
        public bool IsClean()
        {
           return dirtyCells.Count == 0;
        }

        /// <summary>
        /// Limpia una casilla
        /// </summary>
        /// <param name="c"> Casilla a limpiar </param>
        public void Clean(Point pos)
        {
            this[pos].Clean();
            dirtyCells.Remove(pos);
            CleanCount++;
        }

        /// <summary>
        /// Crea el corral
        /// </summary>
        private void CreatePlaypen()
        {
            //Crear el corral que tenga una casilla para cada niño. 
            playpen = new Point[ChildrenCount];

            //Las casillas deben ser adyacentes
            //Generar aleatoriamente la primera casilla del corral
            Point pos = RandomPos();
            
            var i = 0;
            playpen[i] = pos;
            this[pos].CurrentStatus = CellStatus.Empty;
            this[pos].PlaypenIndex = i++; //Guarda la posición de la casilla en el corral

            //Generar el resto de las casillas del corral  
            while (i < ChildrenCount)
            {
                //Obtener los lados libre de la casilla
                var adjacents = AdjacentCells(pos, p =>!this[p].IsPlaypen());
                //Decidir aleatoriamentes hacia cuál de los lados libres se va a crear la nueva casilla del corral
                if (adjacents.Count > 0)
                {
                    var n = randomMov.Next() % adjacents.Count;
                    pos = adjacents.ToArray<Tuple<MovementDirection, Point>>()[n].Item2;
                }
                else
                {
                    var j = i - 2;

                    while (adjacents.Count == 0 && j >= 0)
                    {
                        pos = playpen[j--];
                        adjacents = AdjacentCells(pos, p => !this[p].IsPlaypen());
                    }

                    if (adjacents.Count > 0)
                    {
                        var n = randomMov.Next() % adjacents.Count;
                        pos = adjacents.ToArray<Tuple<MovementDirection, Point>>()[n].Item2;
                    }

                }

                //Adicionar la casilla al corral
                playpen[i] = pos;  //La próxima casilla se genera a partir de ésta
                this[pos].CurrentStatus = CellStatus.Empty;
                this[pos].PlaypenIndex = i++; //Guarda la posición de la casilla en el corral
            }
        }
      
        /// <summary>
        /// Determina si una posición pertenece al corral
        /// </summary>
        /// <param name="pos"> Posición </param>
        /// <returns> true si la posición está en el corral </returns>
        public bool InPlaypen(Point pos)
        {
           return playpen.Contains<Point>(pos) ?  true : false;
        }

        /// <summary>
        /// Genera la suciedad inicial
        /// </summary>
        /// <param name="dirtCount"> Número de casillas sucias </param>
        private void GenerateInitialDirt(int initialDirtCount)
        {
            dirtyCells = new List<Point>();
            var i = 0;
            Point pos;
            while (i < initialDirtCount)
            {
                pos = RandomPos();

                //Si la posiçión generada cae dentro del corral u otra casilla sucia, no se cuenta
                if (!this[pos].IsPlaypen() &&
                    !this[pos].IsDirty() &&
                    !this[pos].IsObstacled() &&
                    AdjacentCells(pos, x => !this[x].IsObstacled() && !this[x].IsPlaypen()).Count > 0)
                {
                    this[pos].Dirty();
                    dirtyCells.Add(pos);
                    i++;
                }
            };
        }

        /// <summary>
        /// Genera los obstáculos iniciales
        /// </summary>
        /// <param name="obstacleCount"> Número de obstáculos </param>
        private void GenerateInitialObstacles(int obstacleCount)
        {
            var i = 0;
            Point pos;
            while (i < obstacleCount)
            {
                pos = RandomPos();

                //Si la posiçión generada cae dentro del corral, en una casilla sucia 
                //u otro obstáculo, no se cuenta 
                if (!this[pos].IsPlaypen() && 
                    !this[pos].IsObstacled())
                {
                    this[pos].SetMobileObject(CellStatus.Obstacle, new Obstacle(this, pos));
                    i++;
                }
            };
        }

        //Crea los niños y los ubica aleatoriamente en casillas del ambiente
        private void CreateChildren()
        {
            children = new List<Child>();
            var i = 0;
            Point pos;
            while (i < ChildrenCount)
            {
                pos = RandomPos();
                if (!this[pos].IsPlaypen() && this[pos].IsEmpty())
                {
                    //Crear el objeto Child e insertarlo en la lista
                    var ch = new Child(this, pos);
                    children.Add(ch);     
                    this[pos].SetMobileObject(CellStatus.Child, ch);

                    //EmptyCount--;
                    i++;
                }

            }
        }

        //Crea los niños y los ubica aleatoriamente en casillas del ambiente
        private void ResetChildren()
        {
            var i = 0;
            Point pos;
            while (i < ChildrenCount)
            {
                pos = RandomPos();
                if (!this[pos].IsPlaypen() && this[pos].IsEmpty())
                {
                    //Crear el objeto Child e insertarlo en la lista
                    children[i].Reset(pos);
                    this[pos].SetMobileObject(CellStatus.Child, children[i]);
                    i++;
                }

            }
        }

        //Crea el robot y lo ubica aleatoriamente en una casilla del ambiente
        private void SetRobot(HomeRobot robot)
        {
            Point pos;

            while (Robot == null)
            {
                pos = RandomPos();
                if (!InPlaypen(pos) && 
                    (this[pos].CurrentStatus == CellStatus.Empty || this[pos].CurrentStatus == CellStatus.Dirt) &&
                    AdjacentCells(pos, x => !this[x].IsObstacled() && !this[x].IsPlaypen()).Count > 0)
                {
                    Robot = robot;
                    Robot.Reset(pos);
                    this[pos].SetMobileObject(CellStatus.Robot, Robot);
                }
            }
        }

        private void ResetRobot()
        {
            while (true)
            {
                var pos = RandomPos();
                if (!InPlaypen(pos) &&
                    (this[pos].CurrentStatus == CellStatus.Empty || this[pos].CurrentStatus == CellStatus.Dirt) &&
                    AdjacentCells(pos, x => !this[x].IsObstacled() && !this[x].IsPlaypen()).Count > 0)
                {
                    Robot.Reset(pos);
                    this[pos].SetMobileObject(CellStatus.Robot, Robot);
                    break;
                }
            }
        }

        //Devuelve una lista con las casillas que rodean a la posición dada, descartando las que 
        //están fuera del ambiente
        public List<Point> Grid(Point pos)
        {
            var grid = new List<Point>();
            for (var i = pos.X - 1; i <= pos.X + 1; i++)
                for (var j = pos.Y - 1; j <= pos.Y + 1; j++)
                    if (((i >= 0 && i < Dimensions.X) && (j >= 0 && j < Dimensions.Y)))/* &&
                        ((i != pos.X) || (j!= pos.Y)))*/
                        grid.Add(new Point(i, j));
            return grid;
        }

        //Elimina las casillas que no están limpias
        public List<Point> CleanGrid(List<Point> grid)
        {
            var noEmpty = new List<Point>();
            foreach (var g in grid)
                if (!this[g].IsEmpty() || InPlaypen(g))
                    noEmpty.Add(g);
            foreach (var g in noEmpty)
                grid.Remove(g);
            return grid;
        }

        //Cuenta el número de niños que hay en la cuadrícula
        public int ChildrenInGrid(List<Point> grid)
        {
            var result = 0;
            foreach (var g in grid)
                if (this[g].IsChild())
                    result++;
            return result;
        }

        /// <summary>
        /// Genera entre 0 y maxCount casillas sucias en la cuadrícula
        /// </summary>
        /// <param name="maxCount"> Máximo número de casillas sucias a generar </param>
        /// <param name="grid"> Cuadrícula donde se va a generar la suciedad </param>
        /// <returns> Número de casillas sucias generadas </returns>
        public int GenDirt(int maxCount, List<Point> grid)
        {
            if ((maxCount == 0) || (grid.Count == 0))
                return 0;
            var result = 0;
            var count = randomDirt.Next() % (maxCount + 1);
            while (result < count)
            {
                var p = grid[randomGrid.Next() % grid.Count];
                this[p].Dirty();
                dirtyCells.Add(p);
                grid.Remove(p);
                result++;
            }
            DirtCount += result;
            return result;
        }

        //Genera todas las cuadrículas de 3x3 dentro del ambiente que contienen la casilla pos 
        //y no contienen la casilla excludePos
        public List<List<Point>> Grids(Point pos, Point excludePos)
        {
            var gridList = new List<List<Point>>();

            for (var k = 0; k < GridSize.X; k++ )
                //No generar cuadrículas cuyas filas se salgan de los límites de ambiente
                if (pos.X - k >= 0 && pos.X - k + GridSize.X - 1 <= Dimensions.X - 1)
                    for (var l = 0; l < GridSize.Y; l++)

                        //No generar cuadrículas cuyas columnas estén fuera de los límites del ambiente
                        //y que contengan la casilla que se quiere excluir
                        if ((pos.Y - l >= 0) && (pos.Y - l + GridSize.Y - 1 <= Dimensions.Y - 1) &&
                            (excludePos.X >= pos.X - k) && (excludePos.X <= pos.X - k + GridSize.X - 1) &&
                            (excludePos.Y >= pos.Y - l) && (excludePos.Y <= pos.Y - l + GridSize.Y - 1))
                        {
                            var grid = new List<Point>();
                            for (var i = 0; i < GridSize.X; i++)
                                for (var j = 0; j < GridSize.Y; j++)
                                    grid.Add(new Point(pos.X - k + i, pos.Y - l + j));
                            gridList.Add(grid);
                        }
            return gridList;
        }

        //Distancia mínima entre dos casillas
        private int DistanceToChild(Point pos1, Point pos2)
        {
             return Math.Abs(pos1.X - pos2.X) + Math.Abs(pos1.Y - pos2.Y);
        }
       
        //Verifica si todos los niños están en el corral
        public bool AllChildrenInPlaypen()
        {
            foreach (var bb in children)
                if (bb.Status != ChildStatus.Penned)
                    return false;
            return true;
        }

        public event EventHandler Played;
        public event EventHandler Changed;

        //Turno de ejecución del ambiente
        public void Play()
        {
            turns++;

            //Ejecutar el turno de cada niño.
            //Pudiera hacerse aleatorio el orden con que se ejecuta
            foreach (var bb in children)
                bb.Play();

            //Chequear si es el momento de la variación aleatoria del ambiente
            if (turns % TurnsToChange == 0)
            {
                dirtyPercentPerEnvChange[envChanges - 1] = DirtPercent;
                envChanges++;
                Reset();
            }

            if (Played != null)
                Played(this, null);
        }

        /// <summary>
        /// Determina si se puede mover desde una posición en una dirección
        /// </summary>
        /// <param name="pos"> Posición </param>
        /// <param name="op"> Dirección del movimiento </param>
        /// <param name="rule"> Condiciones que debe cumplir la casilla destino para poder realizar el movimiento</param>
        /// <returns></returns>
        public Tuple<MovementDirection, Point> CanVisit(Point pos, MovementDirection op, Func<Point, bool> rule)
        {
            var newPos = new Point(pos.X + Environment.offsets[(int)op].X,
                                pos.Y + Environment.offsets[(int)op].Y);


            return (!OutOfBoundaries(newPos) && rule(newPos)) ? new Tuple<MovementDirection, Point>(op, newPos) : null;
        }

        /// <summary>
        /// Celdas adyacentes que pueden se visitadas desde la posición pos
        /// </summary>
        /// <param name="pos"> Posición inicial </param>
        /// <param name="rule"> Condiciones que debe cumplir la casilla destino para poder realizar el movimiento </param>
        /// <returns> Conjunto de tuplas con la dirección del movimiento y la coordenadas de la casilla </returns>
        public HashSet<Tuple<MovementDirection, Point>> AdjacentCells(Point pos, Func<Point, bool> rule)
        {
            var adjacents = new HashSet<Tuple<MovementDirection, Point>>();
           
            var tuple = CanVisit(pos, MovementDirection.Up, rule);
            if (tuple != null)
                adjacents.Add(tuple);

            tuple = CanVisit(pos, MovementDirection.Down, rule);
            if (tuple != null)
                adjacents.Add(tuple);

            tuple = CanVisit(pos, MovementDirection.Left, rule);
            if (tuple != null)
                adjacents.Add(tuple);

            tuple = CanVisit(pos, MovementDirection.Right, rule);
            if (tuple != null)
                adjacents.Add(tuple);

            return adjacents;
        }

        //Posición en el ambiente de cada niño que no está en el corral
        public List<Point> GetChildrenPos()
        {
            var pos = new List<Point>();
            foreach (var ch in children)
                if (!this[ch.CurrentPos].IsPlaypen())
                    pos.Add(ch.CurrentPos);
            return pos;
        }

        //Lista de las casillas del corral que no están ocupadas
        public List<Point> GetEmptyPlaypen(Func<Point, bool> rule = null)
        {
            if (rule == null)
                rule = p => this[p].CurrentStatus == CellStatus.Empty;

            var emptyPlaypen = new List<Point>();

            foreach (var pos in playpen)
                if (rule(pos))
                    emptyPlaypen.Add(pos);

            return emptyPlaypen;
        }

        //Número de casillas del ambiente
        public int Length { get { return Dimensions.X * Dimensions.Y; } }

        public List<List<Point>> SmartDirtyPaths(Point from, List<Point> to)
        {
            Func<Point, bool> rule1 = 
                p => !this[p].IsObstacled() && 
                        (!this[p].IsPlaypen() || 
                            AdjacentCells(p, x => !this[x].IsObstacled() && !this[x].IsPlaypen()).Count > 0);

            Func<Point, bool> rule2 =
                p => !this[p].IsObstacled() && !this[p].IsPlaypen();

            to = to.ToList(); //Crea una copia

            var visitNodes = new List<Node>();

            var paths = new List<List<Point>>();
            //Inserta el nodo de partida en la lista de nodos visitados
            visitNodes.Add(new Node() { pos = from, previuos = null });
            var i = 0;
            while (i < visitNodes.Count)
            {
                //Buscar las casillas hacia donde se puede mover                
                var adjacents = AdjacentCells(visitNodes[i].pos, this[visitNodes[i].pos].IsPlaypen() ? rule2 : rule1);

                foreach (var p in adjacents)
                {
                    var node = new Node()
                    {
                        pos = p.Item2,
                        previuos = visitNodes[i]
                    };

                    //Si el nodo no se encontraba en la lista de nodos lo adiciona
                    //Esto es para evitar la circularidad
                    var visited = visitNodes.FirstOrDefault(x => x.pos == node.pos);
                    if (visited == null)
                        visitNodes.Add(node);

                    var destIndex = to.IndexOf(node.pos);
                    if (destIndex != -1) //Si la casilla corresponde a una de las de destino
                    {
                        //Buscar todo el camino desde el destino al origen
                        var path = new List<Point>();
                        do
                        {
                            path.Add(node.pos);
                            node = node.previuos;
                        }
                        while (node.previuos != null);

                        //path.RemoveAt(path.Count - 1);
                        path.Reverse(); //Se invierte la lista para tener como primer punto el origen
                        paths.Add(path);

                        //Quitar la casilla de destino encontrada de la lista 
                        to.RemoveAt(destIndex);
                    }

                }
                i++;
            }
            return paths;
        }

        public List<List<Point>> Paths(Point from, List<Point> to, Func<Point, bool> rule = null)
        {
            if (rule == null)
                rule = p => !this[p].IsObstacled() && (!this[p].IsPlaypen() || !this[p].IsChild());

            to = to.ToList(); //Crea una copia

            var visitNodes = new List<Node>();

            var paths = new List<List<Point>>();
            //Inserta el nodo de partida en la lista de nodos visitados
            visitNodes.Add(new Node() { pos = from, previuos = null });
            var i = 0;
            while (i < visitNodes.Count)
            {
                //Buscar las casillas hacia donde se puede mover
                var adjacents = AdjacentCells(visitNodes[i].pos, rule);

                foreach (var p in adjacents)
                {
                    var node = new Node()
                    {
                        pos = p.Item2,
                        previuos = visitNodes[i]
                    };

                    //Si el nodo no se encotraba en la lista de nodos lo adiciona
                    //Esto es para evitar la circularidad
                    var visited = visitNodes.FirstOrDefault(x => x.pos == node.pos);
                    if (visited == null)
                        visitNodes.Add(node);

                    var destIndex = to.IndexOf(node.pos);                    
                    if (destIndex != -1) //Si la casilla corresponde a una de las de destino
                    {
                        //Buscar todo el camino desde el destino al origen
                        var path = new List<Point>();
                        do
                        {
                            path.Add(node.pos);
                            node = node.previuos;
                        }
                        while (node.previuos != null);

                        //path.RemoveAt(path.Count - 1);
                        path.Reverse(); //Se invierte la lista para tener como primer punto el origen
                        paths.Add(path);

                        //Quitar la casilla de destino encontrada de la lista 
                        to.RemoveAt(destIndex);
                    }

                }
                i++;
            }
            return paths;
        }

        public void Print()
        {
            for (int i = 0; i < Dimensions.X; i++)
            {
                for (int j = 0; j < Dimensions.Y; j++)
                {
                    var status = cells[i, j].CurrentStatus;
                    if (status == CellStatus.Empty)
                        if (InPlaypen(new Point(i, j)))
                            Console.Write(" PE  ");
                        else
                            Console.Write(" E   ");
                    else if (status == CellStatus.Obstacle)
                        Console.Write(" O   ");
                    else if (status == CellStatus.Dirt)
                        Console.Write(" D   ");
                    else if (status == CellStatus.Child)
                        if (InPlaypen(new Point(i, j)))
                            Console.Write(" PB  ");
                        else
                            Console.Write(" B   ");
                    else if (status == CellStatus.Robot)
                        if (InPlaypen(new Point(i, j)))
                            Console.Write(" PR  ");
                        else
                            Console.Write(" R   ");
                    else if (status == (CellStatus.Robot | CellStatus.Dirt))
                        Console.Write(" RD  ");
                    else if (status == (CellStatus.Robot | CellStatus.Child))
                        if (InPlaypen(new Point(i, j)))
                            Console.Write(" PRB  ");
                        else
                            Console.Write(" RB   ");
                    else if (status == (CellStatus.Robot | CellStatus.Child | CellStatus.Dirt))
                        Console.Write(" RBD ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}
