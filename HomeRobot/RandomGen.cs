using System;
using System.Collections.Generic;
using System.Text;
using Troschuetz;

namespace HomeRobot
{
    public class RandomGen
    {
        private Random random;

        // Inicialización del generador de números aleatorios  
        private void Init()
        {
            if (random == null) 
                random = new Random((int)DateTime.Now.Ticks % 100000);
        }

        public int RandomNumber(int min, int max)
        {
            Init();
            return random.Next(min, max);
        }
    }
}
