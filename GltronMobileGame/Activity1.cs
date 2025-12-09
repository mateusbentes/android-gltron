using System;
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
        Icon = "@drawable/Icon",
        MainLauncher = true,
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.SensorLandscape,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout,
        Theme = "@android:style/Theme.NoTitleBar.Fullscreen"
    )]

    public class Activity1 : Activity
    {
        private Game1 _game;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                // Create the game instance
                _game = new Game1();

                // Get the view from MonoGame services and set it as content view
                var gameView = (View)_game.Services.GetService(typeof(View));
                SetContentView(gameView);

                // Run the game
                _game.Run();
            }
            catch (Exception ex)
            {
                ShowErrorScreen(ex);
            }
        }

        private void ShowErrorScreen(Exception ex)
        {
            try
            {
                var errorView = new Android.Widget.TextView(this);
                errorView.Text = $"GLTron Mobile - Initialization Error\n\n" +
                               $"Error Type: {ex.GetType().Name}\n" +
                               $"Message: {ex.Message}\n\n" +
                               $"Please restart the application.";
                errorView.SetTextColor(Android.Graphics.Color.White);
                errorView.SetBackgroundColor(Android.Graphics.Color.DarkRed);
                errorView.Gravity = Android.Views.GravityFlags.Center;
                errorView.SetPadding(20, 20, 20, 20);
                SetContentView(errorView);
            }
            catch (Exception)
            {
                // Ignore secondary errors
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        protected override void OnDestroy()
        {
            try
            {
                _game?.Dispose();
                _game = null;
            }
            catch (Exception ex)
            {
                // Log error if needed
            }
            
            base.OnDestroy();
        }
    }
}
