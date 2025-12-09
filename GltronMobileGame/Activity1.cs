using System;
using Android.App;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using GltronMobileGame;
using System.Reflection;

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
        private AndroidGameView _gameView;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Enable fullscreen and keep the screen on while playing.
            Window?.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            Window?.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);

            try
            {
                Android.Util.Log.Debug("GLTRON", "Trying MonoGame .NET 9 official approach...");
                
                // Try the official MonoGame .NET 9 approach - let the game handle its own view creation
                Android.Util.Log.Debug("GLTRON", "Creating Game1 and letting it handle Android integration...");
                
                _game = new Game1();
                Android.Util.Log.Debug("GLTRON", "Game1 created successfully!");
                
                // Try to get the view that MonoGame should create automatically
                Android.Util.Log.Debug("GLTRON", "Checking if MonoGame created a view automatically...");
                var gameView = _game.Services.GetService(typeof(Android.Views.View)) as Android.Views.View;
                
                if (gameView != null)
                {
                    Android.Util.Log.Debug("GLTRON", "SUCCESS: MonoGame created view automatically!");
                    SetContentView(gameView);
                    _game.Run();
                }
                else
                {
                    Android.Util.Log.Debug("GLTRON", "MonoGame didn't create view automatically, trying Run() directly...");
                    // Some MonoGame versions handle view creation in Run()
                    _game.Run();
                }
            }
            catch (Exception ex)
            {
                // If initialization fails, show a readable on-screen error instead of a silent crash.
                ShowErrorScreen(ex);
            }
        }

        /// <summary>
        /// Displays a simple centered error message if the game fails to initialize.
        /// This avoids black-screen crashes and gives immediate feedback to the user.
        /// </summary>
        private void ShowErrorScreen(Exception ex)
        {
            var errorView = new Android.Widget.TextView(this);
            errorView.Text = $"GLTron Mobile - Initialization Error\n\n{ex}";
            errorView.SetTextColor(Android.Graphics.Color.White);
            errorView.SetBackgroundColor(Android.Graphics.Color.DarkRed);
            errorView.Gravity = GravityFlags.Center;
            errorView.SetPadding(20, 20, 20, 20);

            SetContentView(errorView);
        }

        protected override void OnPause()
        {
            base.OnPause();

            // Pause rendering + GL thread safely.
            // This prevents crashes on home-button press or app minimization.
            _gameView?.Pause();
        }

        protected override void OnResume()
        {
            base.OnResume();

            // Resume rendering + GL thread.
            // This restores the MonoGame frame loop on returning to the app.
            _gameView?.Resume();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _gameView = null;
            _game?.Dispose();
            _game = null;
        }
    }
}
