using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using GltronMobileGame;
using System;
using Java.Lang;

namespace gltron.org.gltronmobile
{
    [Activity(
        Label = "@string/app_name",
        Icon = "@drawable/Icon",
        MainLauncher = true,
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.Landscape, // Changing to Landscape
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout,
        Theme = "@android:style/Theme.NoTitleBar.Fullscreen"
    )]
    public class Activity1 : Activity
    {
        private Game1 _game;
        
        // CRITICAL: Load native libraries before FNA initialization
        static Activity1()
        {
            try
            {
                // Load FNA native libraries in correct order
                System.loadLibrary("SDL2");
                System.loadLibrary("openal");
                System.loadLibrary("theoraplay");
                System.Diagnostics.Debug.WriteLine("FNA native libraries loaded successfully");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load FNA native libraries: {ex}");
                // Continue anyway - libraries might be loaded differently
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== FNA GAME1 INITIALIZATION ===");
                System.Diagnostics.Debug.WriteLine("Activity.OnCreate starting...");

                // Call base.OnCreate first
                base.OnCreate(bundle);
                System.Diagnostics.Debug.WriteLine("Activity.OnCreate completed");

                System.Diagnostics.Debug.WriteLine("Step 1: Setting up FNA environment...");
                // CRITICAL: FNA requires proper environment setup before creating the game
                SetupFNAEnvironment();

                System.Diagnostics.Debug.WriteLine("Step 2: Creating Game1 instance...");
                try
                {
                    _game = new Game1();
                    System.Diagnostics.Debug.WriteLine("Game1 instance created successfully");
                }
                catch (System.TypeInitializationException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"CRITICAL: TypeInitializationException in Game1 creation: {ex}");
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException}");
                    throw new System.Exception($"FNA initialization failed: {ex.InnerException?.Message ?? ex.Message}", ex);
                }

                System.Diagnostics.Debug.WriteLine("Step 3: Registering activity in game services...");
                _game.Services.AddService(typeof(Activity), this);

                System.Diagnostics.Debug.WriteLine("Step 4: Starting FNA game loop...");
                _game.Run();
                System.Diagnostics.Debug.WriteLine("FNA game loop started");

                System.Diagnostics.Debug.WriteLine("FNA initialized successfully!");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("=== FNA INITIALIZATION EXCEPTION ===");
                System.Diagnostics.Debug.WriteLine($"EXCEPTION: {ex}");
                ShowErrorScreen(ex);
            }
        }

        private void SetupFNAEnvironment()
        {
            try
            {
                // CRITICAL: Set FNA environment variables BEFORE any FNA initialization
                System.Environment.SetEnvironmentVariable("FNA_PLATFORM_BACKEND", "SDL2");
                System.Environment.SetEnvironmentVariable("FNA_AUDIO_BACKEND", "OpenAL");
                System.Environment.SetEnvironmentVariable("FNA_GRAPHICS_BACKEND", "OpenGL");
                
                // CRITICAL: Set Android-specific FNA environment variables
                System.Environment.SetEnvironmentVariable("FNA_OPENGL_FORCE_ES3", "1");
                System.Environment.SetEnvironmentVariable("FNA_OPENGL_FORCE_COMPATIBILITY_PROFILE", "1");
                
                // CRITICAL: Initialize FNA platform BEFORE creating Game instance
                // This must be done before any Microsoft.Xna.Framework types are used
                try
                {
                    var fnaType = System.Type.GetType("Microsoft.Xna.Framework.FNAPlatform, FNA");
                    if (fnaType != null)
                    {
                        // Call FNAPlatform.PrepareWindowInfo with Android activity
                        var prepareMethod = fnaType.GetMethod("PrepareWindowInfo", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                        if (prepareMethod != null)
                        {
                            prepareMethod.Invoke(null, new object[] { });
                            System.Diagnostics.Debug.WriteLine("FNA PrepareWindowInfo called successfully");
                        }
                        
                        // Set Android activity context
                        var setActivityMethod = fnaType.GetMethod("SetActivity", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                        if (setActivityMethod != null)
                        {
                            setActivityMethod.Invoke(null, new object[] { this });
                            System.Diagnostics.Debug.WriteLine("FNA activity set successfully");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"FNA platform initialization failed: {ex}");
                    // This is critical - if FNA platform init fails, the app won't work
                    throw;
                }

                System.Diagnostics.Debug.WriteLine("FNA environment setup completed successfully");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CRITICAL: FNA environment setup failed: {ex}");
                throw; // Don't continue if FNA setup fails
            }
        }

        private void ShowErrorScreen(System.Exception ex)
        {
            try
            {
                var errorView = new Android.Widget.TextView(this)
                {
                    Text = $"GLTron Mobile - FNA Initialization Error\n\n" +
                           $"Error Type: {ex.GetType().Name}\n" +
                           $"Message: {ex.Message}\n\n" +
                           $"FNA migration in progress.\n" +
                           $"Please restart the application.",
                    TextAlignment = TextAlignment.Center,
                    TextSize = 16,

                };
                errorView.SetTextColor(Android.Graphics.Color.White);
                errorView.SetBackgroundColor(Android.Graphics.Color.DarkRed);
                errorView.SetPadding(20, 20, 20, 20);
                SetContentView(errorView);
                System.Diagnostics.Debug.WriteLine("Error view displayed");
            }
            catch (System.Exception ex2)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to show error view: {ex2}");
            }
        }

        protected override void OnPause()
        {
            System.Diagnostics.Debug.WriteLine("Activity1.OnPause - FNA Direct Activity Management");
            // With FNA and direct activity management, we have full control
            try
            {
                // FNA handles pausing automatically when the activity pauses
                // We don't need to manually call OnDeactivated - FNA handles this internally
                System.Diagnostics.Debug.WriteLine("FNA will handle pause automatically");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FNA pause error: {ex}");
            }
            base.OnPause();
        }

        protected override void OnResume()
        {
            System.Diagnostics.Debug.WriteLine("Activity1.OnResume - FNA Direct Activity Management");
            base.OnResume();
            try
            {
                // FNA handles resuming automatically when the activity resumes
                // We don't need to manually call OnActivated - FNA handles this internally
                System.Diagnostics.Debug.WriteLine("FNA will handle resume automatically");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FNA resume error: {ex}");
            }
        }

        protected override void OnDestroy()
        {
            System.Diagnostics.Debug.WriteLine("Activity1.OnDestroy - FNA Direct Activity Management");
            try
            {
                _game?.Dispose();
                System.Diagnostics.Debug.WriteLine("FNA game disposed successfully");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FNA disposal error: {ex}");
            }
            base.OnDestroy();
        }
    }
}
