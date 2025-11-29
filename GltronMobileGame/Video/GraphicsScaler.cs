using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GltronMobileGame.Video
{
    public class GraphicsScaler
    {
        public int VirtualWidth { get; }
        public int VirtualHeight { get; }

        public int BackBufferWidth { get; private set; }
        public int BackBufferHeight { get; private set; }

        public Rectangle ViewportRect { get; private set; }
        public Matrix SpriteTransform { get; private set; } = Matrix.Identity;

        public float ScaleX { get; private set; } = 1f;
        public float ScaleY { get; private set; } = 1f;

        public GraphicsScaler(int virtualWidth = 1280, int virtualHeight = 720)
        {
            VirtualWidth = virtualWidth;
            VirtualHeight = virtualHeight;
        }

        public void Recalculate(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null) return;

            BackBufferWidth = graphicsDevice.PresentationParameters.BackBufferWidth;
            BackBufferHeight = graphicsDevice.PresentationParameters.BackBufferHeight;

            // Compute scaling with letterboxing to preserve aspect ratio
            float targetAspect = VirtualWidth / (float)VirtualHeight;
            float backbufferAspect = BackBufferWidth / (float)BackBufferHeight;

            int vpWidth, vpHeight;
            if (backbufferAspect > targetAspect)
            {
                // Backbuffer is wider than target: pillarbox
                vpHeight = BackBufferHeight;
                vpWidth = (int)(vpHeight * targetAspect);
            }
            else
            {
                // Backbuffer is taller/narrower than target: letterbox
                vpWidth = BackBufferWidth;
                vpHeight = (int)(vpWidth / targetAspect);
            }

            int vpX = (BackBufferWidth - vpWidth) / 2;
            int vpY = (BackBufferHeight - vpHeight) / 2;
            ViewportRect = new Rectangle(vpX, vpY, vpWidth, vpHeight);

            ScaleX = vpWidth / (float)VirtualWidth;
            ScaleY = vpHeight / (float)VirtualHeight;

            // 2D transform that scales to the viewport and offsets by its origin
            SpriteTransform = Matrix.CreateTranslation(-0f, -0f, 0f)
                              * Matrix.CreateScale(ScaleX, ScaleY, 1f)
                              * Matrix.CreateTranslation(vpX, vpY, 0f);
        }

        public Vector2 ToScreen(Vector2 virtualPoint)
        {
            return new Vector2(virtualPoint.X * ScaleX + ViewportRect.X,
                               virtualPoint.Y * ScaleY + ViewportRect.Y);
        }

        public Vector2 ToVirtual(Vector2 screenPoint)
        {
            return new Vector2((screenPoint.X - ViewportRect.X) / ScaleX,
                               (screenPoint.Y - ViewportRect.Y) / ScaleY);
        }
    }
}
