using Foundation;
using UIKit;
using System;

namespace GltronMobileGame
{
    [Register("AppDelegate")]
    class Program : UIApplicationDelegate
    {
        private static Game1 game;

        internal static void RunGame()
        {
            try
            {
                FNAHelper.LogInfo("=== FNA iOS APPLICATION STARTING ===");
                
                // Setup FNA environment for iOS
                FNAHelper.SetupFNAEnvironment();
                
                // Verify native frameworks are available
                FNAHelper.LogInfo("Checking iOS framework availability...");
                
                System.Diagnostics.Debug.WriteLine("GLTRON: iOS - Creating Game1 instance...");
                game = new Game1();
                
                if (game == null)
                {
                    throw new InvalidOperationException("Game1 instance creation returned null");
                }
                
                FNAHelper.LogInfo("iOS - Starting FNA game loop...");
                game.Run();
                
                FNAHelper.LogInfo("FNA iOS Application initialized successfully!");
            }
            catch (Exception ex)
            {
                FNAHelper.LogError("=== FNA iOS INITIALIZATION EXCEPTION ===");
                FNAHelper.LogError($"EXCEPTION TYPE: {ex.GetType().FullName}");
                FNAHelper.LogError($"EXCEPTION MESSAGE: {ex.Message}");
                if (ex.InnerException != null)
                {
                    FNAHelper.LogError($"INNER EXCEPTION: {ex.InnerException.GetType().FullName}");
                    FNAHelper.LogError($"INNER MESSAGE: {ex.InnerException.Message}");
                }
                FNAHelper.LogError($"EXCEPTION STACK: {ex.StackTrace}");
                
                // Show error alert on iOS
                var errorMessage = GetDetailedErrorMessage(ex);
                var alert = UIAlertController.Create(
                    "GLTron Mobile - FNA Error", 
                    $"FNA initialization failed:\n{ex.Message}\n\n{errorMessage}\n\nDevice: {UIDevice.CurrentDevice.Model}\niOS: {UIDevice.CurrentDevice.SystemVersion}", 
                    UIAlertControllerStyle.Alert
                );
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                
                // Get the root view controller to present the alert
                var window = UIApplication.SharedApplication.KeyWindow;
                var rootViewController = window?.RootViewController;
                rootViewController?.PresentViewController(alert, true, null);
                
                throw;
            }
        }

        private static string GetDetailedErrorMessage(Exception ex)
        {
            if (ex is System.TypeInitializationException)
            {
                return "FNA Platform Initialization Failed:\n" +
                       "• SDL2 framework missing or incompatible\n" +
                       "• OpenAL framework missing\n" +
                       "• Metal/OpenGL ES not supported\n" +
                       "• iOS framework architecture mismatch";
            }
            else if (ex.Message.Contains("SDL"))
            {
                return "SDL2 Framework Issue:\n" +
                       "• SDL2.framework not found in app bundle\n" +
                       "• Incompatible SDL2 version for iOS\n" +
                       "• Missing ARM64 framework";
            }
            else if (ex.Message.Contains("OpenAL") || ex.Message.Contains("Audio"))
            {
                return "Audio System Issue:\n" +
                       "• OpenAL framework not found\n" +
                       "• Audio permissions missing\n" +
                       "• iOS audio hardware not supported";
            }
            else if (ex.Message.Contains("Metal") || ex.Message.Contains("OpenGL") || ex.Message.Contains("Graphics"))
            {
                return "Graphics System Issue:\n" +
                       "• Metal not supported on device\n" +
                       "• OpenGL ES not available\n" +
                       "• Graphics driver incompatible";
            }
            else
            {
                return "FNA iOS Requirements:\n" +
                       "• iOS 12.0 or later\n" +
                       "• SDL2.framework in app bundle\n" +
                       "• OpenAL framework support\n" +
                       "• Metal or OpenGL ES support";
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("GLTRON: iOS - Application starting...");
                UIApplication.Main(args, null, typeof(Program));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: iOS - Application startup failed: {ex}");
                throw;
            }
        }

        public override void FinishedLaunching(UIApplication app)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("GLTRON: iOS - FinishedLaunching called");
                RunGame();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: iOS - FinishedLaunching failed: {ex}");
                throw;
            }
        }

        public override void OnActivated(UIApplication application)
        {
            System.Diagnostics.Debug.WriteLine("GLTRON: iOS - OnActivated");
            base.OnActivated(application);
        }

        public override void OnResignActivation(UIApplication application)
        {
            System.Diagnostics.Debug.WriteLine("GLTRON: iOS - OnResignActivation");
            base.OnResignActivation(application);
        }

        public override void DidEnterBackground(UIApplication application)
        {
            System.Diagnostics.Debug.WriteLine("GLTRON: iOS - DidEnterBackground");
            base.DidEnterBackground(application);
        }

        public override void WillEnterForeground(UIApplication application)
        {
            System.Diagnostics.Debug.WriteLine("GLTRON: iOS - WillEnterForeground");
            base.WillEnterForeground(application);
        }
    }
}
