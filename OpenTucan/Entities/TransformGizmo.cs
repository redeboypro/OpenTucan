using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using OpenTucan.Bridges;
using OpenTucan.Graphics;
using OpenTucan.Physics;

namespace OpenTucan.Entities
{
    public delegate void GizmoInteractionEvent(int axis, Vector3 delta);
    public class TransformGizmo : Entity
    {
        public const int XAxis = 0;
        public const int YAxis = 1;
        public const int ZAxis = 2;
        
        private readonly VAO _vertexArrayObject;
        private readonly GizmoShader _shader;
        
        private readonly Camera _camera;
        private readonly NativeWindow _window;
        private Vector3 _currentPlaneNormal;
        private Vector3 _lastPoint;

        private int _axis;
        
        private bool _isMousePressed;
        private bool _isAxisAssigned;
        
        private int _cursorX;
        private int _cursorY;

        public TransformGizmo(Camera camera, NativeWindow nativeWindow)
        {
            _shader = new GizmoShader();
            _camera = camera;
            _window = nativeWindow;
            
            XAxisColor = Color4.Red;
            YAxisColor = Color4.Green;
            ZAxisColor = Color4.Blue;

            _vertexArrayObject = new VAO();
            _vertexArrayObject.CreateVertexBufferObject(0, 3, new []
            {
                new Vector3(0, 0, 0), 
                new Vector3(1, 0, 0),
                
                new Vector3(0, 0, 0),
                new Vector3(0, 1, 0),
                
                new Vector3(0, 0, 0),
                new Vector3(0, 0, 1)
            });
            
            _vertexArrayObject.CreateVertexBufferObject(1, 4, new []
            {
                XAxisColor, 
                XAxisColor,
                
                YAxisColor,
                YAxisColor,
                
                ZAxisColor,
                ZAxisColor
            });

            nativeWindow.MouseDown += (sender, args) =>
            {
                OnMouseDown(args);
            };
            
            nativeWindow.MouseUp += (sender, args) =>
            {
                OnMouseUp(args);
            };
            
            nativeWindow.MouseMove += (sender, args) =>
            {
                OnMouseDrag(args);
            };
        }

        public Color4 XAxisColor { get; set; }
        
        public Color4 YAxisColor { get; set; }
        
        public Color4 ZAxisColor { get; set; }

        public GizmoInteractionEvent Interaction;

        public void OnRenderFrame()
        {
            GL.LineWidth(12);
            _shader.Start();
            _shader.SetProjectionMatrix(_camera.ProjectionMatrix);
            _shader.SetViewMatrix(_camera.ViewMatrix);
            _shader.SetModelMatrix(GetGlobalMatrix());

            GL.BindVertexArray(_vertexArrayObject.Id);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            GL.DrawArrays(PrimitiveType.Lines, 0, 6);

            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.BindVertexArray(0);
            _shader.Stop();

            if (_isAxisAssigned || !_isMousePressed)
            {
                return;
            }
            
            var pixel = Color4.White;

            GL.ReadPixels(_cursorX, _window.Height - _cursorY, 1, 1, PixelFormat.Rgba, PixelType.Float, ref pixel);

            if (pixel == XAxisColor)
            {
                _axis = XAxis;
                UpdateAxisData();
                _isAxisAssigned = true;
            }
                
            if (pixel == YAxisColor)
            {
                _axis = YAxis;
                UpdateAxisData();
                _isAxisAssigned = true;
            }

            if (pixel != ZAxisColor)
            {
                return;
            }

            _axis = ZAxis;
            UpdateAxisData();
            _isAxisAssigned = true;
        }

        private void UpdateAxisData()
        {
            _currentPlaneNormal = RaycastAxis(_axis, _cursorX, _cursorY, out var _hitPoint);
            _lastPoint = _hitPoint;
        }

        private void UpdateLastPoint()
        {
            Raycast(_cursorX, _cursorY, _currentPlaneNormal, out _lastPoint);
        }

        private void OnMouseDown(MouseButtonEventArgs mouseButtonEventArgs)
        {
            _isMousePressed = mouseButtonEventArgs.Button == MouseButton.Left;
            _cursorX = mouseButtonEventArgs.X;
            _cursorY = mouseButtonEventArgs.Y;
        }

        private void OnMouseUp(MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (mouseButtonEventArgs.Button == MouseButton.Left)
            {
                _isMousePressed = false;
            }
            _isAxisAssigned = false;
        }

        private void OnMouseDrag(MouseEventArgs mouseMoveEventArgs)
        {
            _cursorX = mouseMoveEventArgs.X;
            _cursorY = mouseMoveEventArgs.Y;
            
            if (!_isMousePressed || !_isAxisAssigned)
            {
                return;
            }

            if (!Raycast(_cursorX, _cursorY, _currentPlaneNormal, out var _hitPoint))
            {
                return;
            }
            
            Interaction?.Invoke(_axis, _hitPoint - _lastPoint);
            UpdateLastPoint();
        }

        private Vector3 RaycastAxis(int axis, int mousePositionX, int mousePositionY, out Vector3 hitPoint)
        {
            switch (axis)
            {
                case XAxis:
                    return GetAxis(Front(Space.Global), Up(Space.Global), mousePositionX, mousePositionY, out hitPoint);
                case YAxis:
                    return GetAxis(Right(Space.Global), Front(Space.Global), mousePositionX, mousePositionY, out hitPoint);
                case ZAxis:
                    return GetAxis(Right(Space.Global), Up(Space.Global), mousePositionX, mousePositionY, out hitPoint);
            }
            
            hitPoint = Vector3.Zero;
            return Vector3.Zero;
        }

        private Vector3 GetAxis(Vector3 plane1Normal, Vector3 plane2Normal, int mousePositionX, int mousePositionY, out Vector3 hitPoint)
        {
            var firstIntersect = Raycast(mousePositionX, mousePositionY, plane1Normal, out var firstHitPoint);
            if (!firstIntersect)
            {
                Raycast(mousePositionX, mousePositionY, -plane1Normal, out firstHitPoint);
            }
            
            var secondIntersect = Raycast(mousePositionX, mousePositionY, plane2Normal, out var secondHitPoint);
                    
            var firstDistance = Vector3.Distance(firstHitPoint, _camera.WorldSpaceLocation);
            var secondDistance = Vector3.Distance(secondHitPoint, _camera.WorldSpaceLocation);

            if (firstDistance > secondDistance)
            {
                hitPoint = firstHitPoint;
                return firstIntersect ? plane1Normal : -plane1Normal;
            }
                    
            hitPoint = secondHitPoint;
            return secondIntersect ? plane2Normal: -plane2Normal;
        }
        
        private bool Raycast(int mousePositionX, int mousePositionY, Vector3 planeNormal, out Vector3 hitPoint)
        {
            var rayStart = _camera.WorldSpaceLocation;
            var rayDirection = _camera.RectToWorld(mousePositionX, mousePositionY);
            
            var planeNormalDotRayDirection = Vector3.Dot(planeNormal, rayDirection);
            hitPoint = rayStart + rayDirection;
            
            if (Math.Abs(planeNormalDotRayDirection) <= float.Epsilon) 
            {
                return false;
            }
            
            var distanceToIntersectionPoint = Vector3.Dot(WorldSpaceLocation - rayStart, planeNormal) / planeNormalDotRayDirection;
            hitPoint = rayStart + rayDirection * distanceToIntersectionPoint;
            
            return distanceToIntersectionPoint >= 0;
        }

        protected override void OnTransformMatrices()
        {
            
        }
        
        private class GizmoShader : Shader
        {
            private const string VertexShaderCode = @"
#version 400 core

in vec3 vertex;
in vec3 color;

out vec3 pass_color;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

void main(void) 
{
    vec4 wldPosition = modelMatrix * vec4(vertex, 1.0);
	gl_Position = projectionMatrix * viewMatrix * wldPosition;
    pass_color = color;
}
";
            
            private const string FragmentShaderCode = @"
#version 400 core

in vec3 pass_color;

out vec4 outColor;

void main(void) 
{
	outColor = vec4(pass_color, 1);
}
";
            
            public GizmoShader() : base(VertexShaderCode, FragmentShaderCode) { }

            protected override void BindAttributes()
            {
                BindAttribute(0, "vertex");
                BindAttribute(1, "color");
            }
        
            public void SetProjectionMatrix(Matrix4 projectionMatrix)
            {
                SetUniform("projectionMatrix", projectionMatrix);
            }
        
            public void SetViewMatrix(Matrix4 viewMatrix)
            {
                SetUniform("viewMatrix", viewMatrix);
            }
        
            public void SetModelMatrix(Matrix4 modelMatrix)
            {
                SetUniform("modelMatrix", modelMatrix);
            }
        }
    }
}