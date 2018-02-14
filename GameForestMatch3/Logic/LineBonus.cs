using System;
using System.Collections.Generic;

namespace GameForestMatch3.Logic
{
    public class LineBonus : BonusCell
    {
        public bool IsVertical { get; set; }

        public override List<Cell> Action(Cell[,] grid)
        {
            var cellsToDelete = new List<Cell>();
            for (var dim = 0; dim < Field.SIZE; dim++)
            {
                cellsToDelete.Add(IsVertical ? grid[Col, dim] : grid[dim, Row]);
            }
            return cellsToDelete;
        }

        public override int Points
        {
            get
            {
                return 200;
            }
        }

    }
}
