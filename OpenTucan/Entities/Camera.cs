using OpenTK;
using OpenTucan.Common;

namespace OpenTucan.Entities
{
    public class Camera : Entity
    {
        private readonly int rectWidth;
        private readonly int rectHeight;
        
        private float farClip;
        private float nearClip;
        private float fov;
        
        private Matrix4 projection;
        private Matrix4 view;

        public float FOV
        {
            get
            {
                return fov;
            }
            set
            {
                fov = value;
                CalculateProjectionMatrix();
            }
        }
        
        public float FarClip
        {
            get
            {
                return farClip;
            }
            set
            {
                farClip = value;
                CalculateProjectionMatrix();
            }
        }
        
        public float NearClip
        {
            get
            {
                return nearClip;
            }
            set
            {
                nearClip = value;
                CalculateProjectionMatrix();
            }
        }
        
        public Matrix4 ViewMatrix
        {
            get
            {
                return view;
            }
        }
        
        public Matrix4 ProjectionMatrix
        {
            get
            {
                return projection;
            }
        }

        public Camera(int projRectWidth, int projRectHeight)
        {
            rectWidth = projRectWidth;
            rectHeight = projRectHeight;
            farClip = 1000.0f;
            nearClip = 0.01f;
            FOV = MathHelper.PiOver4;
            CalculateViewMatrix();
        }

        private Vector3 GetWorldCoordinates(Vector4 eyeCoords)
        {
            var invertedView = view.Inverted();
            var rayWorld = eyeCoords * invertedView;
            var mouseRay = new Vector3(rayWorld.X, rayWorld.Y, rayWorld.Z);
            mouseRay.Normalize();
            return mouseRay;
        }

        private Vector4 GetEyeCoordinates(Vector4 clipCoords)
        {
            var invertedProjection = projection.Inverted();
            var eyeCoords = clipCoords * invertedProjection;
            return new Vector4(eyeCoords.X, eyeCoords.Y, -1f, 0f);
        }

        public Vector3 RectToWorld(int x, int y)
        {
            var normalizedCoords = Ortho.ScreenToRect(x, y, rectWidth, rectHeight);
            var clipCoords = new Vector4(normalizedCoords.X, normalizedCoords.Y, -1.0f, 1.0f);
            var eyeCoords = GetEyeCoordinates(clipCoords);
            var worldRay = GetWorldCoordinates(eyeCoords);
            return worldRay;
        }

        public Vector2 WorldToRect(Vector3 vector)
        {
            var worldSpaceLocation = vector.Transform(view);
            var transformedLocation = Vector3.TransformPerspective(worldSpaceLocation, projection);
            return new Vector2
            {
                X = (0.5f + 0.5f * transformedLocation.X) * rectWidth,
                Y = (0.5f + 0.5f * -transformedLocation.Y) * rectHeight
            };
        }

        private void CalculateViewMatrix()
        {
            view = Matrix4.LookAt(WorldSpaceLocation, WorldSpaceLocation + Front(Space.Global), Up(Space.Global));
        }

        private void CalculateProjectionMatrix()
        {
            projection = Matrix4.CreatePerspectiveFieldOfView(fov, (float) rectWidth / rectHeight,
                NearClip, FarClip);
        }

        protected override void OnTransformMatrices()
        {
            CalculateViewMatrix();
        }
    }
}