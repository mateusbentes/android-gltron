using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;

namespace GltronMobileGame
{
    [Activity(
        Name = "GltronMobileGame.Activity1",
        Label = "@string/app_name",
        MainLauncher = true,
        Icon = "@drawable/icon",
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.SensorLandscape,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout,
        Theme = "@android:style/Theme.NoTitleBar.Fullscreen"
    )]
    public class Activity1 : Activity
    {
        private Game _game;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Fullscreen flags
            Window?.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

            // Create the game
            _game = new SimpleGame();

            // Force creation and obtain the Android view via services (MonoGame Android 3.8 style)
            var _ = _game.Window?.Handle; // touch handle to force platform init
            var view = (View)_game.Services.GetService(typeof(View));
            if (view == null)
            {
                // Try again once after forcing handle
                _ = _game.Window?.Handle;
                view = (View)_game.Services.GetService(typeof(View));
            }

            if (view != null)
            {
                SetContentView(view);
                _game.Run();
            }
            else
            {
                Android.Util.Log.Error("GLTRON", "Game view is null; finishing Activity to avoid silent kill.");
                Finish();
            }
        }
    }
}
