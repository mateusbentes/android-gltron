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

            // Custom host view approach for MonoGame
            _game = new SimpleGame();

            // Force creation of the Game's window and associated Android view
            var _ = _game.Window?.Handle;
            var view = (View)_game.Services.GetService(typeof(View));
            if (view == null)
            {
                // Touch handle again in case it was lazy
                _ = _game.Window?.Handle;
                view = (View)_game.Services.GetService(typeof(View));
            }

            if (view != null)
            {
                SetContentView(view);
            }

            // Start the game loop
            _game.Run();
        }
    }
}
