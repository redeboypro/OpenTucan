using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTucan.Graphics;
using OpenTucan.GUI;
using OpenTucan.Input;
using Font = OpenTucan.GUI.Font;

namespace FPSLevelEditor
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var input = new Input();

            var display = new GameWindow
            {
                Width = 800, Height = 600, Title = "Level Editor v1.0"
            };
            
            var guiController = new GUIController(display);

            display.Load += (sender, eventArgs) =>
            {
                GL.Enable(EnableCap.Blend);
                GL.Enable(EnableCap.CullFace);
                GL.Enable(EnableCap.Texture2D);
                GL.Enable(EnableCap.DepthTest);
                
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                
                var font = new Font(new Texture(new Bitmap("font.png")));
                guiController = new GUIController(display);
                guiController.Text("Top!", font, 0, 0, 0.5f, 0.5f);
            };

            display.UpdateFrame += (sender, eventArgs) =>
            {
                input.OnUpdateFrame();
            };

            display.RenderFrame += (sender, eventArgs) =>
            {
                GL.ClearColor(Color4.CornflowerBlue);
                GL.Viewport(0, 0, display.Width, display.Height);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                
                guiController.OnRenderFrame(eventArgs, display);
                
                display.SwapBuffers();
            };
            
            display.Run();
        }
    }
}