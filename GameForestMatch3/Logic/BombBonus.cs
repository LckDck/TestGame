using System;
using System.Collections.Generic;

namespace GameForestMatch3.Logic
{
    public class BombBonus : BonusCell
    {
        readonly int[][] AroundCellsPattern =
                {
              new int[] { 1, 0 },
              new int[] {-1, 0 },
              new int[] {-1, 1 },
              new int[] { 1,-1 },
              new int[] {-1,-1 },
              new int[] { 1, 1 },
              new int[] { 0,-1 },
              new int[] { 0, 1 }
        };

        public override List<Cell> Action(Cell[,] grid)
        {
            var cellsToDelete = new List<Cell> { this };
            foreach (var neighbor in AroundCellsPattern) 
            {
                var newCol = Col + neighbor[0];
                var newRow = Row + neighbor[1];
                if (Field.FitGrid(newCol, newRow)) {
                    cellsToDelete.Add(grid[newCol, newRow]);
                }
            }
            return cellsToDelete;
        }


        public override int Points
        {
            get
            {
                return 300;
            }
        }
    }
}
