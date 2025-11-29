using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GltronMobileEngine.Video;

public enum CameraType
{
    Circling,
    Follow,
    FollowFar,
    FollowClose,
    Bird,
    Cockpit,
    Mouse
}

public class Camera
{
    public Matrix View { get; private set; }
    public Matrix Projection { get; private set; }
    public int ViewportWidth { get; }
    public int ViewportHeight { get; }

    // GLTron camera constants (from original Java code)
    private const float CAM_CIRCLE_DIST = 17.0f;
    private const float CAM_FOLLOW_DIST = 18.0f;
    private const float CAM_FOLLOW_FAR_DIST = 30.0f;
    private const float CAM_FOLLOW_CLOSE_DIST = 0.0f;
    private const float CAM_FOLLOW_BIRD_DIST = 200.0f;
    private const float CAM_CIRCLE_Z = 8.0f;
    private const float CAM_COCKPIT_Z = 4.0f;
    private const float CAM_SPEED = 0.000698f;
    private const float B_HEIGHT = 0.0f;

    private CameraType _cameraType;
    private Vector3 _target;
    private Vector3 _camPos;
    private float[] _movement = new float[4]; // R, CHI, PHI, PHI_OFFSET
    private float _phi;

    // Camera defaults from original (R, CHI, PHI)
    private static readonly float[,] CamDefaults = {
        { CAM_CIRCLE_DIST, MathHelper.Pi / 3.0f, 0.0f }, // circle
        { CAM_FOLLOW_DIST, MathHelper.Pi / 4.0f, MathHelper.Pi / 72.0f }, // follow
        { CAM_FOLLOW_FAR_DIST, MathHelper.Pi / 4.0f, MathHelper.Pi / 72.0f }, // follow far
        { CAM_FOLLOW_CLOSE_DIST, MathHelper.Pi / 4.0f, MathHelper.Pi / 72.0f }, // follow close
        { CAM_FOLLOW_BIRD_DIST, MathHelper.Pi / 4.0f, MathHelper.Pi / 72.0f }, // birds-eye view
        { CAM_COCKPIT_Z, MathHelper.Pi / 8.0f, 0.0f }, // cockpit
        { CAM_CIRCLE_DIST, MathHelper.Pi / 3.0f, 0.0f } // free
    };

    public Camera(Viewport viewport)
    {
        ViewportWidth = viewport.Width;
        ViewportHeight = viewport.Height;
        
        // GLTron-style perspective (wider FOV, closer near plane)
        Projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.PiOver2, // 90 degrees like original GLTron
            viewport.AspectRatio,
            1.0f,  // Closer near plane
            500f); // Reasonable far plane
        
        // Initialize as circling camera (default in original)
        _cameraType = CameraType.Circling;
        InitializeCirclingCamera();
        
        _target = Vector3.Zero;
        _camPos = new Vector3(CAM_CIRCLE_DIST, 0, CAM_CIRCLE_Z);
        
        UpdateViewMatrix();
    }

    private void InitializeCirclingCamera()
    {
        _movement[0] = CamDefaults[0, 0]; // R
        _movement[1] = CamDefaults[0, 1]; // CHI
        _movement[2] = CamDefaults[0, 2]; // PHI
        _movement[3] = 0.0f; // PHI_OFFSET
        _phi = 0.0f;
    }

    public void Update(Vector3 playerPos, GameTime gameTime)
    {
        _target = new Vector3(playerPos.X, playerPos.Z, B_HEIGHT); // GLTron uses Y as up
        
        float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        
        // Update camera based on type
        switch (_cameraType)
        {
            case CameraType.Circling:
                UpdateCirclingCamera(dt);
                break;
            case CameraType.Follow:
            case CameraType.FollowFar:
            case CameraType.FollowClose:
            case CameraType.Bird:
                UpdateFollowCamera();
                break;
        }
        
        UpdateViewMatrix();
    }

    private void UpdateCirclingCamera(float dt)
    {
        // Circling camera rotates around the player
        _phi += CAM_SPEED * dt;
        
        float r = _movement[0];
        float chi = _movement[1];
        float phi = _phi + _movement[2] + _movement[3];
        
        // Position camera in spherical coordinates around target
        _camPos.X = _target.X + r * (float)System.Math.Cos(phi) * (float)System.Math.Sin(chi);
        _camPos.Y = _target.Y + r * (float)System.Math.Sin(phi) * (float)System.Math.Sin(chi);
        _camPos.Z = r * (float)System.Math.Cos(chi);
    }

    private void UpdateFollowCamera()
    {
        float r = _movement[0];
        float chi = _movement[1];
        float phi = _movement[2] + _movement[3];
        
        // Follow camera stays behind the player
        _camPos.X = _target.X + r * (float)System.Math.Cos(phi) * (float)System.Math.Sin(chi);
        _camPos.Y = _target.Y + r * (float)System.Math.Sin(phi) * (float)System.Math.Sin(chi);
        _camPos.Z = r * (float)System.Math.Cos(chi);
    }

    private void UpdateViewMatrix()
    {
        // GLTron uses Z as up axis, MonoGame uses Y as up
        Vector3 up = Vector3.UnitZ;
        View = Matrix.CreateLookAt(_camPos, _target, up);
    }

    public void SetCameraType(CameraType type)
    {
        _cameraType = type;
        int index = (int)type;
        if (index < CamDefaults.GetLength(0))
        {
            _movement[0] = CamDefaults[index, 0];
            _movement[1] = CamDefaults[index, 1];
            _movement[2] = CamDefaults[index, 2];
        }
    }
}
