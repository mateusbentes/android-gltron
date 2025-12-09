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
        private AndroidGameView _gameView;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Enable fullscreen and keep the screen on while playing.
            Window?.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            Window?.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);

            try
            {
                // Create the MonoGame Game instance.
                // Note: MonoGame no longer automatically attaches a view or starts the game loop on Android (.NET 8/9),
                // so the Activity is responsible for providing a GL surface and driving the frame updates.
                _game = new Game1();

                // Create our custom AndroidGameView wrapper.
                // This class creates a GLSurfaceView and uses its render thread to call RunOneFrame(),
                // becoming the primary driver of the MonoGame update/draw loop.
                _gameView = new AndroidGameView(this, _game);

                // Attach the custom view to the Activity.
                // IMPORTANT: Do NOT call _game.Run() on Android. 
                // The game loop is driven automatically by AndroidGameView's OpenGL callbacks.
                SetContentView(_gameView);
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
            var errorView = new Android.Widget.TextView(this)
            {
                Text = $"GLTron Mobile - Initialization Error\n\n{ex}",
                Gravity = GravityFlags.Center
            };

            errorView.SetTextColor(Android.Graphics.Color.White);
            errorView.SetBackgroundColor(Android.Graphics.Color.DarkRed);
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

            // When the Activity is destroyed, dispose the Game instance.
            // DO NOT manually dispose the AndroidGameView or underlying GL surface;
            // Android handles teardown of GLSurfaceView and EGL contexts automatically.
            _game?.Dispose();
            _game = null;

            _gameView = null; // Allow GC cleanup
        }
    }
}
