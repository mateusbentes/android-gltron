using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using GltronMobileGame;
using System;

namespace gltron.org.gltronmobile
{
    [Activity(
        Label = "@string/app_name",
        Icon = "@drawable/Icon",
        MainLauncher = true,
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.Landscape,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout,
        Theme = "@android:style/Theme.NoTitleBar.Fullscreen"
    )]
    public class Activity1 : Activity
    {
        private Game1 _game;

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== GLTron FNA Android Activity Starting ===");
                
                // Call base.OnCreate first
                base.OnCreate(bundle);
                
                // Set up FNA environment variables
                SetupFNAEnvironment();
                
                // Create the game instance
                System.Diagnostics.Debug.WriteLine("Creating Game1 instance...");
                _game = new Game1();
                
                // Start the game - FNA handles the view creation internally
                System.Diagnostics.Debug.WriteLine("Starting FNA game...");
                _game.Run();
                
                System.Diagnostics.Debug.WriteLine("GLTron FNA Activity initialized successfully!");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Activity OnCreate failed: {ex}");
                Android.Util.Log.Error("GLTron", $"Activity OnCreate failed: {ex}");
                ShowErrorScreen(ex);
            }
        }

        private void SetupFNAEnvironment()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Setting up FNA environment variables...");
                
                // Set FNA environment variables for Android
                System.Environment.SetEnvironmentVariable("FNA_PLATFORM_BACKEND", "SDL2");
                System.Environment.SetEnvironmentVariable("FNA_AUDIO_BACKEND", "OpenAL");
                System.Environment.SetEnvironmentVariable("FNA_GRAPHICS_BACKEND", "OpenGL");
                
                // Android-specific OpenGL settings
                System.Environment.SetEnvironmentVariable("FNA_OPENGL_FORCE_ES3", "1");
                System.Environment.SetEnvironmentVariable("FNA_OPENGL_FORCE_COMPATIBILITY_PROFILE", "0");
                
                // Additional FNA settings for better Android compatibility
                System.Environment.SetEnvironmentVariable("SDL_ANDROID_SEPARATE_MOUSE_AND_TOUCH", "1");
                System.Environment.SetEnvironmentVariable("SDL_TOUCH_MOUSE_EVENTS", "0");
                
                System.Diagnostics.Debug.WriteLine("FNA environment variables set successfully");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FNA environment setup failed: {ex}");
                Android.Util.Log.Warn("GLTron", $"FNA environment setup failed: {ex}");
            }
        }

        private void ShowErrorScreen(System.Exception ex)
        {
            try
            {
                var errorView = new Android.Widget.TextView(this)
                {
                    Text = $"GLTron Mobile - Initialization Error\n\n" +
                           $"Error: {ex.GetType().Name}\n" +
                           $"Message: {ex.Message}\n\n" +
                           $"Please restart the application.\n\n" +
                           $"If the problem persists, check that your device supports OpenGL ES 3.0.",
                    TextAlignment = TextAlignment.Center,
                    TextSize = 16,
                };
                errorView.SetTextColor(Android.Graphics.Color.White);
                errorView.SetBackgroundColor(Android.Graphics.Color.DarkRed);
                errorView.SetPadding(20, 20, 20, 20);
                SetContentView(errorView);
                
                Android.Util.Log.Error("GLTron", "Error view displayed");
            }
            catch (System.Exception ex2)
            {
                Android.Util.Log.Error("GLTron", $"Failed to show error view: {ex2}");
            }
        }

        protected override void OnPause()
        {
            System.Diagnostics.Debug.WriteLine("Activity1.OnPause");
            base.OnPause();
        }

        protected override void OnResume()
        {
            System.Diagnostics.Debug.WriteLine("Activity1.OnResume");
            base.OnResume();
        }

        protected override void OnDestroy()
        {
            System.Diagnostics.Debug.WriteLine("Activity1.OnDestroy");
            try
            {
                _game?.Dispose();
                System.Diagnostics.Debug.WriteLine("Game disposed successfully");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Game disposal error: {ex}");
            }
            base.OnDestroy();
        }
    }
}
