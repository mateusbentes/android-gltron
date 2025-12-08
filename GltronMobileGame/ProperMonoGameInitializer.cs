using Android.App;
using Microsoft.Xna.Framework;
using System;
using System.Reflection;

namespace GltronMobileGame
{
    /// <summary>
    /// Proper MonoGame initializer that sets up the Android platform correctly
    /// This allows MonoGame to work with direct Activity management
    /// </summary>
    public static class ProperMonoGameInitializer
    {
        private static bool _platformInitialized = false;

        public static bool InitializeMonoGamePlatform(Activity activity)
        {
            if (_platformInitialized)
                return true;

            try
            {
                System.Diagnostics.Debug.WriteLine("ProperMonoGameInitializer: Starting platform initialization...");

                // Step 1: Set the current activity in MonoGame's platform
                if (!SetActivityInMonoGame(activity))
                {
                    System.Diagnostics.Debug.WriteLine("ProperMonoGameInitializer: Failed to set activity");
                    return false;
                }

                // Step 2: Initialize the Android game platform
                if (!InitializeAndroidGamePlatform(activity))
                {
                    System.Diagnostics.Debug.WriteLine("ProperMonoGameInitializer: Failed to initialize platform");
                    return false;
                }

                _platformInitialized = true;
                System.Diagnostics.Debug.WriteLine("ProperMonoGameInitializer: Platform initialization completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ProperMonoGameInitializer: Platform initialization failed: {ex}");
                return false;
            }
        }

        private static bool SetActivityInMonoGame(Activity activity)
        {
            try
            {
                // Try multiple approaches to set the activity in MonoGame

                // Approach 1: Set static Activity field/property in Game class
                var gameType = typeof(Game);
                
                // Try Activity property
                var activityProperty = gameType.GetProperty("Activity", BindingFlags.Static | BindingFlags.Public);
                if (activityProperty != null && activityProperty.CanWrite)
                {
                    activityProperty.SetValue(null, activity);
                    System.Diagnostics.Debug.WriteLine("ProperMonoGameInitializer: Set Game.Activity property");
                    return true;
                }

                // Try Activity field
                var activityField = gameType.GetField("Activity", BindingFlags.Static | BindingFlags.Public);
                if (activityField != null)
                {
                    activityField.SetValue(null, activity);
                    System.Diagnostics.Debug.WriteLine("ProperMonoGameInitializer: Set Game.Activity field");
                    return true;
                }

                // Approach 2: Look for platform-specific activity storage
                var monoGameAssembly = Assembly.GetAssembly(typeof(Game));
                if (monoGameAssembly != null)
                {
                    // Look for AndroidGamePlatform class
                    var platformType = monoGameAssembly.GetType("Microsoft.Xna.Framework.AndroidGamePlatform");
                    if (platformType == null)
                    {
                        platformType = monoGameAssembly.GetType("Microsoft.Xna.Framework.Android.AndroidGamePlatform");
                    }

                    if (platformType != null)
                    {
                        // Try to set activity on platform
                        var activityFieldInPlatform = platformType.GetField("Activity", BindingFlags.Static | BindingFlags.Public);
                        if (activityFieldInPlatform != null)
                        {
                            activityFieldInPlatform.SetValue(null, activity);
                            System.Diagnostics.Debug.WriteLine("ProperMonoGameInitializer: Set AndroidGamePlatform.Activity field");
                            return true;
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine("ProperMonoGameInitializer: No activity field found, continuing anyway");
                return true; // Continue even if we can't set it
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ProperMonoGameInitializer: SetActivityInMonoGame failed: {ex}");
                return false;
            }
        }

        private static bool InitializeAndroidGamePlatform(Activity activity)
        {
            try
            {
                var monoGameAssembly = Assembly.GetAssembly(typeof(Game));
                if (monoGameAssembly == null)
                {
                    System.Diagnostics.Debug.WriteLine("ProperMonoGameInitializer: Could not get MonoGame assembly");
                    return false;
                }

                // Look for AndroidGamePlatform
                var platformType = monoGameAssembly.GetType("Microsoft.Xna.Framework.AndroidGamePlatform");
                if (platformType == null)
                {
                    platformType = monoGameAssembly.GetType("Microsoft.Xna.Framework.Android.AndroidGamePlatform");
                }

                if (platformType != null)
                {
                    System.Diagnostics.Debug.WriteLine($"ProperMonoGameInitializer: Found platform type: {platformType.FullName}");

                    // Try to call Initialize method
                    var initMethod = platformType.GetMethod("Initialize", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(Activity) }, null);
                    if (initMethod != null)
                    {
                        initMethod.Invoke(null, new object[] { activity });
                        System.Diagnostics.Debug.WriteLine("ProperMonoGameInitializer: Called AndroidGamePlatform.Initialize(Activity)");
                        return true;
                    }

                    // Try parameterless Initialize
                    initMethod = platformType.GetMethod("Initialize", BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null);
                    if (initMethod != null)
                    {
                        initMethod.Invoke(null, null);
                        System.Diagnostics.Debug.WriteLine("ProperMonoGameInitializer: Called AndroidGamePlatform.Initialize()");
                        return true;
                    }

                    // Try to create platform instance
                    var constructor = platformType.GetConstructor(new[] { typeof(Activity) });
                    if (constructor != null)
                    {
                        var platformInstance = constructor.Invoke(new object[] { activity });
                        System.Diagnostics.Debug.WriteLine("ProperMonoGameInitializer: Created AndroidGamePlatform instance");
                        
                        // Try to call instance Initialize
                        var instanceInitMethod = platformType.GetMethod("Initialize", BindingFlags.Instance | BindingFlags.Public);
                        if (instanceInitMethod != null)
                        {
                            instanceInitMethod.Invoke(platformInstance, null);
                            System.Diagnostics.Debug.WriteLine("ProperMonoGameInitializer: Called platform instance Initialize()");
                        }
                        
                        return true;
                    }
                }

                System.Diagnostics.Debug.WriteLine("ProperMonoGameInitializer: AndroidGamePlatform not found or no suitable initialization method");
                return true; // Continue anyway
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ProperMonoGameInitializer: InitializeAndroidGamePlatform failed: {ex}");
                return false;
            }
        }

        public static Game1 CreateInitializedGame1(Activity activity)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ProperMonoGameInitializer: Creating initialized Game1...");

                // Initialize platform first
                if (!InitializeMonoGamePlatform(activity))
                {
                    throw new InvalidOperationException("Failed to initialize MonoGame platform");
                }

                // Create Game1 instance
                var game = new Game1();

                // Register activity in services
                game.Services.AddService(typeof(Activity), activity);

                System.Diagnostics.Debug.WriteLine("ProperMonoGameInitializer: Game1 created and initialized successfully");
                return game;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ProperMonoGameInitializer: CreateInitializedGame1 failed: {ex}");
                throw;
            }
        }

        public static void ResetPlatform()
        {
            _platformInitialized = false;
            System.Diagnostics.Debug.WriteLine("ProperMonoGameInitializer: Platform reset");
        }
    }
}
