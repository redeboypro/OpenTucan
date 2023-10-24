using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTucan.Entities;
using OpenTucan.GUI;
using OpenTucan.Input;

namespace OpenTucan
{
    public abstract class TucanApplication : GameWindow
    {
        private World _world;
        private GUIController _guiController;
        private Color4 _backgroundColor;
        private float _frameTime;
        private int _fps;

        protected TucanApplication(string title, int windowWidth, int windowHeight, Color4 backgroundColor)
        {
            VSync = VSyncMode.Off;
            Title = title;
            Width = windowWidth;
            Height = windowHeight;
            WindowBorder = WindowBorder.Fixed;
            _backgroundColor = backgroundColor;

            if (Instance is null)
            {
                Instance = this;
            }
        }
        
        public World World
        {
            get
            {
                return _world;
            }
        }

        public GUIController GUIController
        {
            get
            {
                return _guiController;
            }
        }

        public int FramesPerSecond { get; private set; }

        protected override void OnLoad(EventArgs e)
        {
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.DepthTest);
                
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            
            InputManager.OnLoad();
            
            _world = new World();
            _guiController = new GUIController(this);

            PrepareStart();
            _world.Start();
            PostStart();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            InputManager.OnUpdateFrame();
            
            _frameTime += (float) e.Time;
            _fps++;

            if (_frameTime >= 1.0f)
            {
                FramesPerSecond = _fps;
                _frameTime = 0.0f;
                _fps = 0;
            }
            
            PrepareUpdate(e);
            _world.Update(e);
            PostUpdate(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(_backgroundColor);
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            PrepareRender(e);
            _world.Render(e);
            _guiController.OnRenderFrame(e, this);
            PostRender(e);
            
            SwapBuffers();
        }

        public void SetBackgroundColor(Color4 color)
        {
            _backgroundColor = color;
        }

        protected virtual void PrepareStart() { }
        protected virtual void PrepareUpdate(FrameEventArgs eventArgs) { }
        protected virtual void PrepareRender(FrameEventArgs eventArgs) { }
        protected virtual void PostStart() { }
        protected virtual void PostUpdate(FrameEventArgs eventArgs) { }
        protected virtual void PostRender(FrameEventArgs eventArgs) { }

        public static TucanApplication Instance { get; private set; }
    }
}