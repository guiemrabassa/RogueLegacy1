#define MOBILE

using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using InputSystem;
using Java.Lang;
using Microsoft.Xna.Framework;

namespace RogueCastle.Android
{
    [Activity(
        Label = "Rogue Legacy",
        MainLauncher = true,
        Icon = "@drawable/icon",
        Theme = "@style/Theme.Splash",
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.SensorLandscape,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden,
        Immersive = true
    )]
    public class Activity1 : AndroidGameActivity
    {
        private Game _game;
        private View _view;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            LevelEV.LoadRetail();

            _game = new Game();
            _view = _game.Services.GetService(typeof(View)) as View;
            _game.graphics.IsFullScreen = true;

            SetContentView(_view);

            _game.Run();
        }
    }
}
