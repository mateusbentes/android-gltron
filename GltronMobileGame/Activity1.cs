using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using GltronMobileGame;

namespace gltron.org.gltronmobile
{
    [Activity(
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

            try
            {
                Android.Util.Log.Info("GLTRON", "Activity1.OnCreate started");

                // Fullscreen flags
                Android.Util.Log.Info("GLTRON", "Setting fullscreen flags");
                Window?.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

                // Create the game
                Android.Util.Log.Info("GLTRON", "Creating SimpleGame instance");
                _game = new SimpleGame();
                Android.Util.Log.Info("GLTRON", "SimpleGame instance created successfully");

                // Start the game loop first (this will create & register the View internally)
                Android.Util.Log.Info("GLTRON", "Starting game loop");
                _game.Run();
                Android.Util.Log.Info("GLTRON", "Game loop started successfully");

                // Now poll for the view with timeout
                View view = null;
                const int timeoutMs = 2000;
                var sw = System.Diagnostics.Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < timeoutMs && view == null)
                {
                    view = _game.Services.GetService(typeof(View)) as View;
                    if (view == null)
                        System.Threading.Thread.Sleep(50);
                }

                if (view == null)
                {
                    Android.Util.Log.Error("GLTRON", "Game view never became available after timeout.");
                    Finish();
                    return;
                }

                // Set the content view
                SetContentView(view);
                Android.Util.Log.Info("GLTRON", "Game view successfully set as content view.");
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"OnCreate failed - Exception: {ex.GetType().Name}");
                Android.Util.Log.Error("GLTRON", $"OnCreate failed - Message: {ex.Message}");
                Android.Util.Log.Error("GLTRON", $"OnCreate failed - StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Android.Util.Log.Error("GLTRON", $"OnCreate failed - InnerException: {ex.InnerException}");
                }
                Finish();
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            Android.Util.Log.Info("GLTRON", "Activity1.OnPause");
        }

        protected override void OnResume()
        {
            base.OnResume();
            Android.Util.Log.Info("GLTRON", "Activity1.OnResume");
        }

        protected override void OnDestroy()
        {
            Android.Util.Log.Info("GLTRON", "Activity1.OnDestroy");
            _game?.Dispose();
            base.OnDestroy();
        }
    }
}
