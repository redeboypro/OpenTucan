using System;
using System.IO;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Editor
{
    public class EditorWindow : GameWindow
    {
        private EditorController _controller;
        private readonly Color4 _backgroundColor;
        
        public EditorWindow(int width, int height)
        {
            Title = "TucanEditor - Development Build";
            Width = width;
            Height = height;
            WindowBorder = WindowBorder.Fixed;
            _backgroundColor = new Color4(0.94f, 0.94f, 0.94f, 1f);
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.DepthTest);
                
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            
            var directorySelector = new FolderBrowserDialog();
            directorySelector.ShowDialog();
            var path = directorySelector.SelectedPath;

            if (!Directory.Exists(path))
            {
                Exit();
                return;
            }
            
            _controller = new EditorController(this, path);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(_backgroundColor);
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            _controller.GUI.OnRenderFrame(e);
            SwapBuffers();
        }
    }
}