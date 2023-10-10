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
    public delegate void GizmoInteractionEvent(int axis, float interpolation);
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
        }

        public Color4 XAxisColor { get; set; }
        
        public Color4 YAxisColor { get; set; }
        
        public Color4 ZAxisColor { get; set; }

        public GizmoInteractionEvent Interaction;

        public void OnRenderFrame()
        {
            GL.LineWidth(6);
            _shader.Start();
            _shader.SetProjectionMatrix(_camera.ProjectionMatrix);
            _shader.SetViewMatrix(_camera.ViewMatrix);
            _shader.SetModelMatrix(GetModelMatrix());

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
                UpdateAxisData();
                _axis = XAxis;
                _isAxisAssigned = true;
            }
                
            if (pixel == YAxisColor)
            {
                UpdateAxisData();
                _axis = YAxis;
                _isAxisAssigned = true;
            }

            if (pixel != ZAxisColor)
            {
                return;
            }

            UpdateAxisData();
            _axis = ZAxis;
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

        public void OnMouseDown(MouseButtonEventArgs mouseButtonEventArgs)
        {
            _isMousePressed = mouseButtonEventArgs.Button == MouseButton.Left;
            _cursorX = mouseButtonEventArgs.X;
            _cursorY = mouseButtonEventArgs.Y;
        }

        public void OnMouseUp(MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (mouseButtonEventArgs.Button == MouseButton.Left)
            {
                _isMousePressed = false;
            }
            _isAxisAssigned = false;
        }
        
        public void OnMouseDrag(MouseMoveEventArgs mouseMoveEventArgs)
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
            
            Interaction.Invoke(_axis, (_hitPoint - _lastPoint)[XAxis]);
            UpdateLastPoint();
        }

        private Vector3 RaycastAxis(int axis, int mousePositionX, int mousePositionY, out Vector3 hitPoint)
        {
            switch (axis)
            {
                case XAxis:
                    var xyIntersect = Raycast(mousePositionX, mousePositionY, Vector3.UnitX, out var xyHitPoint);
                    if (!xyIntersect)
                    {
                        Raycast(mousePositionX, mousePositionY, -Vector3.UnitX, out xyHitPoint);
                    }
                    Raycast(mousePositionX, mousePositionY, Vector3.UnitY, out var xzHitPoint);
                    
                    var xyDistance = Vector3.Distance(xyHitPoint, _camera.WorldSpaceLocation);
                    var xzDistance = Vector3.Distance(xzHitPoint, _camera.WorldSpaceLocation);

                    if (xyDistance > xzDistance)
                    {
                        hitPoint = xyHitPoint;
                        return xyIntersect ? Vector3.UnitX : -Vector3.UnitX;
                    }
                    
                    hitPoint = xzHitPoint;
                    return Vector3.UnitY;
                case YAxis:
                    var normal = _camera.WorldSpaceLocation - WorldSpaceLocation;
                    normal.Y = 0;
                    normal.Normalize();
                    
                    Raycast(mousePositionX, mousePositionY, normal, out var adaptiveHitPoint);

                    hitPoint = adaptiveHitPoint;
                    return normal;
                case ZAxis:
                    var zyIntersect = Raycast(mousePositionX, mousePositionY, Vector3.UnitZ, out var zyHitPoint);
                    if (!zyIntersect)
                    {
                        Raycast(mousePositionX, mousePositionY, -Vector3.UnitZ, out zyHitPoint);
                    }
                    Raycast(mousePositionX, mousePositionY, Vector3.UnitY, out var zxHitPoint);
                    
                    var zyDistance = Vector3.Distance(zyHitPoint, _camera.WorldSpaceLocation);
                    var zxDistance = Vector3.Distance(zxHitPoint, _camera.WorldSpaceLocation);

                    if (zyDistance > zxDistance)
                    {
                        hitPoint = zyHitPoint;
                        return zyIntersect ? Vector3.UnitZ : -Vector3.UnitZ;
                    }
                    
                    hitPoint = zxHitPoint;
                    return Vector3.UnitY;
            }
            
            hitPoint = Vector3.Zero;
            return Vector3.Zero;
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