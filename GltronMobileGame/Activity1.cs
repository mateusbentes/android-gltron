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
        ScreenOrientation = ScreenOrientation.SensorLandscape,
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
                FNAHelper.LogInfo("=== FNA ANDROID ACTIVITY STARTING ===");
                
                // Check device compatibility first (non-blocking)
                try
                {
                    var compatibilityResult = FNACompatibilityChecker.CheckCompatibility(this);
                    if (!compatibilityResult.IsCompatible)
                    {
                        FNAHelper.LogError($"Device compatibility warning: OpenGL ES 3.0: {compatibilityResult.OpenGLESSupported}, Architecture: {compatibilityResult.ArchitectureSupported}, Android Version: {compatibilityResult.AndroidVersionSupported}");
                        // Continue anyway - let FNA decide if it can work
                    }
                    else
                    {
                        FNAHelper.LogInfo("Device compatibility check passed");
                    }
                }
                catch (System.Exception ex)
                {
                    FNAHelper.LogError($"Compatibility check failed, continuing anyway: {ex.Message}");
                }
                
                // CRITICAL: Set up FNA environment BEFORE calling base.OnCreate
                FNAHelper.SetupFNAEnvironment();
                
                // Call base.OnCreate
                base.OnCreate(bundle);
                FNAHelper.LogInfo("Base OnCreate completed");
                
                // CRITICAL: Initialize FNA platform for Android with additional safety
                FNAHelper.LogInfo("Initializing FNA platform with Android-specific settings...");
                
                try
                {
                    // Set FNA environment variables BEFORE platform initialization
                    System.Environment.SetEnvironmentVariable("FNA_GRAPHICS_ENABLE_HIGHDPI", "0");
                    System.Environment.SetEnvironmentVariable("FNA_AUDIO_FORCE_AUDIOQUEUE", "1");
                    System.Environment.SetEnvironmentVariable("FNA_GRAPHICS_FORCE_COMPATIBILITY_PROFILE", "1");
                    System.Environment.SetEnvironmentVariable("SDL_ANDROID_SEPARATE_MOUSE_AND_TOUCH", "1");
                    FNAHelper.LogInfo("FNA environment variables set");
                    
                    InitializeFNAPlatform();
                    FNAHelper.LogInfo("FNA platform initialization completed");
                }
                catch (System.Exception ex)
                {
                    FNAHelper.LogError($"FNA platform initialization failed: {ex}");
                    throw;
                }
                
                // Verify native libraries are working
                FNAHelper.VerifyNativeLibraries();
                
                // Test basic class instantiation before creating Game1
                FNAHelper.LogInfo("Testing basic class instantiation...");
                try
                {
                    // Test Vec creation
                    var testVec = new GltronMobileEngine.Vec();
                    FNAHelper.LogInfo("Vec creation: OK");
                    
                    // Test Segment creation
                    var testSegment = new GltronMobileEngine.Segment();
                    FNAHelper.LogInfo("Segment creation: OK");
                    
                    // Test GLTronGame creation
                    var testGLTronGame = new GLTronGame();
                    FNAHelper.LogInfo("GLTronGame creation: OK");
                    
                    FNAHelper.LogInfo("All basic classes instantiated successfully");
                }
                catch (System.Exception ex)
                {
                    FNAHelper.LogError($"Basic class instantiation failed: {ex}");
                    FNAHelper.LogError($"This indicates a fundamental issue with the engine classes");
                    throw new System.InvalidOperationException($"Basic class instantiation failed: {ex.Message}", ex);
                }
                
                // Create the game instance
                FNAHelper.LogInfo("Creating Game1 instance...");
                
                try
                {
                    FNAHelper.LogInfo("Preparing for Game1 creation...");
                    
                    // Additional FNA preparation for Android
                    try
                    {
                        // Force SDL to initialize video subsystem first
                        FNAHelper.LogInfo("Pre-initializing SDL video subsystem...");
                        
                        // Set additional SDL hints for Android
                        System.Environment.SetEnvironmentVariable("SDL_HINT_ANDROID_SEPARATE_MOUSE_AND_TOUCH", "1");
                        System.Environment.SetEnvironmentVariable("SDL_HINT_ACCELEROMETER_AS_JOYSTICK", "0");
                        
                        FNAHelper.LogInfo("SDL hints set for Android");
                    }
                    catch (System.Exception ex)
                    {
                        FNAHelper.LogError($"SDL preparation failed: {ex}");
                    }
                    
                    FNAHelper.LogInfo("Creating Game1 instance...");
                    
                    // Try to create Game1 with additional safety measures and timeout detection
                    try
                    {
                        // Force garbage collection before creating Game1
                        System.GC.Collect();
                        System.GC.WaitForPendingFinalizers();
                        
                        FNAHelper.LogInfo("Memory cleaned, creating Game1...");
                        
                        // Create a timer to detect hanging
                        var timer = new System.Timers.Timer(10000); // 10 second timeout
                        timer.Elapsed += (sender, e) => {
                            FNAHelper.LogError("Game1 constructor has been running for 10+ seconds - likely hanging in FNA base constructor");
                            FNAHelper.LogError("This usually indicates SDL2/OpenGL initialization issues on Android");
                        };
                        timer.Start();
                        
                        try
                        {
                            _game = new Game1();
                            timer.Stop();
                            FNAHelper.LogInfo("Game1 instance created successfully");
                        }
                        finally
                        {
                            timer.Stop();
                            timer.Dispose();
                        }
                    }
                    catch (System.TypeInitializationException ex)
                    {
                        FNAHelper.LogError($"Type initialization failed: {ex}");
                        FNAHelper.LogError($"Inner exception: {ex.InnerException}");
                        throw new System.InvalidOperationException("FNA type initialization failed - likely SDL2/OpenGL issue", ex);
                    }
                    catch (System.DllNotFoundException ex)
                    {
                        FNAHelper.LogError($"Native library not found: {ex}");
                        throw new System.InvalidOperationException("Required native libraries missing", ex);
                    }
                }
                catch (System.Exception ex)
                {
                    FNAHelper.LogError($"CRITICAL: Game1 constructor failed: {ex}");
                    FNAHelper.LogError($"Game1 constructor stack trace: {ex.StackTrace}");
                    
                    // Try to identify the specific failure point
                    string failureContext = "Unknown";
                    if (ex.Message.Contains("GraphicsDeviceManager"))
                        failureContext = "GraphicsDeviceManager Creation";
                    else if (ex.Message.Contains("GLTronGame"))
                        failureContext = "GLTronGame Creation";
                    else if (ex.Message.Contains("Content"))
                        failureContext = "Content Root Directory";
                    else if (ex.Message.Contains("Segment"))
                        failureContext = "Segment/Walls Initialization";
                    else if (ex.Message.Contains("Vec"))
                        failureContext = "Vec/Vector Initialization";
                    
                    FNAHelper.LogError($"Game1 failure context: {failureContext}");
                    
                    // Try to show an error message to the user instead of crashing
                    try
                    {
                        var errorMessage = $"Game initialization failed in {failureContext}. Please check logs for details.";
                        Android.Widget.Toast.MakeText(this, errorMessage, Android.Widget.ToastLength.Long).Show();
                    }
                    catch { /* Ignore toast errors */ }
                    
                    // Re-throw with more context
                    throw new System.InvalidOperationException($"Game1 constructor failed in {failureContext}: {ex.Message}", ex);
                }
                
                // Add crash detection
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
                Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += OnAndroidUnhandledException;
                
                // Start the game loop
                FNAHelper.LogInfo("Starting FNA game loop...");
                _game.Run();
                
                FNAHelper.LogInfo("FNA Activity initialized successfully!");
            }
            catch (System.Exception ex)
            {
                FNAHelper.LogError("=== FNA INITIALIZATION EXCEPTION ===");
                FNAHelper.LogError($"EXCEPTION TYPE: {ex.GetType().FullName}");
                FNAHelper.LogError($"EXCEPTION MESSAGE: {ex.Message}");
                if (ex.InnerException != null)
                {
                    FNAHelper.LogError($"INNER EXCEPTION: {ex.InnerException.GetType().FullName}");
                    FNAHelper.LogError($"INNER MESSAGE: {ex.InnerException.Message}");
                }
                FNAHelper.LogError($"EXCEPTION STACK: {ex.StackTrace}");
                
                ShowErrorScreen(ex);
            }
        }

        private void InitializeFNAPlatform()
        {
            try
            {
                FNAHelper.LogInfo("Initializing FNA platform for Android...");
                
                // Check if native libraries are available
                CheckNativeLibraries();
                
                // Try to initialize SDL2 explicitly before FNA
                try
                {
                    FNAHelper.LogInfo("Attempting to pre-initialize SDL2...");
                    
                    // Try to access SDL2 through FNA's platform layer
                    var fnaPlatformType = System.Type.GetType("Microsoft.Xna.Framework.FNAPlatform, FNA");
                    if (fnaPlatformType != null)
                    {
                        FNAHelper.LogInfo("FNAPlatform type found, attempting initialization...");
                        
                        // Try to call FNAPlatform initialization methods if they exist
                        var initMethod = fnaPlatformType.GetMethod("Init", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                        if (initMethod != null)
                        {
                            FNAHelper.LogInfo("Calling FNAPlatform.Init()...");
                            initMethod.Invoke(null, null);
                            FNAHelper.LogInfo("FNAPlatform.Init() completed");
                        }
                        else
                        {
                            FNAHelper.LogInfo("FNAPlatform.Init() method not found, trying alternative approach");
                        }
                    }
                    else
                    {
                        FNAHelper.LogInfo("FNAPlatform type not found, using basic initialization");
                    }
                }
                catch (System.Exception ex)
                {
                    FNAHelper.LogError($"SDL2 pre-initialization failed: {ex}");
                    FNAHelper.LogInfo("Continuing with standard FNA initialization...");
                }
                
                // Initialize FNA platform manually if needed
                // This ensures FNA knows about the Android context
                var fnaLoggerType = System.Type.GetType("Microsoft.Xna.Framework.FNALoggerEXT, FNA");
                if (fnaLoggerType != null)
                {
                    var logInfoMethod = fnaLoggerType.GetMethod("LogInfo", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    logInfoMethod?.Invoke(null, new object[] { "FNA Android platform initializing..." });
                }
                
                FNAHelper.LogInfo("FNA platform initialization completed");
            }
            catch (System.Exception ex)
            {
                FNAHelper.LogError($"FNA platform initialization failed: {ex}");
                throw new System.InvalidOperationException($"FNA platform initialization failed: {ex.Message}", ex);
            }
        }

        private void CheckNativeLibraries()
        {
            try
            {
                FNAHelper.LogInfo("Checking FNA native libraries...");
                
                // Check if library files exist in APK
                var context = Android.App.Application.Context;
                var packageManager = context.PackageManager;
                var packageInfo = packageManager.GetPackageInfo(context.PackageName, PackageInfoFlags.SharedLibraryFiles);
                
                FNAHelper.LogInfo($"App package: {context.PackageName}");
                FNAHelper.LogInfo($"App data dir: {context.ApplicationInfo.DataDir}");
                FNAHelper.LogInfo($"Native lib dir: {context.ApplicationInfo.NativeLibraryDir}");
                
                // List native libraries in the APK
                try
                {
                    var libDir = new Java.IO.File(context.ApplicationInfo.NativeLibraryDir);
                    if (libDir.Exists())
                    {
                        var files = libDir.ListFiles();
                        FNAHelper.LogInfo($"Native libraries in APK ({files?.Length ?? 0} files):");
                        if (files != null)
                        {
                            foreach (var file in files)
                            {
                                FNAHelper.LogInfo($"  - {file.Name} ({file.Length()} bytes)");
                            }
                        }
                    }
                    else
                    {
                        FNAHelper.LogError($"Native library directory does not exist: {context.ApplicationInfo.NativeLibraryDir}");
                    }
                }
                catch (System.Exception ex)
                {
                    FNAHelper.LogError($"Failed to list native libraries: {ex.Message}");
                }
                
                // Try to load libraries with different names
                string[] sdlNames = { "SDL2", "libSDL2", "libSDL2.so" };
                string[] openalNames = { "openal", "libopenal", "libopenal.so", "OpenAL32" };
                
                bool sdlFound = false;
                bool openalFound = false;
                
                // Check SDL2
                foreach (var name in sdlNames)
                {
                    try
                    {
                        if (System.Runtime.InteropServices.NativeLibrary.TryLoad(name, out var handle))
                        {
                            FNAHelper.LogInfo($"✅ SDL2 library loaded successfully as '{name}'");
                            System.Runtime.InteropServices.NativeLibrary.Free(handle);
                            sdlFound = true;
                            break;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        FNAHelper.LogInfo($"Failed to load SDL2 as '{name}': {ex.Message}");
                    }
                }
                
                if (!sdlFound)
                {
                    FNAHelper.LogError("❌ SDL2 library not found with any name variant");
                }
                
                // Check OpenAL
                foreach (var name in openalNames)
                {
                    try
                    {
                        if (System.Runtime.InteropServices.NativeLibrary.TryLoad(name, out var handle))
                        {
                            FNAHelper.LogInfo($"✅ OpenAL library loaded successfully as '{name}'");
                            System.Runtime.InteropServices.NativeLibrary.Free(handle);
                            openalFound = true;
                            break;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        FNAHelper.LogInfo($"Failed to load OpenAL as '{name}': {ex.Message}");
                    }
                }
                
                if (!openalFound)
                {
                    FNAHelper.LogError("❌ OpenAL library not found with any name variant");
                }
                
                // Summary
                if (sdlFound && openalFound)
                {
                    FNAHelper.LogInfo("✅ All required native libraries found and loadable");
                }
                else
                {
                    FNAHelper.LogError($"⚠️ Missing libraries - SDL2: {sdlFound}, OpenAL: {openalFound}");
                    FNAHelper.LogError("This may cause FNA initialization to fail");
                }
                
                FNAHelper.LogInfo("Native library check completed");
            }
            catch (System.Exception ex)
            {
                FNAHelper.LogError($"Native library check failed: {ex}");
            }
        }



        private void ShowErrorScreen(System.Exception ex)
        {
            try
            {
                var errorView = new Android.Widget.TextView(this);
                
                string errorDetails = GetDetailedErrorMessage(ex);
                
                errorView.Text = $"GLTron Mobile - FNA Initialization Error\n\n" +
                               $"Error: {ex.GetType().Name}\n" +
                               $"Message: {ex.Message}\n\n" +
                               errorDetails + "\n\n" +
                               $"Device Info:\n" +
                               $"• Android {Android.OS.Build.VERSION.Release} (API {Android.OS.Build.VERSION.SdkInt})\n" +
                               $"• Model: {Android.OS.Build.Model}\n\n" +
                               $"Please restart the application.\n" +
                               $"If the problem persists, your device may not support FNA.";
                               
                errorView.SetTextColor(Android.Graphics.Color.White);
                errorView.SetBackgroundColor(Android.Graphics.Color.DarkRed);
                errorView.Gravity = Android.Views.GravityFlags.Center;
                errorView.SetPadding(20, 20, 20, 20);
                errorView.TextSize = 12f;
                SetContentView(errorView);
                
                FNAHelper.LogError("FNA error view displayed");
            }
            catch (System.Exception ex2)
            {
                FNAHelper.LogError($"Failed to show error view: {ex2}");
            }
        }

        private string GetDetailedErrorMessage(System.Exception ex)
        {
            if (ex is System.TypeInitializationException)
            {
                return "FNA Platform Initialization Failed:\n" +
                       "• SDL2 native library missing or incompatible\n" +
                       "• OpenAL audio library missing\n" +
                       "• OpenGL ES 3.0 not supported\n" +
                       "• Native library architecture mismatch";
            }
            else if (ex.Message.Contains("SDL"))
            {
                return "SDL2 Library Issue:\n" +
                       "• SDL2 native library not found\n" +
                       "• Incompatible SDL2 version\n" +
                       "• Missing ARM64/ARMv7 libraries";
            }
            else if (ex.Message.Contains("OpenAL") || ex.Message.Contains("Audio"))
            {
                return "Audio System Issue:\n" +
                       "• OpenAL library not found\n" +
                       "• Audio permissions missing\n" +
                       "• Audio hardware not supported";
            }
            else if (ex.Message.Contains("OpenGL") || ex.Message.Contains("Graphics"))
            {
                return "Graphics System Issue:\n" +
                       "• OpenGL ES 3.0 not supported\n" +
                       "• Graphics driver incompatible\n" +
                       "• Hardware acceleration disabled";
            }
            else
            {
                return "FNA Requirements:\n" +
                       "• OpenGL ES 3.0 support\n" +
                       "• SDL2 native libraries\n" +
                       "• OpenAL audio support\n" +
                       "• ARM64 or ARMv7 architecture";
            }
        }

        protected override void OnPause()
        {
            FNAHelper.LogInfo("Activity1.OnPause - FNA handles pause automatically");
            base.OnPause();
            // FNA handles game pausing automatically when the activity pauses
        }

        protected override void OnResume()
        {
            FNAHelper.LogInfo("Activity1.OnResume - FNA handles resume automatically");
            base.OnResume();
            // FNA handles game resuming automatically when the activity resumes
        }

        protected override void OnDestroy()
        {
            FNAHelper.LogInfo("Activity1.OnDestroy - Disposing FNA game");
            
            try
            {
                _game?.Dispose();
                _game = null;
                FNAHelper.LogInfo("FNA game disposed successfully");
            }
            catch (System.Exception ex)
            {
                FNAHelper.LogError($"Error disposing FNA game: {ex}");
            }
            
            base.OnDestroy();
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var exception = e.ExceptionObject as System.Exception;
                FNAHelper.LogError("=== UNHANDLED EXCEPTION DETECTED ===");
                FNAHelper.LogError($"Exception: {exception?.GetType().FullName}");
                FNAHelper.LogError($"Message: {exception?.Message}");
                FNAHelper.LogError($"Stack Trace: {exception?.StackTrace}");
                FNAHelper.LogError($"Is Terminating: {e.IsTerminating}");
                FNAHelper.LogError("=== END UNHANDLED EXCEPTION ===");
            }
            catch
            {
                // Last resort logging
                System.Diagnostics.Debug.WriteLine("GLTRON: Unhandled exception occurred but logging failed");
            }
        }

        private void OnAndroidUnhandledException(object sender, Android.Runtime.RaiseThrowableEventArgs e)
        {
            try
            {
                FNAHelper.LogError("=== ANDROID UNHANDLED EXCEPTION DETECTED ===");
                FNAHelper.LogError($"Exception: {e.Exception?.GetType().FullName}");
                FNAHelper.LogError($"Message: {e.Exception?.Message}");
                FNAHelper.LogError($"Stack Trace: {e.Exception?.StackTrace}");
                FNAHelper.LogError("=== END ANDROID UNHANDLED EXCEPTION ===");
                e.Handled = false; // Let the system handle it after logging
            }
            catch
            {
                // Last resort logging
                System.Diagnostics.Debug.WriteLine("GLTRON: Android unhandled exception occurred but logging failed");
            }
        }
    }
}
