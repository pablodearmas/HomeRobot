using System;
using System.Collections.Generic;
using System.Text;

namespace HomeRobot.Core
{
    public class EnvironmentSettings
    {
        public int Rows { get; set; }

        public int Cols { get; set; }

        public int Turns { get; set; }

        public int DirtPercent { get; set; }

        public int ObstaclePercent { get; set; }

        public int ChildrenCount { get; set; }

        public override string ToString()
        {
            return $"{Rows}X{Cols}[{ChildrenCount}] - T:{Turns} - D:{DirtPercent}% - O:{ObstaclePercent}%";
        }
    }
}
