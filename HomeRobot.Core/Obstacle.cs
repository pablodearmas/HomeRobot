using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace HomeRobot.Core
{
    public class Obstacle: MobileObject
    {
        public Obstacle(Environment env, Point pos): base(env)
        {
            Reset(pos);
        }

        protected override void SetObject(Point pos)
        {
            Environment[pos].SetMobileObject(CellStatus.Obstacle, this);
            Environment[CurrentPos].SetMobileObject(CellStatus.Obstacle, null);
        }
    }
}
