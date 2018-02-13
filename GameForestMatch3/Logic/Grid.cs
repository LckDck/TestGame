using System;
using System.Collections.Generic;
using System.Linq;

namespace GameForestMatch3.Logic
{
    public class Grid
    {
        public const int SIZE = 8;

        int[][] HorizontalTwoInALinePattern =
        {
              new int[] { 1, 0 },
              new int[] {-2, 0 },
              new int[] {-1,-1 },
              new int[] {-1, 1 },
              new int[] { 2,-1 },
              new int[] { 2, 1 },
              new int[] { 3, 0 }
        };

        int[][] HorizontalSeparatePattern =
        {
              new int[] { 1,-1 },
              new int[] { 1, 1 }
        };

        int[][] VerticalTwoInALinePattern =
        {
              new int[] { 0,-2 },
              new int[] {-1,-1 },
              new int[] { 1,-1 },
              new int[] { 1,-1 },
              new int[] {-1, 2 },
              new int[] { 1, 2 },
              new int[] { 1, 3 }
        };

        int[][] VerticalTwoSeparatePattern =
        {
              new int[] {-1, 1 },
              new int[] { 1, 1 }
        };


        int[][] NeighborsPattern =
        {
              new int[] { 1, 0 },
              new int[] {-1, 0 },
              new int[] { 0,-1 },
              new int[] { 0, 1 }
        };

        Cell[,] Cells;

        Random Rnd = new Random();

        readonly List<GemType> TypeList;

        public Grid()
        {
            TypeList = Enum.GetValues(typeof(GemType)).OfType<GemType>().ToList();
            InitGrid();
            CheckAndFixGrid();
        }


        void InitGrid()
        {
            Cells = new Cell[SIZE, SIZE];
            for (var col = 0; col < SIZE; col++)
            {
                for (var row = 0; row < SIZE; row++)
                {
                    Cells[col, row] = new Cell
                    {
                        Type = GenerateType(),
                        Col = col,
                        Row = row
                    };
                }
            }
        }


        void CheckAndFixGrid()
        {
            var matches = GetMatches();
            if (matches.Any())
            {
                // If there are some matches, we destroy them all (devil laugh!)
                FixToDoNoMatches(matches);
            }

            // If there is no moves possible, we add one
            if (HasNoMoves())
            {
                FixToHaveMove();
            }
        }


        bool HasNoMoves()
        {
            for (var col = 0; col < SIZE; col++)
            {
                for (var row = 0; row < SIZE; row++)
                {
                    // Horizontal, two in a line     
                    if (Match(col, row, new int[][] { new int[] { 1, 0 } }, HorizontalTwoInALinePattern))
                    {
                        return true;
                    }

                    // Horizontal, two separate
                    if (Match(col, row, new int[][] { new int[] { 2, 0 } }, HorizontalSeparatePattern))
                    {
                        return true;
                    }

                    // Vertical, two in a line
                    if (Match(col, row, new int[][] { new int[] { 0, 1 } }, VerticalTwoInALinePattern))
                    {
                        return true;
                    }

                    // Vertical, two separate
                    if (Match(col, row, new int[][] { new int[] { 0, 2 } }, VerticalTwoSeparatePattern))
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        bool Match(int col, int row, int[][] must, int[][] need)
        {
            var currentType = Cells[col, row].Type;

            // Check if we have all from "must"
            for (var i = 0; i < must.Length; i++)
            {
                if (!HasGivenType(col + must[i][0], row + must[i][1], currentType))
                {
                    return false;
                }
            }
            // Check if we have one from "need"
            for (var i = 0; i < need.Length; i++)
            {
                if (HasGivenType(col + need[i][0], row + need[i][1], currentType))
                {
                    return true;
                }
            }
            return false;
        }


        public bool HasGivenType(int col, int row, GemType type)
        {
            if ((col < 0) || (col > SIZE - 1) || (row < 0) || (row > SIZE - 1)) return false;
            return (Cells[col, row].Type == type);
        }


        // Function adds a possibility to move like "X Y X X" in a random place
        void FixToHaveMove()
        {
            // Take a random cell, from which we can start possibility like "X Y X X"
            var randomCell = GetRandomCellStartingMovePossibility();
            // Get list of cells in that line
            var possibility = GetCellsForPossibility(randomCell);

            // Make all possibility the same type
            var possibilityType = GenerateType();
            possibility.ForEach((obj) => obj.Type = possibilityType);

            // Then change one cell type to match X Y X X pattern
            possibility[1].Type = GenerateType(new List<GemType> { possibilityType });

            // Change neighbors types to be sure they make no more matches
            RePaintNeighbors(possibility);
        }


        void RePaintNeighbors(List<Cell> possibility)
        {
            var neighbors = new List<Cell>();
            foreach (var cell in possibility)
            {
                neighbors.AddRange(GetNeighbors(cell));
            }
            // Remove cells that are contained in "possibility" itself and duplicate cells
            neighbors = neighbors.Except(possibility).Distinct().ToList();

            // To each founded cell set a unic type (relative to its neighbors), 
            // so there is guaranteed no new matches after repaint
            foreach (var cell in neighbors)
            {
                cell.Type = GetNewGemType(cell);
            }
        }


        Cell GetRandomCellStartingMovePossibility()
        {
            // Since the cell should start a line like "X X Y X",
            // it should have enough space (another 3 cells) to place this line in a grid
            var rndCol = Rnd.Next(SIZE - 3);
            var rndRow = Rnd.Next(SIZE - 3);
            return Cells[rndCol, rndRow];
        }


        List<Cell> GetCellsForPossibility(Cell cell)
        {
            var col = cell.Col;
            var row = cell.Row;

            // Take next 3 cells in the row
            if (row + 3 < SIZE)
            {
                return new List<Cell>
                {
                    cell, Cells[col, row + 1], Cells[col, row + 2], Cells[col, row + 3]
                };
            }

            // Take next 3 cells in the col
            return new List<Cell>
                {
                    cell, Cells[col + 1, row], Cells[col + 2, row], Cells[col + 3, row]
                };
        }


        void FixToDoNoMatches(List<List<Cell>> matches)
        {
            foreach (var match in matches)
            {
                // Change a type of every 3th cell starting from 2st,
                // to the type that not matches all its neighbors
                for (var i = 2; i < match.Count; i += 3)
                {
                    var type = match[i].Type;
                    match[i].Type = GetNewGemType(match[i]);
                }
            }
        }


        GemType GetNewGemType(Cell cell)
        {
            var neighbors = GetNeighbors(cell);
            var types = neighbors.Select(item => item.Type).Distinct().ToList();
            if (!types.Contains(cell.Type))
            {
                types.Add(cell.Type);
            }
            return GenerateType(types);
        }


        List<Cell> GetNeighbors(Cell cell)
        {
            var result = new List<Cell>();
            foreach (var neighbor in NeighborsPattern)
            {
                result.Add(GetShiftedCell(cell.Col, cell.Row, neighbor));
            }
            return result;
        }


        Cell GetShiftedCell(int col, int row, int[] shift)
        {
            col = col + shift[0];
            row = row + shift[1];
            if (col < 0 || col >= SIZE || row < 0 || row >= SIZE) return null;
            return Cells[col, row];
        }


        List<List<Cell>> GetMatches()
        {
            var matchList = new List<List<Cell>>();
            // Horizontal matches
            for (var row = 0; row < SIZE; row++)
            {
                for (var col = 0; col < SIZE - 2; col++)
                {
                    var match = GetHorizontalMatch(col, row);
                    if (match.Count > 2)
                    {
                        matchList.Add(match);
                        //skip other gems in this match
                        col += match.Count - 1;
                    }
                }
            }
            // Vertical matches
            for (var col = 0; col < SIZE; col++)
            {
                for (var row = 0; row < SIZE - 2; row++)
                {
                    var match = GetVerticalMatch(col, row);
                    if (match.Count > 2)
                    {
                        matchList.Add(match);
                        // Skip other cells in this match
                        row += match.Count - 1;
                    }
                }
            }
            return matchList;
        }


        List<Cell> GetHorizontalMatch(int col, int row)
        {
            var match = new List<Cell> { Cells[col, row] };
            for (var i = 1; col + i < SIZE; i++)
            {
                if (Cells[col, row].Type == Cells[col + i, row].Type)
                {
                    match.Add(Cells[col + i, row]);
                }
                else
                {
                    return match;
                }
            }
            return match;
        }


        List<Cell> GetVerticalMatch(int col, int row)
        {
            var match = new List<Cell> { Cells[col, row] };
            for (var i = 1; row + i < SIZE; i++)
            {
                if (Cells[col, row].Type == Cells[col, row + i].Type)
                {
                    match.Add(Cells[col, row + i]);
                }
                else
                {
                    return match;
                }
            }
            return match;
        }


        GemType GenerateType(List<GemType> except = null)
        {
            // Can not except more types than total amount of types
            if (except.Distinct().Count() >= TypeList.Count())
            {
                throw new Exception("Can not generate a type");
            }

            if (except == null) except = new List<GemType>();

            // There will be at least one unused GemType since we have 5 types
            var unictypes = TypeList.Except(except).ToList();
            var index = Rnd.Next(unictypes.Count());
            return unictypes[index];
        }


        void FillEmpltyPlaces()
        {
            for (var col = 0; col < SIZE; col++)
            {
                for (var row = 0; row < SIZE; row++)
                {
                    if (Cells[col, row] == null)
                    {
                        MoveDown(Cells[col, row]);
                    };
                }
            }

            for (var col = 0; col < SIZE; col++)
            {
                for (var row = 0; row < SIZE; row++)
                {
                    if (Cells[col, row] == null)
                    {
                        Cells[col, row] = new Cell
                        {
                            Type = GenerateType(),
                            Col = col,
                            Row = row
                        };
                    };
                }
            }
            CheckAndFixGrid();
        }


        void MoveDown(int removedCol, int removedRow)
        {
            for (var row = removedRow - 1; row >= 0; row--) {
                if (Cells[removedCol, row] != null)
                {
                    Cells[removedCol, row].Row++;
                    Cells[removedCol, row + 1] = Cells[removedCol, row];
                    Cells[removedCol, row] = null;
                }
            }
        }

        void DestroyMatches()
        {
            var matches = GetMatches();
            foreach (var match in matches)
            {
                foreach (var cell in match)
                {
                    var col = cell.Col;
                    var row = cell.Row;
                    Cells[col, row] = null;
                    MoveDown(col, row);
                }
            }
        }
    }
}
