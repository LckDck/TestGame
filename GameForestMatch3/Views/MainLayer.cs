using System;
using System.Collections.Generic;
using System.Linq;
using CocosSharp;
using GameForestMatch3.Logic;
using GameForestMatch3.Views.Elements;

namespace GameForestMatch3.Views
{
    public class MainLayer : CCLayerColor
    {
        float CellAnimationDuration = 0.5f;
        CellSprite[,] places;
        List<GemSprite> gems;
        Field GameCore;

        bool IsMoving;
        protected override void AddedToScene()
        {
            base.AddedToScene();

            GameCore = new Field();
            GameCore.TouchedChanged += OnTouched;
            GameCore.CellDeleted += OnCellDeleted;
            GameCore.MovedDown += OnMovedDown;
            GameCore.CellCreated += OnCellCreated;
            GameCore.NoMoreMatches += OnNoMoreMatches;
            GameCore.PointsChanged += OnScoreChanged;

            Scene.SceneResolutionPolicy = CCSceneResolutionPolicy.ShowAll;
            places = new CellSprite[Field.SIZE, Field.SIZE];
            gems = new List<GemSprite>();
            CreateField();
            AddScoreLabel();
        }


        public static CCScene GameLayerScene(CCWindow mainWindow)
        {
            var scene = new CCScene(mainWindow);
            var layer = new MainLayer();
            scene.AddChild(layer);
            return scene;
        }


        void OnScoreChanged(object sender, EventArgs e)
        {
            ScoreLabel.Text = $"SCORE: {GameCore.Points}";
        }


        void OnNoMoreMatches(object sender, EventArgs e)
        {
            IsMoving = false;
        }


        void OnCellCreated(object sender, Cell cell)
        {
            var gem = CreateGem(cell.Type, cell.Col, cell.Row);
            AddChild(gem);
        }


        void OnMovedDown(object sender, EventArgs e)
        {
            foreach (var gem in gems)
            {
                MoveGem(gem);
            }

            if (gems.Count() < Field.SIZE * Field.SIZE)
            {
                StartCreationTimer();
            }
        }


        void OnCellDeleted(object sender, Tuple<int, int> cell)
        {
            var col = cell.Item1;
            var row = cell.Item2;

            var gem = gems.Find(item => item.Col == col && item.Row == row);
            if (gem != null)
            {
                RemoveChild(gem);
                gems.Remove(gem);
            }
        }


        void OnTouched(object sender, EventArgs e)
        {
            if (IsMoving)
            {
                return;
            }
            IsMoving = true;

            ClearBackgroundColor();
            int col;
            int row;
            if (GameCore.LastTouched != null)
            {
                col = GameCore.LastTouched.Col;
                row = GameCore.LastTouched.Row;
                places[col, row].Color = CCColor3B.DarkGray;
            }

            if (GameCore.FirstTouched != null)
            {
                col = GameCore.FirstTouched.Col;
                row = GameCore.FirstTouched.Row;
                places[col, row].Color = CCColor3B.DarkGray;
            }

            var changeFound = false;
            foreach (var gem in gems)
            {
                var success = MoveGem(gem);
                if (success)
                {
                    changeFound = true;
                }
            }

            if (changeFound)
            {
                StartDestroyTimer();
            }
            else
            {
                IsMoving = false;
            }
        }


        bool MoveGem(GemSprite gem)
        {
            if (gem == null) return false;
            var col = gem.Col;
            var row = gem.Row;
            if (gem.Cell.Col != gem.Col || gem.Cell.Row != gem.Row)
            {
                gem.StopAllActions();
                var newPlace = places[gem.Cell.Col, gem.Cell.Row];
                var target = new CCPoint(newPlace.PositionX, newPlace.PositionY);

                gem.Col = gem.Cell.Col;
                gem.Row = gem.Cell.Row;

                var moveAction = new CCMoveTo(CellAnimationDuration, target);
                var ease = new CCEaseIn(moveAction, 4);

                gem.AddAction(ease);
                return true;
            }
            return false;
        }


        bool DestroyTimerStarted;
        void StartDestroyTimer()
        {
            if (DestroyTimerStarted) return;
            DestroyTimerStarted = true;

            var delayAction = new CCDelayTime(CellAnimationDuration + 0.1f);
            var delayCompletedAction = new CCCallFunc(() =>
            {
                DestroyTimerStarted = false;
                ClearBackgroundColor();
                GameCore.DestroyMatches();

            });
            CCSequence sequence = new CCSequence(delayAction, delayCompletedAction);
            RunAction(sequence);
        }


        bool FillTimerStarted;
        void StartCreationTimer()
        {
            if (FillTimerStarted) return;

            FillTimerStarted = true;

            var delayAction = new CCDelayTime(CellAnimationDuration + 0.1f);
            var delayCompletedAction = new CCCallFunc(() =>
            {
                GameCore.FillEmpltyPlaces();
                FillTimerStarted = false;
                StartDestroyTimer();
            });
            CCSequence sequence = new CCSequence(delayAction, delayCompletedAction);
            RunAction(sequence);
        }


        void ClearBackgroundColor()
        {
            foreach (var place in places)
            {
                place.Color = CCColor3B.Gray;
            }
        }


        int GemSize;
        void CreateField()
        {
            var width = VisibleBoundsWorldspace.Size.Width;
            var height = VisibleBoundsWorldspace.Size.Height;
            var distance = 2;
            var distancesWidth = (Field.SIZE - 1) * distance;

            var fieldWidth = width * (float)0.9;
            var offsetX = (width - fieldWidth - distancesWidth) / 2;
            var offsetY = (height - fieldWidth - distancesWidth) / 2;

            var oneSize = (fieldWidth - distance) / Field.SIZE;
            GemSize = (int)oneSize - 20;

            for (var i = 0; i < Field.SIZE; i++)
            {
                for (var j = 0; j < Field.SIZE; j++)
                {
                    var back = CreateBack(oneSize, i, j, offsetX, offsetY, distance);
                    AddChild(back);
                    CreateGem(GameCore.GetCell(i, j).Type, i, j);
                }
            }

            foreach (var gem in gems)
            {
                AddChild(gem);
            }

            var touchListener = new CCEventListenerTouchOneByOne();
            touchListener.OnTouchBegan = TouchBegan;
            AddEventListener(touchListener, this);
        }


        CellSprite CreateBack(float oneWidth, int i, int j, float offsetX, float offsetY, int distance)
        {
            var back = new CellSprite(new CCTexture2D(), new CCRect(0, 0, oneWidth, oneWidth))
            {
                Color = CCColor3B.Gray,
                PositionX = offsetX + i * (oneWidth + distance) + oneWidth / 2,
                PositionY = offsetY + j * (oneWidth + distance) + oneWidth / 2,
                Col = i,
                Row = j,
            };
            places[i, j] = back;
            return back;
        }

        GemSprite CreateGem(CellType type, int col, int row)
        {
            var gem = new GemSprite(new CCTexture2D(), new CCRect(0, 0, GemSize, GemSize))
            {
                Color = GetColor(type),
                PositionX = places[col, row].PositionX,
                PositionY = places[col, row].PositionY,
                Cell = GameCore.GetCell(col, row),
                Col = col,
                Row = row
            };
            gems.Add(gem);
            return gem;
        }


        private CCColor3B GetColor(CellType type)
        {
            switch (type)
            {
                case CellType.Blue:
                    return CCColor3B.Blue;
                case CellType.Yellow:
                    return CCColor3B.Yellow;
                case CellType.Red:
                    return CCColor3B.Red;
                case CellType.Purple:
                    return CCColor3B.Magenta;
                case CellType.Green:
                    return CCColor3B.Green;
                default:
                    return CCColor3B.DarkGray;
            }
        }


        bool TouchBegan(CCTouch touch, CCEvent ev)
        {
            foreach (var place in places)
            {
                if (place.BoundingBoxTransformedToWorld.ContainsPoint(touch.Location))
                {
                    GameCore.Touch(place.Col, place.Row);
                }
            }
            return false;
        }


        CCLabelTtf ScoreLabel;
        void AddScoreLabel()
        {
            ScoreLabel = new CCLabelTtf($"SCORE: {GameCore.Points}", "arial", 22)
            {
                Color = CCColor3B.Orange,
                HorizontalAlignment = CCTextAlignment.Center,
                VerticalAlignment = CCVerticalTextAlignment.Center,
                AnchorPoint = CCPoint.AnchorMiddle,
                PositionX = VisibleBoundsWorldspace.Center.X,
                PositionY = VisibleBoundsWorldspace.UpperRight.Y - 50
            };
            AddChild(ScoreLabel);
        }
    }
}