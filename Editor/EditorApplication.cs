using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTucan;
using OpenTucan.Entities;
using OpenTucan.GUI;
using OpenTucan.GUI.Advanced;

namespace Editor
{
    public class EditorApplication : TucanApplication
    {
        private GUIXml _guiXml;
        private TransformGizmo _transformGizmo;
        private Font _font;
        
        public EditorApplication(string title, int windowWidth, int windowHeight, Color4 backgroundColor) : base(title, windowWidth, windowHeight, backgroundColor) { }

        protected override void PrepareStart()
        {
            var camera = new Camera(Width, Height);
            _transformGizmo = new TransformGizmo(camera, this)
            {
                WorldSpaceLocation = Vector3.UnitZ * 5
            };
            _transformGizmo.Interaction += (axis, interpolation) =>
            {
                var sign = Math.Sign(interpolation.X + interpolation.Y + interpolation.Z);
                switch (axis)
                {
                    case 0:
                        _transformGizmo.WorldSpaceLocation += _transformGizmo.Right(Space.Global) * interpolation.Length * sign;
                        break;
                    case 1:
                        _transformGizmo.WorldSpaceLocation += _transformGizmo.Up(Space.Global) * interpolation.Length * sign;
                        break;
                    case 2:
                        _transformGizmo.WorldSpaceLocation += _transformGizmo.Front(Space.Global) * interpolation.Length * sign;
                        break;
                }
            };
        }

        protected override void PostRender(FrameEventArgs eventArgs)
        {
            _transformGizmo.OnRenderFrame();
        }
    }
}