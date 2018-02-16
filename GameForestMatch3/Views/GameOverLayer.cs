using System;
using CocosSharp;
using GameForestMatch3.Views.Elements;

namespace GameForestMatch3.Views
{
    public class GameOverLayer : CCLayerColor
    {
        int Score;
        public GameOverLayer(int score) {
            Score = score;
        }

        Button OkButton;
        protected override void AddedToScene()
        {
            base.AddedToScene();

            Scene.SceneResolutionPolicy = CCSceneResolutionPolicy.ShowAll;


            var label = new CCLabelTtf($"GAME OVER\n\nSCORE:\n{Score}", "arial", 22)
            {
                Color = CCColor3B.Orange,
                HorizontalAlignment = CCTextAlignment.Center,
                VerticalAlignment = CCVerticalTextAlignment.Center,
                AnchorPoint = CCPoint.AnchorMiddle,
                PositionX = VisibleBoundsWorldspace.Center.X,
                PositionY = VisibleBoundsWorldspace.Center.Y + 200,
            };
            AddChild(label);

            OkButton = new Button("OK")
            {
                Position = VisibleBoundsWorldspace.Center,
            };

            OkButton.Triggered += OnStartAgain;
            AddChild(OkButton);
        }

        private void OnStartAgain(object sender, EventArgs e)
        {
            Window.DefaultDirector.ReplaceScene(MenuLayer.GameStartLayerScene(Window));
        }

        public override void Cleanup()
        {
            base.Cleanup();
            OkButton.Triggered -= OnStartAgain;
            RemoveAllChildren(true);
        }

        public static CCScene GameOverLayerScene(CCWindow mainWindow, int score)
        {
            var scene = new CCScene(mainWindow);
            var layer = new GameOverLayer(score);

            scene.AddChild(layer);

            return scene;
        }
    }
}
