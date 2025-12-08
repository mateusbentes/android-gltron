using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using GltronMobileGame;

namespace gltron.org.gltronmobile
{
    [Activity(
        Label = "@string/app_name",
        Icon = "@drawable/Icon",
        MainLauncher = true,
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.SensorLandscape,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout,
        Theme = "@android:style/Theme.NoTitleBar.Fullscreen"
    )]
    public class Activity1 : AndroidGameActivity
    {
        private Game1 _game;

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
#if ANDROID
                Android.Util.Log.Info("GLTRON", "=== MONOGAME GAME1 INITIALIZATION ===");


#else
                System.Diagnostics.Debug.WriteLine("GLTRON: === MONOGAME GAME1 INITIALIZATION ===");
#endif

                base.OnCreate(bundle);





                _game = new Game1();





















                SetContentView((View)_game.Services.GetService(typeof(View)));
                _game.Run();
            }
            catch (System.Exception ex)
            {





#if ANDROID
                Android.Util.Log.Error("GLTRON", $"Initialization failed: {ex}");
#else
                System.Diagnostics.Debug.WriteLine($"GLTRON: Initialization failed: {ex}");
#endif
                ShowErrorScreen(ex);
            }
        }

        private void ShowErrorScreen(System.Exception ex)
        {
            try
            {
                var errorView = new Android.Widget.TextView(this);
                errorView.Text = $"GLTron Mobile - Initialization Error\n\n" +
                               $"Error Type: {ex.GetType().Name}\n" +
                               $"Message: {ex.Message}\n\n" +
                               $"Please restart the application.\n" +
                               $"If the problem persists, try restarting your device.";
                errorView.SetTextColor(Android.Graphics.Color.White);
                errorView.SetBackgroundColor(Android.Graphics.Color.DarkRed);
                errorView.Gravity = Android.Views.GravityFlags.Center;
                errorView.SetPadding(20, 20, 20, 20);
                SetContentView(errorView);

#if ANDROID
                Android.Util.Log.Info("GLTRON", "Error view displayed");
#else
                System.Diagnostics.Debug.WriteLine("GLTRON: Error view displayed");
#endif
            }
            catch (System.Exception ex2)
            {

#if ANDROID
                Android.Util.Log.Error("GLTRON", $"Failed to show error view: {ex2}");
#else
                System.Diagnostics.Debug.WriteLine($"GLTRON: Failed to show error view: {ex2}");
#endif
            }
        }

        protected override void OnPause()
        {

            _game?.OnPause();
            base.OnPause();
        }

        protected override void OnResume()
        {

            _game?.OnResume();
            base.OnResume();
        }

        protected override void OnDestroy()
        {


#if ANDROID
            Android.Util.Log.Info("GLTRON", "Activity1.OnDestroy");
#else
            System.Diagnostics.Debug.WriteLine("GLTRON: Activity1.OnDestroy");
#endif

            try
            {
                _game?.Dispose();
                _game = null;
            }
            catch (System.Exception ex)
            {

#if ANDROID
                Android.Util.Log.Error("GLTRON", $"Error disposing game: {ex}");
#else
                System.Diagnostics.Debug.WriteLine($"GLTRON: Error disposing game: {ex}");
#endif
            }
            
            base.OnDestroy();
        }
    }
}
