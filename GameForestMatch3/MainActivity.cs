using Android.App;
using Android.OS;
using Android.Content.PM;
using CocosSharp;
using Microsoft.Xna.Framework;

namespace GameForestMatch3
{
    // the ConfigurationChanges flags set here keep the EGL context
    // from being destroyed whenever the device is rotated or the
    // keyboard is shown (highly recommended for all GL apps)
    [Activity(Label = "GameForestMatch3",
        AlwaysRetainTaskState = true,
        Icon = "@mipmap/icon",
        Theme = "@android:style/Theme.NoTitleBar",
        ScreenOrientation = ScreenOrientation.Portrait,
        LaunchMode = LaunchMode.SingleInstance,
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
    
    public class MainActivity : AndroidGameActivity
    {
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var application = new CCApplication();
            application.ApplicationDelegate = new CocosApplicationDelegate();
            SetContentView(application.AndroidContentView);
            application.StartGame();
        }
    }
}


