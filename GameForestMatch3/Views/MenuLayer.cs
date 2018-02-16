using System;
using CocosSharp;
using GameForestMatch3.Views.Elements;

namespace GameForestMatch3.Views
{
    public class MenuLayer : CCLayerColor
    {
        Button StartButton;
        protected override void AddedToScene()
        {
            base.AddedToScene();

            Scene.SceneResolutionPolicy = CCSceneResolutionPolicy.ShowAll;

            StartButton = new Button("START")
            {
                Position = VisibleBoundsWorldspace.Center,
            };

            StartButton.Triggered += StartClicked;

            AddChild(StartButton);
        }

        void StartClicked(object sender, EventArgs e)
        {
            Window.DefaultDirector.ReplaceScene(MainLayer.GameLayerScene(Window));
        }

        public override void Cleanup()
        {
            base.Cleanup();
            StartButton.Triggered -= StartClicked;
            RemoveAllChildren(true);
        }

        public static CCScene GameStartLayerScene(CCWindow mainWindow)
        {
            var scene = new CCScene(mainWindow);
            var layer = new MenuLayer();
            scene.AddChild(layer);
            return scene;
        }
    }
}
