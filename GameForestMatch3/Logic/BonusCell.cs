using System;
using System.Collections.Generic;

namespace GameForestMatch3.Logic
{
    public abstract class BonusCell : Cell
    {
        public abstract List<Cell> Action (Cell[,] grid);
    }
}
