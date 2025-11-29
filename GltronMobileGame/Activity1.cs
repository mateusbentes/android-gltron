using Android.App;
using Android.Content.PM;
using Android.OS;
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
            try
            {
                Android.Util.Log.Info("GLTRON", "Activity OnCreate start");
                base.OnCreate(bundle);

                // Set fullscreen flags
                Window?.SetFlags(Android.Views.WindowManagerFlags.Fullscreen, Android.Views.WindowManagerFlags.Fullscreen);

                // Create the MonoGame Game and set content view to its window
                _game = new SimpleGame();

                // Obtain the Android view from the Game's services and set it as content view
                var view = (Android.Views.View)_game.Services.GetService(typeof(Android.Views.View));
                if (view == null)
                {
                    // Force window/view creation by touching the handle, then try again
                    var _ = _game.Window?.Handle;
                    view = (Android.Views.View)_game.Services.GetService(typeof(Android.Views.View));
                }
                if (view != null)
                {
                    SetContentView(view);
                }

                _game.Run();

                Android.Util.Log.Info("GLTRON", "Activity OnCreate complete");
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"Activity OnCreate failed: {ex}");
                Finish();
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                Android.Util.Log.Info("GLTRON", "Activity OnDestroy");
                _game?.Dispose();
                base.OnDestroy();
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"Activity OnDestroy error: {ex}");
            }
        }
    }
}
