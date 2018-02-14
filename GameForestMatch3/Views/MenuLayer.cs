using System;
using CocosSharp;
using GameForestMatch3.Views.Elements;

namespace GameForestMatch3.Views
{
    public class MenuLayer : CCLayerColor
    {
        protected override void AddedToScene()
        {
            base.AddedToScene();

            Scene.SceneResolutionPolicy = CCSceneResolutionPolicy.ShowAll;

            var button = new Button("START")
            {
                Position = VisibleBoundsWorldspace.Center,
            };

            button.Triggered += (sender, e) =>
            {
                Window.DefaultDirector.ReplaceScene(MainLayer.GameLayerScene(Window));
            };

            AddChild(button);
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
