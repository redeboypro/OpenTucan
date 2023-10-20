using OpenTK;
using OpenTucan.Common;

namespace OpenTucan.Entities
{
    public class Camera : Entity
    {
        private readonly int _rectWidth;
        private readonly int _rectHeight;
        
        private float _farClip;
        private float _nearClip;
        private float _fov;
        
        private Matrix4 _projection;
        private Matrix4 _view;

        public float FOV
        {
            get
            {
                return _fov;
            }
            set
            {
                _fov = value;
                CalculateProjectionMatrix();
            }
        }
        
        public float FarClip
        {
            get
            {
                return _farClip;
            }
            set
            {
                _farClip = value;
                CalculateProjectionMatrix();
            }
        }
        
        public float NearClip
        {
            get
            {
                return _nearClip;
            }
            set
            {
                _nearClip = value;
                CalculateProjectionMatrix();
            }
        }
        
        public Matrix4 ViewMatrix
        {
            get
            {
                return _view;
            }
        }
        
        public Matrix4 ProjectionMatrix
        {
            get
            {
                return _projection;
            }
        }

        public Camera(int projRectWidth, int projRectHeight)
        {
            if (Main is null)
            {
                SetMain();
            }
            
            _rectWidth = projRectWidth;
            _rectHeight = projRectHeight;
            _farClip = 1000.0f;
            _nearClip = 0.01f;
            FOV = MathHelper.PiOver4;
            CalculateViewMatrix();
        }

        private Vector3 GetWorldCoordinates(Vector4 eyeCoords)
        {
            var invertedView = _view.Inverted();
            var rayWorld = eyeCoords * invertedView;
            var mouseRay = new Vector3(rayWorld.X, rayWorld.Y, rayWorld.Z);
            mouseRay.Normalize();
            return mouseRay;
        }

        private Vector4 GetEyeCoordinates(Vector4 clipCoords)
        {
            var invertedProjection = _projection.Inverted();
            var eyeCoords = clipCoords * invertedProjection;
            return new Vector4(eyeCoords.X, eyeCoords.Y, -1f, 0f);
        }

        public Vector3 RectToWorld(int x, int y)
        {
            var normalizedCoords = Ortho.ScreenToRect(x, y, _rectWidth, _rectHeight);
            var clipCoords = new Vector4(normalizedCoords.X, normalizedCoords.Y, -1.0f, 1.0f);
            var eyeCoords = GetEyeCoordinates(clipCoords);
            var worldRay = GetWorldCoordinates(eyeCoords);
            return worldRay;
        }

        public Vector2 WorldToRect(Vector3 vector)
        {
            var worldSpaceLocation = vector.Transform(_view);
            var transformedLocation = Vector3.TransformPerspective(worldSpaceLocation, _projection);
            return new Vector2
            {
                X = (0.5f + 0.5f * transformedLocation.X) * _rectWidth,
                Y = (0.5f + 0.5f * -transformedLocation.Y) * _rectHeight
            };
        }

        public void SetMain()
        {
            Main = this;
        }

        private void CalculateViewMatrix()
        {
            _view = Matrix4.LookAt(WorldSpaceLocation, WorldSpaceLocation + Front(Space.Global), Up(Space.Global));
        }

        private void CalculateProjectionMatrix()
        {
            _projection = Matrix4.CreatePerspectiveFieldOfView(_fov, (float) _rectWidth / _rectHeight,
                NearClip, FarClip);
        }

        protected override void OnTransformMatrices()
        {
            CalculateViewMatrix();
        }
        
        public static Camera Main { get; private set; }
    }
}