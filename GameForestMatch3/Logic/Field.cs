using System;
using System.Collections.Generic;
using System.Linq;

namespace GameForestMatch3.Logic
{
    public class Field
    {
        public const int SIZE = 8;

        readonly int[][] HorizontalTwoInALinePattern =
        {
              new int[] { 1, 0 },
              new int[] {-2, 0 },
              new int[] {-1,-1 },
              new int[] {-1, 1 },
              new int[] { 2,-1 },
              new int[] { 2, 1 },
              new int[] { 3, 0 }
        };

        readonly int[][] HorizontalSeparatePattern =
        {
              new int[] { 1,-1 },
              new int[] { 1, 1 }
        };

        readonly int[][] VerticalTwoInALinePattern =
        {
              new int[] { 0,-2 },
              new int[] {-1,-1 },
              new int[] { 1,-1 },
              new int[] { 1,-1 },
              new int[] {-1, 2 },
              new int[] { 1, 2 },
              new int[] { 1, 3 }
        };

        readonly int[][] VerticalTwoSeparatePattern =
        {
              new int[] {-1, 1 },
              new int[] { 1, 1 }
        };


        readonly int[][] NeighborsPattern =
        {
              new int[] { 1, 0 },
              new int[] {-1, 0 },
              new int[] { 0,-1 },
              new int[] { 0, 1 }
        };

        Cell[,] Grid;

        Random Rnd = new Random();

        int _points;
        public int Points
        {
            get
            {
                return _points;
            }
            set
            {
                _points = value;
                PointsChanged.Invoke(null, new EventArgs());
            }
        }

        public event EventHandler PointsChanged;



        public Cell LastTouched { get; set; }
        public Cell FirstTouched { get; set; }

        public event EventHandler TouchedChanged;



        readonly List<CellType> TypeList = Enum.GetValues(typeof(CellType)).OfType<CellType>().ToList();


        public static bool FitGrid(int col, int row)
        {
            return col >= 0 && col < SIZE && row >= 0 && row < SIZE;
        }

        public void ResetSelected()
        {
            FirstTouched = null;
            LastTouched = null;
        }

        public Field()
        {
            InitGrid();
            CheckAndFixGrid();
        }


        void InitGrid()
        {
            Grid = new Cell[SIZE, SIZE];
            for (var col = 0; col < SIZE; col++)
            {
                for (var row = 0; row < SIZE; row++)
                {
                    Grid[col, row] = new Cell
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
            var currentType = Grid[col, row].Type;

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


        public bool HasGivenType(int col, int row, CellType type)
        {
            if (!FitGrid(col, row)) return false;
            return (Grid[col, row].Type == type);
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
            possibility[1].Type = GenerateType(new List<CellType> { possibilityType });

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
            return Grid[rndCol, rndRow];
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
                    cell, Grid[col, row + 1], Grid[col, row + 2], Grid[col, row + 3]
                };
            }

            // Take next 3 cells in the column
            return new List<Cell>
                {
                    cell, Grid[col + 1, row], Grid[col + 2, row], Grid[col + 3, row]
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


        CellType GetNewGemType(Cell cell)
        {
            var neighbors = GetNeighbors(cell);
            var allTypes = neighbors.Select(item => item.Type);
            var types = allTypes.Distinct().ToList();
            if (!types.Contains(cell.Type))
            {
                types.Add(cell.Type);
            }

            // All neighbors are already different
            if (types.Count() == TypeList.Count())
            {
                return cell.Type;
            }
            return GenerateType(types);
        }


        List<Cell> GetNeighbors(Cell cell)
        {
            var result = new List<Cell>();
            foreach (var neighbor in NeighborsPattern)
            {
                var neighborCell = GetShiftedCell(cell.Col, cell.Row, neighbor);
                if (neighborCell != null)
                {
                    result.Add(neighborCell);
                }
            }
            return result;
        }


        Cell GetShiftedCell(int col, int row, int[] shift)
        {
            col = col + shift[0];
            row = row + shift[1];
            if (!FitGrid(col, row)) return null;
            return Grid[col, row];
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
            var match = new List<Cell> { Grid[col, row] };
            for (var i = 1; col + i < SIZE; i++)
            {
                if (Grid[col, row].Type == Grid[col + i, row].Type)
                {
                    match.Add(Grid[col + i, row]);
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
            var match = new List<Cell> { Grid[col, row] };
            for (var i = 1; row + i < SIZE; i++)
            {
                if (Grid[col, row].Type == Grid[col, row + i].Type)
                {
                    match.Add(Grid[col, row + i]);
                }
                else
                {
                    return match;
                }
            }
            return match;
        }


        CellType GenerateType(List<CellType> except = null)
        {
            if (except == null) except = new List<CellType>();

            // Can not except more types than total amount of types
            if (except.Distinct().Count() >= TypeList.Count())
            {
                throw new Exception("Can not generate a type");
            }

            // There will be at least one unused GemType since we have 5 types
            var unictypes = TypeList.Except(except).ToList();
            var index = Rnd.Next(unictypes.Count());
            return unictypes[index];
        }


        public void FillEmpltyPlaces()
        {
            for (var col = 0; col < SIZE; col++)
            {
                for (var row = 0; row < SIZE; row++)
                {
                    if (Grid[col, row] == null)
                    {
                        Grid[col, row] = new Cell
                        {
                            Type = GenerateType(),
                            Col = col,
                            Row = row
                        };

                        CellCreated.Invoke(null, Grid[col,row]);
                    };
                }
            }

            // TODO вернуть когда надо
            //CheckAndFixGrid();
        }

        void MoveDown(int removedCol, int removedRow)
        {
            var cellsToMove = new List<Tuple<int, int>>();
            // Since Cocos2d has Y axe on the bottom of the screen, we have upside down field,
            // so cells fall upwards :/
            for (var row = removedRow + 1; row < SIZE; row++)
            {
                if (Grid[removedCol, row] != null)
                {
                    Grid[removedCol, row].Row--;
                    Grid[removedCol, row - 1] = Grid[removedCol, row];
                    Grid[removedCol, row] = null;
                }
            }
        }

        public event EventHandler NoMoreMatches;

        public void DestroyMatches()
        {
            var matches = GetMatches();

            if (!matches.Any())
            {
                ResetSelected();
                NoMoreMatches.Invoke(null, new EventArgs());
                TouchedChanged.Invoke(null, new EventArgs());
                return;
            }

            Consolidate(matches);
            for (var i = 0; i < matches.Count(); i++)
            {
                var toDelete = new List<Tuple<int, int>>();
                foreach (var cell in matches[i])
                {
                    toDelete.Add(new Tuple<int, int>(cell.Col, cell.Row));
                }

                var deleteIndex = 0;
                foreach (var cell in matches[i])
                {
                    var col = cell.Col;
                    var row = cell.Row;

                    Points += cell.Points;
                    Cell bonusCell = null;//GetBonusCell(cell, matches[i], LastTouched);



                    // TODO check if add on the fly works correctly
                    //if (cell is BonusCell)
                    //{
                    //    var toDestroy = ((BonusCell)cell).Action(Grid);
                    //    matches.Add(toDestroy);
                    //}

                    //Grid[col, row] = bonusCell;

                    Grid[col, row] = null;
                    CellDeleted.Invoke(null, new Tuple<int, int>(col, row));
                    MoveDown(col, row);
                    MovedDown.Invoke(null, new EventArgs());
                }


            }
        }


        public event EventHandler<Tuple<int,int>> CellDeleted;
        public event EventHandler MovedDown;
        public event EventHandler<Cell> CellCreated;

        BonusCell GetBonusCell(Cell cell, List<Cell> match, Cell lastTouched)
        {
            if (match.Count() < 4)
            {
                return null;
            }

            if (match.Count() == 4)
            {
                // Bonus appears on the last_touched_place 
                // or in any place if the match is appeared after appearing new cells
                var bonusPlaceCell = lastTouched ?? match.First();
                return new LineBonus
                {
                    IsVertical = IsVertical(match),
                    Type = cell.Type,
                    Col = bonusPlaceCell.Col,
                    Row = bonusPlaceCell.Row
                };
            }


            if (match.Count() > 4)
            {
                var crossCenter = GetCrossCenter(match);
                // Bonus appears on the cross center, or last_touched_place, 
                // or in any place if the match is appeared after appearing new cells
                var bonusPlaceCell = crossCenter ?? lastTouched ?? match.First();
                return new BombBonus
                {
                    Type = cell.Type,
                    Col = bonusPlaceCell.Col,
                    Row = bonusPlaceCell.Row
                };
            }
            return null;
        }

        Cell GetCrossCenter(List<Cell> match)
        {
            return match.Find(item => item.IsCenter);
        }

        bool IsVertical(List<Cell> match)
        {
            return (match.First().Row == match.Last().Row);
        }


        void Consolidate(List<List<Cell>> matches)
        {
            bool intersectionFound;
            do
            {
                intersectionFound = false;
                for (var i = 0; i < matches.Count() - 1; i++)
                {
                    for (var j = i + 1; j < matches.Count(); j++)
                    {
                        var intersection = matches[i].Intersect(matches[j]);
                        if (intersection.Any())
                        {
                            intersection.ToList().ForEach(item => item.IsCenter = true);

                            matches[i].AddRange(matches[j]);
                            matches.Remove(matches[j]);
                            intersectionFound = true;
                            break;
                        }
                    }
                    if (intersectionFound)
                    {
                        break;
                    }
                }
            } while (intersectionFound);
        }


        public void Touch(int col, int row)
        {
            Touch(Grid[col, row]);
        }

        public void Touch(Cell cell)
        {
            FirstTouched = LastTouched;
            LastTouched = cell;
            if (FirstTouched != null)
            {
                if (AreNeighbors(FirstTouched, LastTouched))
                {
                    Swap(FirstTouched, LastTouched);
                    var matches = GetMatches();
                    if (!matches.Any()) 
                    {
                        Swap(FirstTouched, LastTouched);
                        ResetSelected();
                    }
                }
                else if (FirstTouched != LastTouched)
                {
                    FirstTouched = null;
                    LastTouched = cell;
                }
                else {
                    ResetSelected();
                }
            }
            TouchedChanged.Invoke(null, new EventArgs());
        }


        bool AreNeighbors(Cell firstTouched, Cell lastTouched)
        {
            foreach (var neighbor in NeighborsPattern)
            {
                var cell = GetShiftedCell(firstTouched.Col, firstTouched.Row, neighbor);
                if (cell != null && cell.Col == lastTouched.Col && cell.Row == lastTouched.Row)
                {
                    return true;
                }
            }
            return false;
        }


        void Swap(Cell firstTouched, Cell lastTouched)
        {

            var fCol = firstTouched.Col;
            var fRow = firstTouched.Row;
            var lCol = lastTouched.Col;
            var lRow = lastTouched.Row;

            Grid[fCol, fRow] = lastTouched;
            Grid[lCol, lRow] = firstTouched;

            lastTouched.Col = fCol;
            lastTouched.Row = fRow;

            firstTouched.Col = lCol;
            firstTouched.Row = lRow;
        }


        public Cell GetCell(int col, int row) {
            return Grid[col, row];
        }
    }
}
