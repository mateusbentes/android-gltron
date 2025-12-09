using System;
using Android.App;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using GltronMobileGame;

namespace gltron.org.gltronmobile
{
    [Activity(
        Label = "GLTron Mobile",
        MainLauncher = true,
        Theme = "@android:style/Theme.NoTitleBar.Fullscreen",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape
    )]
    public class Activity1 : Activity
    {
        private Game1 _game;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                // Enable edge-to-edge fullscreen
                Window?.SetFlags(
                    WindowManagerFlags.Fullscreen,
                    WindowManagerFlags.Fullscreen
                );

                Window?.SetFlags(
                    WindowManagerFlags.KeepScreenOn,
                    WindowManagerFlags.KeepScreenOn
                );

                // Create the game instance
                _game = new Game1();

                // Debug: Check what services are available
                Android.Util.Log.Debug("GLTRON", "=== Available Services ===");
                foreach (var service in _game.Services)
                {
                    Android.Util.Log.Debug("GLTRON", $"Service: {service.GetType().FullName}");
                }

                // Try to get the view from MonoGame services
                var gameView = _game.Services.GetService(typeof(View)) as View;
                Android.Util.Log.Debug("GLTRON", $"GameView from services: {gameView?.GetType().FullName ?? "NULL"}");

                if (gameView != null)
                {
                    Android.Util.Log.Debug("GLTRON", "Using view from services");
                    SetContentView(gameView);
                }
                else
                {
                    Android.Util.Log.Debug("GLTRON", "No view available from services - MonoGame .NET 9 behavior");
                    // For now, let's see what happens if we just run the game
                }

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
