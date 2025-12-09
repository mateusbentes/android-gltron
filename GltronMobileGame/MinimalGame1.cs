using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GltronMobileGame
{
    public class MinimalGame1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public MinimalGame1()
        {
            try
            {
                Android.Util.Log.Debug("GLTRON", "MinimalGame1 constructor start");
                
                _graphics = new GraphicsDeviceManager(this);
                Android.Util.Log.Debug("GLTRON", "GraphicsDeviceManager created");
                
                Content.RootDirectory = "Content";
                Android.Util.Log.Debug("GLTRON", "Content root directory set");
                
                Android.Util.Log.Debug("GLTRON", "MinimalGame1 constructor complete");
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"MinimalGame1 constructor failed: {ex.Message}");
                Android.Util.Log.Error("GLTRON", $"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        protected override void Initialize()
        {
            try
            {
                Android.Util.Log.Debug("GLTRON", "MinimalGame1 Initialize start");
                base.Initialize();
                Android.Util.Log.Debug("GLTRON", "MinimalGame1 Initialize complete");
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"MinimalGame1 Initialize failed: {ex.Message}");
                throw;
            }
        }

        protected override void LoadContent()
        {
            try
            {
                Android.Util.Log.Debug("GLTRON", "MinimalGame1 LoadContent start");
                
                _spriteBatch = new SpriteBatch(GraphicsDevice);
                Android.Util.Log.Debug("GLTRON", "SpriteBatch created");
                
                Android.Util.Log.Debug("GLTRON", "MinimalGame1 LoadContent complete");
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"MinimalGame1 LoadContent failed: {ex.Message}");
                throw;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            try
            {
                GraphicsDevice.Clear(Color.CornflowerBlue);
                
                _spriteBatch.Begin();
                // Just draw a simple colored screen to prove it works
                _spriteBatch.End();
                
                base.Draw(gameTime);
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"MinimalGame1 Draw failed: {ex.Message}");
            }
        }
    }
}
