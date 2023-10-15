using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTucan.Common;
using OpenTucan.Entities;

namespace OpenTucan.GUI
{
    public class GUIControl : Entity
    {
        private readonly Vector3[] _vertices;
        private readonly Vector3[] _sharedVertices;
        
        private bool _isPressed;

        private Color4 _color;

        public GUIControl()
        {
            Color = Color4.White;
            _vertices = new[]
            {
                new Vector3(-1, 1, 0), 
                new Vector3(-1, -1, 0),
                new Vector3(1, -1, 0),
                new Vector3(1, 1, 0)
            };
            _sharedVertices = new Vector3[4];
            OnTransformMatrices();
        }

        public bool IsMasked { get; set; }
        
        public Color4 Color 
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
                ChangeColor?.Invoke(_color);
            }
        }
        
        public Action<Color4> ChangeColor { get; set; }

        public Action<Vector2> Press { get; set; }
        
        public Action Release { get; set; }

        public Action<Vector2, Vector2> Drag { get; set; }
        
        public Action<KeyPressEventArgs> KeyPress { get; set; }
        
        public Action<KeyboardKeyEventArgs> KeyDown { get; set; }
        
        public Action<KeyboardKeyEventArgs> KeyUp { get; set; }

        public Action<FrameEventArgs> Render { get; set; }

        public bool ContainsPoint(Vector2 point)
        {
            var firstTriangleContainsPoint = PointIsInsideTriangle(point, _sharedVertices[0], _sharedVertices[1], _sharedVertices[3]);
            var secondTriangleContainsPoint = PointIsInsideTriangle(point, _sharedVertices[1], _sharedVertices[2], _sharedVertices[3]);
            return firstTriangleContainsPoint || secondTriangleContainsPoint;
        }

        public void OnKeyDown(KeyboardKeyEventArgs eventArgs)
        {
            KeyDown?.Invoke(eventArgs);
            
            for (var childId = 0; childId < GetChildrenAmount(); childId++)
            {
                ((GUIControl) GetChild(childId)).OnKeyDown(eventArgs);
            }
        }
        
        public void OnKeyUp(KeyboardKeyEventArgs eventArgs)
        {
            KeyUp?.Invoke(eventArgs);
            
            for (var childId = 0; childId < GetChildrenAmount(); childId++)
            {
                ((GUIControl) GetChild(childId)).OnKeyUp(eventArgs);
            }
        }
        
        public void OnKeyPress(KeyPressEventArgs eventArgs)
        {
            KeyPress?.Invoke(eventArgs);
            
            for (var childId = 0; childId < GetChildrenAmount(); childId++)
            {
                ((GUIControl) GetChild(childId)).OnKeyPress(eventArgs);
            }
        }

        public void OnMouseDown(Vector2 mousePos)
        {
            if (!ContainsPoint(mousePos) || !IsActive) 
            {
                return;
            }
            
            Press?.Invoke(mousePos);

            _isPressed = true;
            for (var childId = 0; childId < GetChildrenAmount(); childId++)
            {
                ((GUIControl) GetChild(childId)).OnMouseDown(mousePos);
            }
        }
        
        public void OnMouseUp() 
        {
            if (!IsActive || !_isPressed) 
            {
                return;
            }
            
            Release?.Invoke();
            
            _isPressed = false;
            for (var childId = 0; childId < GetChildrenAmount(); childId++)
            {
                ((GUIControl) GetChild(childId)).OnMouseUp();
            }
        }
        
        public void OnMouseMove(Vector2 mousePos, Vector2 mouseDelta) 
        {
            if (!IsActive) 
            {
                return;
            }
            
            if (_isPressed) 
            {
                Drag?.Invoke(mousePos, mouseDelta);
            }

            for (var childId = 0; childId < GetChildrenAmount(); childId++)
            {
                ((GUIControl) GetChild(childId)).OnMouseMove(mousePos, mouseDelta);
            }
        }
        
        public void OnRenderFrame(FrameEventArgs eventArgs, INativeWindow window) 
        {
            if (!IsActive) 
            {
                return;
            }
            
            Render?.Invoke(eventArgs);

            var scissorLocation = Ortho.RectToScreen(
                WorldSpaceLocation.X - WorldSpaceScale.X,
                WorldSpaceLocation.Y - WorldSpaceScale.Y,
                window.Width,
                window.Height);
            
            var scissorSize = Ortho.RectToScreen(
                WorldSpaceScale.X * 2 - 1,
                WorldSpaceScale.Y * 2 - 1,
                window.Width,
                window.Height);
            
            if (IsMasked)
            {
                GL.Scissor((int) scissorLocation.X, (int) scissorLocation.Y, (int) scissorSize.X, (int) scissorSize.Y);
                GL.Enable(EnableCap.ScissorTest);
            }

            for (var childId = 0; childId < GetChildrenAmount(); childId++)
            {
                GL.Clear(ClearBufferMask.DepthBufferBit);
                ((GUIControl) GetChild(childId)).OnRenderFrame(eventArgs, window);
            }
            
            if (IsMasked) 
            {
                GL.Disable(EnableCap.ScissorTest);
            }
        }

        protected sealed override void OnTransformMatrices()
        {
            if (_vertices is null || _sharedVertices is null)
            {
                return;
            }
            
            for (var i = 0; i < _vertices.Length; i++)
            {
                _sharedVertices[i] = WorldSpaceRotation * (_vertices[i] * WorldSpaceScale) + WorldSpaceLocation;
            }
        }
        
        private static float Sign(Vector2 p1, Vector3 p2, Vector3 p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        private static bool PointIsInsideTriangle (Vector2 point, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            var d1 = Sign(point, v1, v2);
            var d2 = Sign(point, v2, v3);
            var d3 = Sign(point, v3, v1);

            var hasNegative = d1 < 0 || d2 < 0 || d3 < 0;
            var hasPositive = d1 > 0 || d2 > 0 || d3 > 0;

            return !(hasNegative && hasPositive);
        }
    }
}