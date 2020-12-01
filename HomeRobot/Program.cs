using System;
using System.Globalization;
using System.IO;
using HomeRobot.Core;
using Newtonsoft;
using Newtonsoft.Json;

namespace HomeRobot
{
    //////////////////////////////////////////////////////////////////////////////////////////////////
    ///Para situar la simulación en el mundo real se consultó la norma inglesa de espacio ocupado por 
    ///un niño en las edades pre-escolares y se tomó como referencia la indicación de que un niño ocupa
    ///0.35m2 y que necesita aproximadamente 3.5m2 para realizar sus actividades de juego y gateo. 
    ///Como un niño ocupa una casilla, la casilla tendría un área de 0.35m2 y el niño necesitaría 10 
    ///casillas para realizar sus actividades.
    ///
    /// El tamaño del ambiente, el número de niños, el % de suciedad y el % de obstáculos son los 
    /// parámetros a variar en los 10 ambientes diferentes. En tanto, en las 30 simulaciones por cada
    /// ambiente, las variaciones serán en cuanto a la disposición aleatoria en la que se sitúan los
    /// elementos del ambiente.
    /////////////////////////////////////////////////////////////////////////////////////////////////
    class Program
    {
        private const int simulationCount = 30;

        private double[] dirtPercentPerSimulation;
        private int successfullCount;
        private int robotDissmissedCount;
        private int timeoutCount;

        private EnvironmentSettings[] settings;

        //Leer las condiciones iniciales del ambiente de un archivo json
        private void GetInitialConditions(string filename)
        {
            var s = File.ReadAllText(filename);
            settings = JsonConvert.DeserializeObject<EnvironmentSettings[]>(s);
        }

        //Exportar los resultados para un archivo de excel
        //Para cada una de las 30 simulaciones que se ejecutan sobre un mismo ambiente 
        //se deben almacenar los datos para poder plotearlos
        private void ExportData(string filename)
        {
            var line = $"{successfullCount}, {robotDissmissedCount}, {timeoutCount}, {string.Join(", ", dirtPercentPerSimulation) } \n";
            File.AppendAllText(filename, line);  
        }

        private void Execute(string agentModel)
        {
            File.Delete("homerobotsim.csv");

            Core.HomeRobot robot;
            //Elegir la operación a realizar moverse o limpiar dependiendo de la estrategia.

            foreach (var setting in settings)
            {
                Console.WriteLine("Simulating: {0}", setting);

                //Ciclo de 30 simulaciones
                successfullCount = 0;
                robotDissmissedCount = 0;
                timeoutCount = 0;
                dirtPercentPerSimulation = new double[simulationCount];

                var simulationIndex = 0;
                while (simulationIndex < simulationCount)
                {
                    //Iniciar el ambiente
                    Type robotType;
                    switch (agentModel.ToLower())
                    {
                        case "random":
                            robotType = typeof(RandomRobot);
                            break;
                        case "cleaner":
                            robotType = typeof(CleanerRobot);
                            break;
                        case "catcher":
                            robotType = typeof(CatcherRobot);
                            break;
                        case "smart":
                            robotType = typeof(SmartRobot);
                            break;
                        default:
                            robotType = null;
                            break;
                    }

                    var env = new Core.Environment(setting, robotType);
                    env.Played += Env_Played;
                    env.Robot.Played += Robot_Played;

                    //Comenzar simulación
                    var simulation = new Simulation(env, env.Robot);
                    var end = simulation.Run();

                    dirtPercentPerSimulation[simulationIndex] = env.SimulationDirtPercent();
                    simulationIndex++;
                    
                    switch (end)
                    {
                        case ExecutionEnd.Succefull:
                            successfullCount++;
                            break;
                        case ExecutionEnd.RobotDismissed:
                            robotDissmissedCount++;
                            break;
                        case ExecutionEnd.Timeout:
                            timeoutCount++;
                            break;
                    }
                }
                ExportData("homerobotsim.csv");
           }
        }

        private void Robot_Played(object sender, EventArgs e)
        {
            var robot = (Core.HomeRobot)sender;
            //robot.Environment.Print();
        }

        private void Env_Played(object sender, EventArgs e)
        {
            var env = (Core.Environment) sender;
            //env.Print();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Home Robot Simulation");

            var sim = new Program();
            //Obtener el nombre del archivo de las condiciones iniciales 
            if (args.Length >= 1)
            {
                var filename = args[0];
                //Leer el archivo de configuración con las condiciones iniciales
                sim.GetInitialConditions(filename);
                
                //Ejecutar la simulación             
                sim.Execute(args[1]);
            }
 

        }      
    }
}
