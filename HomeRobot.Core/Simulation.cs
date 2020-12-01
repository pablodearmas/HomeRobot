using System;
using System.Collections.Generic;
using System.Text;

namespace HomeRobot.Core
{
    public enum ExecutionEnd { Succefull, RobotDismissed, Timeout };

    public class Simulation
    {
        

        private Environment environment;

        private HomeRobot robot;

        public Simulation(Environment env, HomeRobot bot)
        {
            environment = env;
            robot = bot;
        }

        public bool Successfull()
        {
            return environment.IsClean() && environment.AllChildrenInPlaypen() ? true : false;
        }

        public bool RobotDissmissed()
        {
            return environment.DirtPercent >= Environment.dirtTop ? true : false;
        }

        public void PlayTurn()
        {
            robot.Play();

            environment.Play();
        }

        public ExecutionEnd Run()
        {
            //Ejecutar hasta 100 unidades de cambio del ambiente 
            //si no se cumplen antes las condiciones de parada
            var changes = 0;
            var dirtPerChange = new float[Environment.MaxEnvironmentChanges];

            while (changes < Environment.MaxEnvironmentChanges * environment.TurnsToChange)
            {
                PlayTurn();

                if (Successfull())
                {
                    //environment.dirtyPercentPerEnvChange[changes% Environment.MaxEnvironmentChanges] = environment.DirtPercent;
                    environment.dirtyPercentPerEnvChange[((Core.Environment)environment).envChanges-1] = environment.DirtPercent;
                    return ExecutionEnd.Succefull;
                }
                if (RobotDissmissed())
                {
                    //environment.dirtyPercentPerEnvChange[changes% Environment.MaxEnvironmentChanges] = environment.DirtPercent;
                    environment.dirtyPercentPerEnvChange[((Core.Environment)environment).envChanges-1] = environment.DirtPercent;
                    return ExecutionEnd.RobotDismissed;
                }
                
                changes++;
            }
            return ExecutionEnd.Timeout;
        }
    }
}
