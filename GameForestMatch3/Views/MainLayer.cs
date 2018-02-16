using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CocosSharp;
using GameForestMatch3.Logic;
using GameForestMatch3.Views.Elements;

namespace GameForestMatch3.Views
{
    public class MainLayer : CCLayerColor
    {
        readonly float CellAnimationDuration = 0.5f;
        readonly float CellAppearingDuration = 0.1f;
        readonly int GameDurationInSeconds = 60;

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
            GameCore.BonusCreated += OnBonusCreated;

            var touchListener = new CCEventListenerTouchOneByOne();
            touchListener.OnTouchBegan = TouchBegan;
            AddEventListener(touchListener, this);

            Scene.SceneResolutionPolicy = CCSceneResolutionPolicy.ShowAll;
            places = new CellSprite[Field.SIZE, Field.SIZE];
            gems = new List<GemSprite>();

            CreateField();
            AddScoreLabel();
            AddTimerLabel();
            InitTimer();
        }


        public override void Cleanup()
        {
            base.Cleanup();
            GameCore.TouchedChanged -= OnTouched;
            GameCore.CellDeleted -= OnCellDeleted;
            GameCore.MovedDown -= OnMovedDown;
            GameCore.CellCreated -= OnCellCreated;
            GameCore.NoMoreMatches -= OnNoMoreMatches;
            GameCore.PointsChanged -= OnScoreChanged;
            GameCore.BonusCreated -= OnBonusCreated;
            RemoveAllListeners();
            RemoveAllChildren(true);
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
            if (TimeIsOut)
            {
                GameOver();
            }
        }


        void OnCellCreated(object sender, Cell cell)
        {
            var gem = CreateGem(cell.Type, cell.Col, cell.Row);
            gem.Scale = 0.5f;
            gem.AddAction (new CCScaleTo(CellAppearingDuration, 1));
            AddChild(gem);
        }


        void OnMovedDown(object sender, EventArgs e)
        {
            foreach (var gem in gems)
            {
                MoveGem(gem);
            }
            // If there is empty places on field - initiate creation process
            if (gems.Count() < Field.SIZE * Field.SIZE)
            {
                StartCreationTimer();
            }
        }


        void OnBonusCreated(object sender, BonusCell cell)
        {
            var gem = CreateGem(cell.Type, cell.Col, cell.Row, cell.BonusType);
            gem.Scale = 0.5f;
            gem.AddAction(new CCScaleTo(CellAppearingDuration, 1));
            AddChild(gem);
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

            // Clear cell selection 
            ClearBackgroundColor();
            int col;
            int row;

            // Select last touched gem
            if (GameCore.LastTouched != null)
            {
                col = GameCore.LastTouched.Col;
                row = GameCore.LastTouched.Row;
                places[col, row].Color = CCColor3B.DarkGray;
            }

            // Select previously touched gem
            if (GameCore.FirstTouched != null)
            {
                col = GameCore.FirstTouched.Col;
                row = GameCore.FirstTouched.Row;
                places[col, row].Color = CCColor3B.DarkGray;
            }

            // Trying to move gem to the position corresponding its col and row
            var changeFound = false;
            foreach (var gem in gems)
            {
                var success = MoveGem(gem);
                if (success)
                {
                    changeFound = true;
                }
            }

            // If there was a move, initiate destroy process
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

            // Check if gem place corresponds its cell place
            // if not - it should move
            if (gem.Cell.Col != gem.Col || gem.Cell.Row != gem.Row)
            {
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

                }
            }

            for (var i = 0; i < Field.SIZE; i++)
            {
                for (var j = 0; j < Field.SIZE; j++)
                {
                    var gem = CreateGem(GameCore.GetCell(i, j).Type, i, j);
                    AddChild(gem);
                }
            }
        }


        CellSprite CreateBack(float oneWidth, int i, int j, float offsetX, float offsetY, int distance)
        {
            // Cocos Y axe starts on the bottom
            var row = Field.SIZE - 1 - j;
            var back = new CellSprite(new CCTexture2D(), new CCRect(0, 0, oneWidth, oneWidth))
            {
                Color = CCColor3B.Gray,
                PositionX = offsetX + i * (oneWidth + distance) + oneWidth / 2,
                PositionY = offsetY + row * (oneWidth + distance) + oneWidth / 2,
                Col = i,
                Row = j,
            };
            places[i, j] = back;
            return back;
        }


        GemSprite CreateGem(CellType type, int col, int row, BonusType bonusType)
        {
            var gem = CreateGem(type, col, row);
            var bonusView = GetBonusView(bonusType, GemSize / 2, GemSize / 2);
            gem.AddChild(bonusView);
            return gem;
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


        CCSprite GetBonusView(BonusType bonusType, int positionX, int positionY)
        {
            CCRect frame = new CCRect(0, 0, GemSize - 30, GemSize - 30);
            switch (bonusType)
            {
                case BonusType.LineHorizontal:
                    frame = new CCRect(0, 0, GemSize - 20, GemSize - 60);
                    break;
                case BonusType.LineVertical:
                    frame = new CCRect(0, 0, GemSize - 60, GemSize - 20);
                    break;
                case BonusType.Bomb:
                    break;
            }

            var view = new CCSprite(new CCTexture2D(), frame)
            {
                Opacity = 200,
                PositionX = positionX,
                PositionY = positionY,
                Color = CCColor3B.White
            };
            return view;
        }


        CCColor3B GetColor(CellType type)
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


        CCLabelTtf TimerLabel;
        void AddTimerLabel()
        {
            TimerLabel = new CCLabelTtf($"01:00", "arial", 22)
            {
                Color = CCColor3B.Orange,
                HorizontalAlignment = CCTextAlignment.Center,
                VerticalAlignment = CCVerticalTextAlignment.Center,
                AnchorPoint = CCPoint.AnchorMiddle,
                PositionX = VisibleBoundsWorldspace.Center.X,
                PositionY = VisibleBoundsWorldspace.UpperRight.Y - 100
            };
            AddChild(TimerLabel);
        }

        void InitTimer()
        {
            Schedule(OnTimer, 1, 60, 2);
        }

        bool TimeIsOut;
        int seconds;
        void OnTimer(float obj)
        {
            seconds++;
            if (seconds > GameDurationInSeconds)
            {
                seconds = GameDurationInSeconds;
                if (!IsMoving)
                {
                    GameOver();
                }
                TimeIsOut = true;
                return;
            }
            var rest = 60 - seconds;
            var secondsString = rest > 9 ? rest.ToString() : "0" + rest;
            TimerLabel.Text = $"00:{secondsString}";
        }

        void GameOver()
        {
            Window.DefaultDirector.ReplaceScene(GameOverLayer.GameOverLayerScene(Window, GameCore.Points));
        }
    }
}