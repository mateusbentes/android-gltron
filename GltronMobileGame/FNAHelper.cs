using System;

namespace GltronMobileGame
{
    /// <summary>
    /// FNA initialization helper for Android and iOS
    /// Handles FNA environment setup for mobile platforms
    /// </summary>
    public static class FNAHelper
    {
        /// <summary>
        /// Sets up FNA environment variables for mobile platforms
        /// </summary>
        public static void SetupFNAEnvironment()
        {
            try
            {
                LogInfo("Setting up FNA environment variables...");
                
                // Core FNA backend configuration
                System.Environment.SetEnvironmentVariable("FNA_PLATFORM_BACKEND", "SDL2");
                System.Environment.SetEnvironmentVariable("FNA_AUDIO_BACKEND", "OpenAL");
                System.Environment.SetEnvironmentVariable("FNA_GRAPHICS_BACKEND", "OpenGL");
                
                // FNA logging
                System.Environment.SetEnvironmentVariable("FNA_ENABLE_LOGGING", "1");
                
#if ANDROID
                LogInfo("Applying Android-specific FNA settings...");
                
                // Android-specific OpenGL settings with ES2 fallback
                // Try ES3 first, but allow fallback to ES2 for compatibility
                System.Environment.SetEnvironmentVariable("FNA_OPENGL_FORCE_ES3", "0"); // Allow ES2 fallback
                System.Environment.SetEnvironmentVariable("FNA_OPENGL_FORCE_COMPATIBILITY_PROFILE", "0");
                System.Environment.SetEnvironmentVariable("FNA_OPENGL_ES_FALLBACK", "1"); // Enable ES2 fallback
                
                // Android touch/mouse settings
                System.Environment.SetEnvironmentVariable("SDL_ANDROID_SEPARATE_MOUSE_AND_TOUCH", "1");
                System.Environment.SetEnvironmentVariable("SDL_TOUCH_MOUSE_EVENTS", "0");
                
                // Android-specific SDL settings
                System.Environment.SetEnvironmentVariable("SDL_ANDROID_BLOCK_ON_PAUSE", "0");
                System.Environment.SetEnvironmentVariable("SDL_ANDROID_TRAP_BACK_BUTTON", "1");
                
                // OpenGL ES settings for better compatibility
                System.Environment.SetEnvironmentVariable("SDL_OPENGL_ES_DRIVER", "1");
                
                // Audio settings for Android
                System.Environment.SetEnvironmentVariable("SDL_AUDIODRIVER", "android");
                
                LogInfo("Android-specific FNA settings applied");
                
                // Log current Android context info
                try
                {
                    var context = Android.App.Application.Context;
                    if (context != null)
                    {
                        LogInfo($"Android API Level: {Android.OS.Build.VERSION.SdkInt}");
                        LogInfo($"Android Version: {Android.OS.Build.VERSION.Release}");
                        LogInfo($"Device Model: {Android.OS.Build.Model}");
                    }
                }
                catch (System.Exception ex)
                {
                    LogError($"Failed to get Android context info: {ex.Message}");
                }
                
#elif IOS
                LogInfo("Applying iOS-specific FNA settings...");
                
                // iOS-specific settings
                System.Environment.SetEnvironmentVariable("FNA_OPENGL_FORCE_ES3", "1");
                System.Environment.SetEnvironmentVariable("FNA_OPENGL_FORCE_COMPATIBILITY_PROFILE", "0");
                
                LogInfo("iOS-specific FNA settings applied");
#endif
                
                LogInfo("FNA environment variables set successfully");
                
                // Log all FNA environment variables for debugging
                LogEnvironmentVariables();
            }
            catch (System.Exception ex)
            {
                LogError($"FNA environment setup failed: {ex}");
                throw; // Re-throw as this is critical for FNA initialization
            }
        }

        /// <summary>
        /// Logs current FNA environment variables for debugging
        /// </summary>
        private static void LogEnvironmentVariables()
        {
            try
            {
                LogInfo("=== FNA Environment Variables ===");
                LogInfo($"FNA_PLATFORM_BACKEND: {System.Environment.GetEnvironmentVariable("FNA_PLATFORM_BACKEND")}");
                LogInfo($"FNA_AUDIO_BACKEND: {System.Environment.GetEnvironmentVariable("FNA_AUDIO_BACKEND")}");
                LogInfo($"FNA_GRAPHICS_BACKEND: {System.Environment.GetEnvironmentVariable("FNA_GRAPHICS_BACKEND")}");
                LogInfo($"FNA_OPENGL_FORCE_ES3: {System.Environment.GetEnvironmentVariable("FNA_OPENGL_FORCE_ES3")}");
                LogInfo($"FNA_OPENGL_ES_FALLBACK: {System.Environment.GetEnvironmentVariable("FNA_OPENGL_ES_FALLBACK")}");
                LogInfo($"SDL_ANDROID_SEPARATE_MOUSE_AND_TOUCH: {System.Environment.GetEnvironmentVariable("SDL_ANDROID_SEPARATE_MOUSE_AND_TOUCH")}");
                LogInfo($"SDL_AUDIODRIVER: {System.Environment.GetEnvironmentVariable("SDL_AUDIODRIVER")}");
                LogInfo("=== End Environment Variables ===");
            }
            catch (System.Exception ex)
            {
                LogError($"Failed to log environment variables: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if native libraries can be loaded
        /// </summary>
        public static void VerifyNativeLibraries()
        {
            try
            {
                LogInfo("=== Verifying Native Libraries ===");
                
                // Check SDL2
                try
                {
                    if (System.Runtime.InteropServices.NativeLibrary.TryLoad("SDL2", out var sdl2Handle))
                    {
                        LogInfo("✅ SDL2 library loaded successfully");
                        System.Runtime.InteropServices.NativeLibrary.Free(sdl2Handle);
                    }
                    else
                    {
                        LogError("❌ SDL2 library failed to load");
                    }
                }
                catch (System.Exception ex)
                {
                    LogError($"❌ SDL2 library check failed: {ex.Message}");
                }
                
                // Check OpenAL
                try
                {
                    if (System.Runtime.InteropServices.NativeLibrary.TryLoad("openal", out var openalHandle))
                    {
                        LogInfo("✅ OpenAL library loaded successfully");
                        System.Runtime.InteropServices.NativeLibrary.Free(openalHandle);
                    }
                    else
                    {
                        LogError("❌ OpenAL library failed to load");
                    }
                }
                catch (System.Exception ex)
                {
                    LogError($"❌ OpenAL library check failed: {ex.Message}");
                }
                
                LogInfo("=== End Native Library Verification ===");
            }
            catch (System.Exception ex)
            {
                LogError($"Native library verification failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Mobile platform logging helper
        /// </summary>
        public static void LogInfo(string message)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON-FNA: {message}");
            
#if ANDROID
            try { Android.Util.Log.Info("GLTRON-FNA", message); } catch { }
#elif IOS
            try { Foundation.NSLog($"GLTRON-FNA: {message}"); } catch { }
#endif
        }

        /// <summary>
        /// Mobile platform error logging helper
        /// </summary>
        public static void LogError(string message)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON-FNA: ERROR - {message}");
            
#if ANDROID
            try { Android.Util.Log.Error("GLTRON-FNA", message); } catch { }
#elif IOS
            try { Foundation.NSLog($"GLTRON-FNA: ERROR - {message}"); } catch { }
#endif
        }

        /// <summary>
        /// Gets current mobile platform
        /// </summary>
        public static string GetPlatform()
        {
#if ANDROID
            return "Android";
#elif IOS
            return "iOS";
#else
            return "Unknown";
#endif
        }
    }
}
