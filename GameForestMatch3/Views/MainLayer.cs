using System;
using System.Collections.Generic;
using CocosSharp;
using GameForestMatch3.Logic;
using GameForestMatch3.Views.Elements;

namespace GameForestMatch3.Views
{
    public class MainLayer : CCLayerColor
    {
        public MainLayer() : base()
        {

        }
        float CellAnimationDuration = 0.3f;
        CellSprite[,] places;
        CellSprite[,] gems;
        Logic.Field GameCore;
        protected override void AddedToScene()
        {
            base.AddedToScene();

            Scene.SceneResolutionPolicy = CCSceneResolutionPolicy.ShowAll;
            GameCore = new Logic.Field();
            places = new CellSprite[Logic.Field.SIZE, Logic.Field.SIZE];
            gems = new CellSprite[Logic.Field.SIZE, Logic.Field.SIZE];
            GameCore.TouchedChanged += (sender, e) =>
            {
                ClearBackgroundColor();
                CellSprite gem1 = null;
                CellSprite gem2 = null;
                int col;
                int row;
                if (GameCore.LastTouched != null)
                {
                    col = GameCore.LastTouched.Col;
                    row = GameCore.LastTouched.Row;
                    places[col, row].Color = CCColor3B.Orange;
                    gem1 = gems[col, row];
                }

                if (GameCore.FirstTouched != null)
                {
                    col = GameCore.FirstTouched.Col;
                    row = GameCore.FirstTouched.Row;
                    places[col, row].Color = CCColor3B.Orange;
                    gem2 = gems[col, row];
                }

                if (gem1 != null && gem2 != null)
                {
                    var point1 = new CCPoint(gem2.PositionX, gem2.PositionY);
                    var point2 = new CCPoint(gem1.PositionX, gem1.PositionY);

                    var moveAction = new CCMoveTo(CellAnimationDuration, point1);
                    var moveCompletedAction = new CCCallFunc(() =>
                    {
                        ClearBackgroundColor();

                        SwapGems(gem1, gem2);


                        GameCore.ResetSelected();
                        GameCore.DestroyMatches();
                    });
                    CCSequence mySequence = new CCSequence(moveAction, moveCompletedAction);

                    gem1.RunAction(mySequence);
                    gem2.AddAction(new CCMoveTo(CellAnimationDuration, point2));


                }
            };

            GameCore.CellDeleted += (sender, cell) =>
            {
                var col = cell.Col;
                var row = cell.Row;

                RemoveChild(gems[col, row]);
                gems[col, row] = null;

                StartCreationTimer();
            };


            GameCore.MovedDown += (sender, list) =>
            {

                foreach (var cell in list)
                {
                    var col = cell.Col;
                    var row = cell.Row;
                    var newRow = row++;
                    var point = new CCPoint(places[col, newRow].PositionX, places[col, newRow].PositionY);
                    var gem = gems[col, row];
                    gem.Row = newRow;
                    gem.AddAction(new CCMoveTo(CellAnimationDuration, point));
                }
            };


            GameCore.CellCreated += (sender, cell) =>
            {
                var gem = new CellSprite(new CCTexture2D(), new CCRect(0, 0, GemSize, GemSize))
                {
                    Color = GetColor(cell.Type),
                    PositionX = places[cell.Col, cell.Row].PositionX,
                    PositionY = places[cell.Col, cell.Row].PositionY,
                };
                gems[cell.Col, cell.Row] = gem;
                AddChild(gem);
            };
            CreateField();
            AddScoreLabel();
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
            });
            CCSequence sequence = new CCSequence(delayAction, delayCompletedAction);
            RunAction(sequence);

        }

        private void SwapGems(CellSprite gem1, CellSprite gem2)
        {
            var tmpGem = gems[gem1.Col, gem1.Row];
            gems[gem1.Col, gem1.Row] = gems[gem2.Col, gem2.Row];
            gems[gem2.Col, gem2.Row] = tmpGem;

            var tmpCol = gem1.Col;
            var tmpRow = gem1.Row;
            gem1.Col = gem2.Col;
            gem1.Row = gem2.Row;
            gem2.Col = tmpCol;
            gem2.Row = tmpRow;
        }

        private void ClearBackgroundColor()
        {
            foreach (var place in places)
            {
                place.Color = CCColor3B.Gray;
            }
        }

        public static CCScene GameLayerScene(CCWindow mainWindow)
        {
            var scene = new CCScene(mainWindow);
            var layer = new MainLayer();

            scene.AddChild(layer);

            return scene;
        }

        int GemSize;

        void CreateField()
        {
            var width = VisibleBoundsWorldspace.Size.Width;
            var height = VisibleBoundsWorldspace.Size.Height;
            var distance = 2;
            var distancesWidth = (Logic.Field.SIZE - 1) * distance;

            var fieldWidth = width * (float)0.9;
            var offsetX = (width - fieldWidth - distancesWidth) / 2;
            var offsetY = (height - fieldWidth - distancesWidth) / 2;



            var oneWidth = (fieldWidth - distance) / Logic.Field.SIZE;
            GemSize = (int)oneWidth - 20;


            for (var i = 0; i < Logic.Field.SIZE; i++)
            {
                for (var j = 0; j < Logic.Field.SIZE; j++)
                {
                    var back = new CellSprite(new CCTexture2D(), new CCRect(0, 0, oneWidth, oneWidth))
                    {
                        Color = CCColor3B.Gray,
                    };
                    back.PositionX = offsetX + i * (oneWidth + distance) + oneWidth / 2;
                    back.PositionY = offsetY + j * (oneWidth + distance) + oneWidth / 2;
                    back.Col = i;
                    back.Row = j;


                    var gem = new CellSprite(new CCTexture2D(), new CCRect(0, 0, GemSize, GemSize))
                    {
                        Color = GetColor(GameCore.Grid[i, j].Type),
                    };
                    gem.PositionX = back.PositionX;
                    gem.PositionY = back.PositionY;
                    gem.Col = i;
                    gem.Row = j;
                    gems[i, j] = gem;

                    places[i, j] = back;


                    AddChild(back);

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

        private CCColor3B GetColor(CellType type)
        {
            switch (type)
            {
                case CellType.BlueRomb:
                    return CCColor3B.Blue;
                case CellType.YellowSquare:
                    return CCColor3B.Yellow;
                case CellType.RedCircle:
                    return CCColor3B.Red;
                case CellType.PurpleBackTriangle:
                    return CCColor3B.Magenta;
                case CellType.GreenTriangle:
                    return CCColor3B.Green;
                default:
                    return CCColor3B.DarkGray;
            }
        }

        private bool TouchBegan(CCTouch touch, CCEvent ev)
        {
            foreach (var place in places)
            {
                if (place.BoundingBoxTransformedToWorld.ContainsPoint(touch.Location))
                {
                    GameCore.Touch(place.Col, place.Row);
                    return true;
                }
            }
            return false;
        }


        void AddScoreLabel()
        {
            var label = new CCLabelTtf($"SCORE: {GameCore.Points}", "arial", 22)
            {
                Color = CCColor3B.Orange,
                HorizontalAlignment = CCTextAlignment.Center,
                VerticalAlignment = CCVerticalTextAlignment.Center,
                AnchorPoint = CCPoint.AnchorMiddle
            };

            GameCore.PointsChanged += (sender, e) => { label.Text = $"SCORE: {GameCore.Points}"; };

            label.PositionX = VisibleBoundsWorldspace.Center.X;
            label.PositionY = VisibleBoundsWorldspace.UpperRight.Y - 50;

            AddChild(label);
        }

    }
}