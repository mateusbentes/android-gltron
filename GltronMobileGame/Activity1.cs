using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GltronMobileGame;
using System;
using System.Reflection;

// Define the namespace for the GLTron mobile game.
// This groups related classes and avoids naming conflicts with other projects.
namespace gltron.org.gltronmobile
{
    // The [Activity] attribute defines this class as an Android Activity, which is the entry point for the app.
    // It specifies how the Activity should behave and appear in the Android system.
    [Activity(
        Label = "@string/app_name", // Sets the app name (defined in Resources/values/strings.xml)
        Icon = "@drawable/Icon",    // Sets the app icon (defined in Resources/drawable/Icon.png)
        MainLauncher = true,        // Marks this Activity as the main entry point when the app starts
        AlwaysRetainTaskState = true, // Ensures the app retains its state when sent to the background
        LaunchMode = LaunchMode.SingleInstance, // Ensures only one instance of this Activity runs at a time
        ScreenOrientation = ScreenOrientation.Landscape, // Forces the app to run in landscape mode
        // Specifies which configuration changes the Activity will handle itself (without restarting)
        ConfigurationChanges =
            ConfigChanges.Orientation |          // Handles screen orientation changes
            ConfigChanges.Keyboard |             // Handles keyboard visibility changes
            ConfigChanges.KeyboardHidden |       // Handles keyboard hidden state changes
            ConfigChanges.ScreenSize |           // Handles screen size changes (e.g., multi-window mode)
            ConfigChanges.ScreenLayout,          // Handles screen layout changes
        Theme = "@android:style/Theme.NoTitleBar.Fullscreen" // Uses a fullscreen theme without a title bar
    )]
    // Activity1 inherits directly from Android.App.Activity for complete control.
    // We manually implement all MonoGame integration without using AndroidGameActivity.
    public class Activity1 : Activity
    {
        private Game1 _game;  // Instance of the MonoGame game (Game1 class)
        private View _view;   // The Android View that will render the MonoGame content

        // OnCreate is called when the Activity is first created.
        // This is where you initialize the game and set up the UI.
        // We manually implement MonoGame integration without AndroidGameActivity.
        protected override void OnCreate(Bundle bundle)
        {
            // Call the base class's OnCreate method to ensure proper initialization.
            base.OnCreate(bundle);

            try
            {
                System.Diagnostics.Debug.WriteLine("=== DIRECT ACTIVITY MONOGAME INITIALIZATION ===");

                // CRITICAL: Set up MonoGame's static activity reference before creating Game1
                // This is what AndroidGameActivity does internally
                SetMonoGameActivity();

                // Step 1: Create the Game1 instance
                _game = new Game1();

                // Step 2: Register this activity in the game services
                _game.Services.AddService(typeof(Activity), this);

                // Step 3: Start the MonoGame game loop
                _game.Run();

                // Step 4: Get the view that MonoGame created
                _view = _game.Services.GetService<View>();
                if (_view != null)
                {
                    System.Diagnostics.Debug.WriteLine("DIRECT ACTIVITY: Successfully retrieved MonoGame view");
                }

                System.Diagnostics.Debug.WriteLine("DIRECT ACTIVITY: MonoGame initialization completed successfully");
            }
            catch (System.Exception ex)
            {
                // Log any exceptions that occur during initialization.
                System.Diagnostics.Debug.WriteLine($"DIRECT ACTIVITY EXCEPTION: {ex}");

                // Display an error screen to the user if initialization fails.
                ShowErrorScreen(ex);
            }
        }

        // Set up MonoGame's internal activity reference using reflection
        private void SetMonoGameActivity()
        {
            try
            {
                // Try to find and set MonoGame's internal activity reference
                var gameType = typeof(Microsoft.Xna.Framework.Game);
                var activityField = gameType.GetField("Activity", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (activityField != null)
                {
                    activityField.SetValue(null, this);
                    System.Diagnostics.Debug.WriteLine("DIRECT ACTIVITY: Set MonoGame static Activity reference");
                    return;
                }

                // Try alternative field names
                activityField = gameType.GetField("_activity", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                if (activityField != null)
                {
                    activityField.SetValue(null, this);
                    System.Diagnostics.Debug.WriteLine("DIRECT ACTIVITY: Set MonoGame static _activity reference");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("DIRECT ACTIVITY: Could not find MonoGame activity field, continuing anyway");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DIRECT ACTIVITY: SetMonoGameActivity failed: {ex.Message}");
                // Continue anyway - MonoGame might still work
            }
        }

        // ShowErrorScreen displays a user-friendly error message if the game fails to initialize.
        private void ShowErrorScreen(System.Exception ex)
        {
            // Create a TextView to display the error message.
            var errorView = new Android.Widget.TextView(this)
            {
                Text = $"GLTron Mobile - Initialization Error\n\n{ex.Message}", // Error message
                TextAlignment = TextAlignment.Center, // Center-align the text
                TextSize = 16                        // Set text size
            };
            
            // Set colors using methods (not properties)
            errorView.SetTextColor(Android.Graphics.Color.White);
            errorView.SetBackgroundColor(Android.Graphics.Color.DarkRed);

            // Add padding around the text for better readability.
            errorView.SetPadding(20, 20, 20, 20);

            // Set the error view as the content of the Activity.
            SetContentView(errorView);
        }

        // OnPause is called when the Activity is paused (e.g., when the app is sent to the background).
        // This is where you pause the game to save resources.
        protected override void OnPause()
        {
            try
            {
                // MonoGame's Game class should automatically handle pausing when the activity pauses.
                // Since we registered this Activity in the services, MonoGame can detect lifecycle changes.
                System.Diagnostics.Debug.WriteLine("DIRECT ACTIVITY: Activity pausing - MonoGame should handle game pause automatically");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DIRECT ACTIVITY OnPause error: {ex.Message}");
            }

            // Call the base class's OnPause method.
            base.OnPause();
        }

        // OnResume is called when the Activity resumes (e.g., when the app returns to the foreground).
        // This is where you resume the game.
        protected override void OnResume()
        {
            // Call the base class's OnResume method.
            base.OnResume();

            try
            {
                // MonoGame's Game class should automatically handle resuming when the activity resumes.
                // Since we registered this Activity in the services, MonoGame can detect lifecycle changes.
                System.Diagnostics.Debug.WriteLine("DIRECT ACTIVITY: Activity resuming - MonoGame should handle game resume automatically");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DIRECT ACTIVITY OnResume error: {ex.Message}");
            }
        }

        // OnDestroy is called when the Activity is being destroyed (e.g., when the app is closed).
        // This is where you clean up resources.
        protected override void OnDestroy()
        {
            try
            {
                // Dispose of the game to release resources (e.g., textures, sounds).
                // This is important to prevent memory leaks.
                _game?.Dispose();
                System.Diagnostics.Debug.WriteLine("DIRECT ACTIVITY: Game disposed successfully");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DIRECT ACTIVITY OnDestroy game disposal error: {ex.Message}");
            }

            // Call the base class's OnDestroy method.
            base.OnDestroy();
        }
    }
}
