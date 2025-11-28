using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GltronMobileEngine.Video;

public class Camera
{
    public Matrix View { get; private set; }
    public Matrix Projection { get; private set; }
    public int ViewportWidth { get; }
    public int ViewportHeight { get; }

    private Vector3 _target;
    private float _orbitAngle;

    public Camera(Viewport viewport)
    {
        ViewportWidth = viewport.Width;
        ViewportHeight = viewport.Height;
        Projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.PiOver4,
            viewport.AspectRatio,
            0.1f,
            1000f);
        _target = Vector3.Zero;
        _orbitAngle = 0f;
        View = Matrix.CreateLookAt(new Vector3(0, 50, 50), Vector3.Zero, Vector3.Up);
    }

    public void Update(Vector3 target, GameTime gameTime)
    {
        _target = target;
        _orbitAngle += (float)gameTime.ElapsedGameTime.TotalSeconds * 0.3f;
        var camPos = target + new Vector3(
            (float)System.Math.Cos(_orbitAngle) * 80f,
            45f,
            (float)System.Math.Sin(_orbitAngle) * 80f);
        View = Matrix.CreateLookAt(camPos, _target, Vector3.Up);
    }
}
