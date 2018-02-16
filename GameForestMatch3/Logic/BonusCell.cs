using System;
using System.Collections.Generic;

namespace GameForestMatch3.Logic
{
    public abstract class BonusCell : Cell
    {
        public BonusType BonusType { get; set; }
        public bool IsNew { get; set; }
        public bool IsDetonated { get; set; }
        public bool IsEmpty { get; set; }
        public abstract List<Cell> Action (Cell[,] grid);
    }
}
