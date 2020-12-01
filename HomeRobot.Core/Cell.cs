using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace HomeRobot.Core
{
    [Flags]
    public enum CellStatus { Empty = 0, Obstacle = 1, Child = 2, Robot = 4, Dirt = 8 };

    public class Cell
    {
        public CellStatus CurrentStatus { get; set; }
        public Point Index { get; private set; }

        public int playpenIndex = -1;

        public int PlaypenIndex
        {
            get { return playpenIndex; }
            set { playpenIndex = value; }
        }

        private SortedList<CellStatus, MobileObject> objs = new SortedList<CellStatus, MobileObject>();

        public void SetMobileObject(CellStatus type, MobileObject value)                
        {
            if (value == null)
                CurrentStatus ^= type;
            else
                CurrentStatus |= type;

            if (objs.ContainsKey(type))
                objs[type] = value;
            else
                objs.Add(type, value);
        }

        public MobileObject GetMobileObject(CellStatus type)
        {
            return objs.FirstOrDefault(x => x.Key == type).Value;
        }

        public Cell(Point p)
        {
            CurrentStatus = CellStatus.Empty;
            Index = p;
        }

        public void Reset()
        {
            playpenIndex = -1;
            CurrentStatus = CellStatus.Empty;
            objs.Clear();
        }

        public bool IsDirty()
        {
            return (CurrentStatus & CellStatus.Dirt) == CellStatus.Dirt;
        }

        public bool IsEmpty()
        {
            return CurrentStatus == CellStatus.Empty;
        }

        public bool IsObstacled()
        {
            return CurrentStatus == CellStatus.Obstacle;
        }

        public bool IsBusy()
        {
            return ((CurrentStatus & CellStatus.Child) == CellStatus.Child) ||
                ((CurrentStatus & CellStatus.Robot) == CellStatus.Robot);
        }

        public bool IsChild()
        {
            return ((CurrentStatus & CellStatus.Child) == CellStatus.Child);
        }

        public bool IsRobot()
        {
            return ((CurrentStatus & CellStatus.Robot) == CellStatus.Robot);
        }

        public bool IsPlaypen()
        {
            return (playpenIndex != -1);
        }

        public bool OnlyDirt()
        {
            return CurrentStatus == CellStatus.Dirt;
        }   

        public void Clean()
        {
            if ((CurrentStatus & CellStatus.Dirt) == CellStatus.Dirt)
                CurrentStatus ^= CellStatus.Dirt;   //Eliminar la suciedad del estado actual 
        }

        public void Dirty()
        {
            CurrentStatus |= CellStatus.Dirt;
        }
    }

}
