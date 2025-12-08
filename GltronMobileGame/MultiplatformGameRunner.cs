using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if ANDROID
using Android.App;
using Android.Views;
#elif IOS
using Foundation;
using UIKit;
#endif

namespace GltronMobileGame
{
    /// <summary>
    /// Multiplatform game runner that bypasses the problematic FNA Game constructor
    /// Works on Android, iOS, Windows, Linux, macOS
    /// </summary>
    public class MultiplatformGameRunner
    {
        private GraphicsDeviceManager _graphics;
        private GLTronGame _glTronGame;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private GltronMobileEngine.Video.HUD _hud;
        private GltronMobileEngine.Video.WorldGraphics _worldGraphics;
        private GltronMobileEngine.Video.TrailsRenderer _trailsRenderer;
        private GltronMobileEngine.Video.Camera _camera;
        private Texture2D _whitePixel;
        private Texture2D _menuBackground;
        private bool _isRunning = false;
        
        // Platform-specific context
#if ANDROID
        private Activity _activity;
#elif IOS
        private UIViewController _viewController;
#else
        private object _platformContext; // For desktop platforms
#endif
        
        // Swipe detection system
        private Vector2? _swipeStartPosition = null;
        private double _swipeStartTime = 0;
        private bool _swipeInProgress = false;
        private const float MIN_SWIPE_DISTANCE = 30f;
        private const double MAX_SWIPE_TIME = 500;

        // Platform-agnostic constructor
#if ANDROID
        public MultiplatformGameRunner(Activity activity)
        {
            _activity = activity;
            InitializeCommon();
        }
#elif IOS
        public MultiplatformGameRunner(UIViewController viewController)
        {
            _viewController = viewController;
            InitializeCommon();
        }
#else
        public MultiplatformGameRunner(object platformContext = null)
        {
            _platformContext = platformContext;
            InitializeCommon();
        }
#endif

        private void InitializeCommon()
        {
            try
            {
                LogInfo("MultiplatformGameRunner constructor starting...");
                
                // Create GLTronGame first (we know this works on all platforms)
                _glTronGame = new GLTronGame();
                
                LogInfo("MultiplatformGameRunner constructor completed");
            }
            catch (System.Exception ex)
            {
                LogError($"MultiplatformGameRunner constructor failed: {ex}");
                throw;
            }
        }

        public void Initialize()
        {
            try
            {
                LogInfo("MultiplatformGameRunner Initialize starting...");
                
                // Initialize GLTronGame
                _glTronGame?.initialiseGame();
                
                LogInfo("MultiplatformGameRunner Initialize completed");
            }
            catch (System.Exception ex)
            {
                LogError($"MultiplatformGameRunner Initialize failed: {ex}");
                throw;
            }
        }

        public void LoadContent()
        {
            try
            {
                LogInfo("MultiplatformGameRunner LoadContent starting...");
                
                // Content loading would go here when we have a proper graphics context
                
                LogInfo("MultiplatformGameRunner LoadContent completed");
            }
            catch (System.Exception ex)
            {
                LogError($"MultiplatformGameRunner LoadContent failed: {ex}");
                throw;
            }
        }

        public void Update(GameTime gameTime)
        {
            try
            {
                // Run game logic
                _glTronGame?.RunGame(gameTime);
            }
            catch (System.Exception ex)
            {
                LogError($"MultiplatformGameRunner Update failed: {ex}");
            }
        }

        public void Draw(GameTime gameTime)
        {
            try
            {
                // Drawing would go here when we have a proper graphics context
                // For now, just log that we're running
                if (!_isRunning)
                {
                    LogInfo("MultiplatformGameRunner Draw - game is running!");
                    _isRunning = true;
                }
            }
            catch (System.Exception ex)
            {
                LogError($"MultiplatformGameRunner Draw failed: {ex}");
            }
        }

        public void Run()
        {
            try
            {
                LogInfo("MultiplatformGameRunner Run starting...");
                
                Initialize();
                LoadContent();
                
                // Simple game loop for testing
                var gameTime = new GameTime();
                int frameCount = 0;
                
                while (frameCount < 100) // Run for 100 frames as a test
                {
                    Update(gameTime);
                    Draw(gameTime);
                    frameCount++;
                    
                    if (frameCount % 30 == 0)
                    {
                        LogInfo($"MultiplatformGameRunner running - frame {frameCount}");
                    }
                    
                    System.Threading.Thread.Sleep(16); // ~60 FPS
                }
                
                LogInfo("MultiplatformGameRunner completed test run successfully!");
            }
            catch (System.Exception ex)
            {
                LogError($"MultiplatformGameRunner Run failed: {ex}");
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                LogInfo("MultiplatformGameRunner disposing...");
                
                // Don't dispose GraphicsDeviceManager directly - it's managed by FNA
                // _graphics?.Dispose(); // This is protected
                _spriteBatch?.Dispose();
                _whitePixel?.Dispose();
                _menuBackground?.Dispose();
                
                // Set references to null for garbage collection
                _graphics = null;
                _spriteBatch = null;
                _whitePixel = null;
                _menuBackground = null;
                _glTronGame = null;
                
                LogInfo("MultiplatformGameRunner disposed");
            }
            catch (System.Exception ex)
            {
                LogError($"MultiplatformGameRunner dispose failed: {ex}");
            }
        }

        // Platform-agnostic logging
        private void LogInfo(string message)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: {message}");
            
#if ANDROID
            try { Android.Util.Log.Info("GLTRON", message); } catch { }
#elif IOS
            try { Foundation.NSLog($"GLTRON: {message}"); } catch { }
#else
            Console.WriteLine($"GLTRON: {message}");
#endif
        }

        private void LogError(string message)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: ERROR - {message}");
            
#if ANDROID
            try { Android.Util.Log.Error("GLTRON", message); } catch { }
#elif IOS
            try { Foundation.NSLog($"GLTRON: ERROR - {message}"); } catch { }
#else
            Console.WriteLine($"GLTRON: ERROR - {message}");
#endif
        }

        // Platform-specific input handling
        public void HandleTouchInput(float x, float y, int screenWidth, int screenHeight)
        {
            try
            {
                _glTronGame?.addTouchEvent(x, y, screenWidth, screenHeight);
            }
            catch (System.Exception ex)
            {
                LogError($"HandleTouchInput failed: {ex}");
            }
        }

        // Platform-specific lifecycle methods
        public void OnPause()
        {
            try
            {
                LogInfo("MultiplatformGameRunner paused");
                // Pause game logic here
            }
            catch (System.Exception ex)
            {
                LogError($"OnPause failed: {ex}");
            }
        }

        public void OnResume()
        {
            try
            {
                LogInfo("MultiplatformGameRunner resumed");
                // Resume game logic here
            }
            catch (System.Exception ex)
            {
                LogError($"OnResume failed: {ex}");
            }
        }

        // Platform detection
        public static string GetCurrentPlatform()
        {
#if ANDROID
            return "Android";
#elif IOS
            return "iOS";
#elif WINDOWS
            return "Windows";
#elif LINUX
            return "Linux";
#elif MACOS
            return "macOS";
#else
            return "Unknown";
#endif
        }
    }
}
