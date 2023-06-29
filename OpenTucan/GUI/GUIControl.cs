using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTucan.Common;
using OpenTucan.Entities;

namespace OpenTucan.GUI
{
    public delegate void GUICallback(MouseEventArgs eventArgs);
    
    public delegate void RenderCallback(FrameEventArgs eventArgs);
        
    public class GUIControl : Entity
    {
        private readonly Vector3[] vertices;
        private readonly Vector3[] sharedVertices;

        private bool isActive;
        private bool isPressed;

        public GUIControl()
        {
            isActive = true;
            vertices = new[]
            {
                new Vector3(-1, 1, 0), 
                new Vector3(-1, -1, 0),
                new Vector3(1, -1, 0),
                new Vector3(1, 1, 0)
            };
            sharedVertices = new Vector3[4];
            OnTransformMatrices();
        }

        public bool IsActive
        {
            get
            {
                return isActive;
            }
        }
        
        public bool UseScissors { get; set; }
        
        public GUICallback Press { get; set; }
        
        public GUICallback Release { get; set; }
        
        public GUICallback Drag { get; set; }
        
        public RenderCallback Render { get; set; }

        public bool ContainsPoint(Vector2 point)
        {
            var x1 = sharedVertices[0].X;
            var y1 = sharedVertices[0].Y;
            
            var x2 = sharedVertices[1].X;
            var y2 = sharedVertices[1].Y;
            
            var x3 = sharedVertices[2].X;
            var y3 = sharedVertices[2].Y;
            
            var x4 = sharedVertices[3].X;
            var y4 = sharedVertices[3].Y;

            var a1 = Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
            var b1 = Math.Sqrt((x1 - point.X) * (x1 - point.X) + (y1 - point.Y) * (y1 - point.Y));
            
            var a2 = Math.Sqrt((x2 - x3) * (x2 - x3) + (y2 - y3) * (y2 - y3));
            var b2 = Math.Sqrt((x2 - point.X) * (x2 - point.X) + (y2 - point.Y) * (y2 - point.Y));
            
            var a3 = Math.Sqrt((x3 - x4) * (x3 - x4) + (y3 - y4) * (y3 - y4));
            var b3 = Math.Sqrt((x3 - point.X) * (x3 - point.X) + (y3 - point.Y) * (y3 - point.Y));
            
            var a4 = Math.Sqrt((x4 - x1) * (x4 - x1) + (y4 - y1) * (y4 - y1));
            var b4 = Math.Sqrt((x4 - point.X) * (x4 - point.X) + (y4 - point.Y) * (y4 - point.Y));

            var u1 = (a1 + b1 + b2) / 2;
            var u2 = (a2 + b2 + b3) / 2;
            var u3 = (a3 + b3 + b4) / 2;
            var u4 = (a4 + b4 + b1) / 2;

            var l1 = Math.Sqrt(u1 * (u1 - a1) * (u1 - b1) * (u1 - b2));
            var l2 = Math.Sqrt(u2 * (u2 - a2) * (u2 - b2) * (u2 - b3));
            var l3 = Math.Sqrt(u3 * (u3 - a3) * (u3 - b3) * (u3 - b4));
            var l4 = Math.Sqrt(u4 * (u4 - a4) * (u4 - b4) * (u4 - b1));

            var difference = l1 + l2 + l3 + l4 - a1 * a2;
            
            return difference < 1;
        }

        public void SetActive(bool state)
        {
            isActive = state;
        }

        public void OnMouseDown(MouseButtonEventArgs eventArgs, INativeWindow window)
        {
            if (!ContainsPoint(Ortho.ScreenToRect(eventArgs.X, eventArgs.Y, window.Width, window.Height)) || !isActive) 
            {
                return;
            }
            
            Press?.Invoke(eventArgs);
            
            isPressed = true;
            for (var childId = 0; childId < GetChildrenAmount(); childId++)
            {
                ((GUIControl) GetChild(childId)).OnMouseDown(eventArgs, window);
            }
        }
        
        public void OnMouseUp(MouseButtonEventArgs eventArgs, INativeWindow window) 
        {
            if (!isActive || !isPressed) 
            {
                return;
            }
            
            Release?.Invoke(eventArgs);
            
            isPressed = false;
            for (var childId = 0; childId < GetChildrenAmount(); childId++)
            {
                ((GUIControl) GetChild(childId)).OnMouseUp(eventArgs, window);
            }
        }
        
        public void OnMouseMove(MouseMoveEventArgs eventArgs, INativeWindow window) 
        {
            if (!isActive) 
            {
                return;
            }
            
            if (isPressed) 
            {
                Drag?.Invoke(eventArgs);
            }

            for (var childId = 0; childId < GetChildrenAmount(); childId++)
            {
                ((GUIControl) GetChild(childId)).OnMouseMove(eventArgs, window);
            }
        }
        
        public void OnRenderFrame(FrameEventArgs eventArgs, INativeWindow window) 
        {
            if (!isActive) 
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
            
            if (UseScissors)
            {
                GL.Scissor((int) scissorLocation.X, (int) scissorLocation.Y, (int) scissorSize.X, (int) scissorSize.Y);
                GL.Enable(EnableCap.ScissorTest);
            }

            for (var childId = 0; childId < GetChildrenAmount(); childId++)
            {
                ((GUIControl) GetChild(childId)).OnRenderFrame(eventArgs, window);
            }
            
            if (UseScissors) 
            {
                GL.Disable(EnableCap.ScissorTest);
            }
        }

        protected override void OnTransformMatrices()
        {
            if (vertices is null || sharedVertices is null)
            {
                return;
            }
            
            for (var i = 0; i < vertices.Length; i++)
            {
                sharedVertices[i] = WorldSpaceRotation * (vertices[i] * WorldSpaceScale) + WorldSpaceLocation;
            }
        }
    }
}