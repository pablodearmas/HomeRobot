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
    public class SmartRobot: CatcherRobot
    {
        public SmartRobot(Environment env) : base(env)
        {

        }

        public override void Reset(Point pos)
        {
            base.Reset(pos);
        }

        //private List<List<Point>> GetSmartDirtyPaths()
        //{
        //    var dirtyPositions = new List<Point>(Environment.DirtyCells);
        //    return Environment.SmartDirtyPaths(CurrentPos, dirtyPositions);
        //}

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
                    PrepareFinalClean(() => GetMinPathToDirty((p, plist) => Environment.SmartDirtyPaths(p, plist)));

                }
            }
            else if (minPathToDirty != null)
            {
                CrossPlaypen();
            }
            else if (Environment.DirtPercent > Environment.CriticalDirtyPercent &&
                    Environment[CurrentPos].IsDirty())
            {
                Environment.Clean(CurrentPos);
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
