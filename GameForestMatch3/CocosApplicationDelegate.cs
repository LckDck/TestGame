﻿using CocosDenshion;
using CocosSharp;
using GameForestMatch3.Views;

namespace GameForestMatch3
{
    public class CocosApplicationDelegate : CCApplicationDelegate
    {

        public override void ApplicationDidFinishLaunching(CCApplication application, CCWindow mainWindow)
        {
            application.ContentRootDirectory = "Content";
            application.PreferMultiSampling = false;
            CCSize winSize = mainWindow.WindowSizeInPixels;
            float desiredWidth = 640.0f;
            float desiredHeight = 960.0f;

            CCScene.SetDefaultDesignResolution(desiredWidth, desiredHeight, CCSceneResolutionPolicy.ExactFit);

            CCScene scene = MenuLayer.GameStartLayerScene(mainWindow);
            mainWindow.RunWithScene(scene);
        }

        public override void ApplicationDidEnterBackground(CCApplication application)
        {
            // stop all of the animation actions that are running.
            application.Paused = true;

            // if you use SimpleAudioEngine, your music must be paused
            CCSimpleAudioEngine.SharedEngine.PauseBackgroundMusic();
        }

        public override void ApplicationWillEnterForeground(CCApplication application)
        {
            application.Paused = false;

            // if you use SimpleAudioEngine, your background music track must resume here.
            CCSimpleAudioEngine.SharedEngine.ResumeBackgroundMusic();
        }
    }
}