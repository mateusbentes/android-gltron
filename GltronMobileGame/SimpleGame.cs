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
            
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            
            // Set up graphics for mobile landscape
            _graphics.IsFullScreen = true;
            _graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            
            Android.Util.Log.Info("GLTRON", "SimpleGame constructor complete");
        }
        catch (System.Exception ex)
        {
            try { Android.Util.Log.Error("GLTRON", $"SimpleGame constructor failed: {ex}"); } catch { }
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
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Create a simple white pixel texture
            _whitePixel = new Texture2D(GraphicsDevice, 1, 1);
            _whitePixel.SetData(new[] { Color.White });
            
            Android.Util.Log.Info("GLTRON", "LoadContent complete");
        }
        catch (System.Exception ex)
        {
            try { Android.Util.Log.Error("GLTRON", $"LoadContent error: {ex}"); } catch { }
        }
    }

    protected override void Update(GameTime gameTime)
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

    protected override void Draw(GameTime gameTime)
    {
        try
        {
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
        }
        catch (System.Exception ex)
        {
            try { Android.Util.Log.Error("GLTRON", $"Draw error: {ex}"); } catch { }
        }

        base.Draw(gameTime);
    }
}
