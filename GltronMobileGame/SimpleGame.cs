using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using GltronMobileGame.Video;

namespace GltronMobileGame;

public class SimpleGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _whitePixel;
    private bool _showMenu = true;
    private GraphicsScaler _scaler = new GraphicsScaler(1280, 720);

    public SimpleGame()
    {
        try
        {
            Android.Util.Log.Info("GLTRON", "SimpleGame constructor start");
            
            Android.Util.Log.Info("GLTRON", "Creating GraphicsDeviceManager");
            _graphics = new GraphicsDeviceManager(this);
            Android.Util.Log.Info("GLTRON", "GraphicsDeviceManager created");
            
            Android.Util.Log.Info("GLTRON", "Setting Content.RootDirectory");
            Content.RootDirectory = "Content";
            Android.Util.Log.Info("GLTRON", "Content.RootDirectory set");
            
            // Set up graphics for mobile landscape
            Android.Util.Log.Info("GLTRON", "Setting graphics properties");
            _graphics.IsFullScreen = true;
            _graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            Android.Util.Log.Info("GLTRON", "Graphics properties set");
            
            Android.Util.Log.Info("GLTRON", "SimpleGame constructor complete");
        }
        catch (System.Exception ex)
        {
            try { 
                Android.Util.Log.Error("GLTRON", $"SimpleGame constructor failed - Exception: {ex.GetType().Name}");
                Android.Util.Log.Error("GLTRON", $"SimpleGame constructor failed - Message: {ex.Message}");
                Android.Util.Log.Error("GLTRON", $"SimpleGame constructor failed - StackTrace: {ex.StackTrace}");
            } catch { }
            throw;
        }
    }

    protected override void Initialize()
    {
        try
        {
            Android.Util.Log.Info("GLTRON", "SimpleGame Initialize start");
            
            // Apply graphics settings
            _graphics.ApplyChanges();

            // Recalculate scaler with current backbuffer size
            _scaler.Recalculate(GraphicsDevice);

            // Handle dynamic size changes
            this.Window.ClientSizeChanged += (_, __) =>
            {
                try
                {
                    Android.Util.Log.Info("GLTRON", "ClientSizeChanged: recalculating scaler");
                    _scaler.Recalculate(GraphicsDevice);
                }
                catch { }
            };
            
            // Log resolution info
            var viewport = GraphicsDevice.Viewport;
            Android.Util.Log.Info("GLTRON", $"Screen resolution: {viewport.Width}x{viewport.Height} (virtual: {_scaler.VirtualWidth}x{_scaler.VirtualHeight})");
            
            TouchPanel.EnabledGestures = GestureType.Tap;
            Android.Util.Log.Info("GLTRON", "SimpleGame Initialize complete");
        }
        catch (System.Exception ex)
        {
            try { Android.Util.Log.Error("GLTRON", $"Initialize error: {ex}"); } catch { }
        }

        base.Initialize();
    }

    protected override void LoadContent()
    {
        try
        {
            Android.Util.Log.Info("GLTRON", "LoadContent start");
            
            if (GraphicsDevice == null)
            {
                Android.Util.Log.Error("GLTRON", "GraphicsDevice is null in LoadContent");
                return;
            }
            
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Android.Util.Log.Info("GLTRON", "SpriteBatch created");
            
            // Create a simple white pixel texture
            _whitePixel = new Texture2D(GraphicsDevice, 1, 1);
            _whitePixel.SetData(new[] { Color.White });
            Android.Util.Log.Info("GLTRON", "White pixel texture created");
            
            Android.Util.Log.Info("GLTRON", "LoadContent complete");
        }
        catch (System.Exception ex)
        {
            try { Android.Util.Log.Error("GLTRON", $"LoadContent error: {ex}"); } catch { }
            throw; // Re-throw to see the actual error
        }
    }

    protected override void Update(GameTime gameTime)
    {
        try
        {
            // Handle touch input
            TouchCollection touchCollection = TouchPanel.GetState();
            foreach (TouchLocation touch in touchCollection)
            {
                if (touch.State == TouchLocationState.Pressed)
                {
                    try 
                    { 
                        Android.Util.Log.Info("GLTRON", $"Touch detected at: {touch.Position.X}, {touch.Position.Y}"); 
                        _showMenu = !_showMenu; // Toggle between menu and game
                    } 
                    catch { }
                }
            }

            base.Update(gameTime);
        }
        catch (System.Exception ex)
        {
            try { Android.Util.Log.Error("GLTRON", $"Update error: {ex}"); } catch { }
            throw;
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        try
        {
            if (GraphicsDevice == null)
            {
                Android.Util.Log.Error("GLTRON", "GraphicsDevice is null in Draw");
                return;
            }

            if (_spriteBatch == null)
            {
                Android.Util.Log.Error("GLTRON", "SpriteBatch is null in Draw");
                return;
            }

            if (_whitePixel == null)
            {
                Android.Util.Log.Error("GLTRON", "WhitePixel texture is null in Draw");
                return;
            }

            // Clear with dark background
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(transformMatrix: _scaler.SpriteTransform);
            
            // Use virtual coordinates (1280x720) below; scaler will transform to screen
            if (_showMenu)
            {
                // Draw menu - cyan rectangle in center of virtual space
                _spriteBatch.Draw(_whitePixel, new Rectangle(1280/2 - 200, 720/2 - 100, 400, 200), Color.Cyan);
                // Draw smaller rectangle as "text"
                _spriteBatch.Draw(_whitePixel, new Rectangle(1280/2 - 150, 720/2 - 50, 300, 20), Color.White);
                _spriteBatch.Draw(_whitePixel, new Rectangle(1280/2 - 100, 720/2, 200, 20), Color.Yellow);
            }
            else
            {
                // Draw game - green rectangle in corner
                _spriteBatch.Draw(_whitePixel, new Rectangle(10, 10, 200, 100), Color.Green);
                // Draw some "game elements" in the center of virtual space
                _spriteBatch.Draw(_whitePixel, new Rectangle(1280/2, 720/2, 50, 50), Color.Red);
            }
            
            _spriteBatch.End();

            base.Draw(gameTime);
        }
        catch (System.Exception ex)
        {
            try { Android.Util.Log.Error("GLTRON", $"Draw error: {ex}"); } catch { }
            throw;
        }
    }
}
