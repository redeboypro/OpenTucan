using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTucan.Entities;
using OpenTucan.Graphics;
using OpenTucan.Graphics.Samples;
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
            var physicsWorld = new List<Rigidbody>();

            SampleShader shader = null;
            Mesh mesh = null;
            Texture texture = null;

            Camera camera = null;
            Rigidbody rb = null;

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
                
                shader = new SampleShader();
                
                texture = new Texture(new Bitmap("ak47.png"));
                mesh = Mesh.FromFile("ak47_view.obj");

                camera = new Camera(display.Width, display.Height)
                {
                    WorldSpaceLocation = Vector3.UnitZ * -10
                };

                rb = new Rigidbody(physicsWorld, mesh.GetBoundsMinimum(), mesh.GetBoundsMaximum());
                
                var font = new Font(new Texture(new Bitmap("font.png")));
                guiController = new GUIController(display);
                guiController.Text("test", font, 0, 0, 0.5f, 0.5f);
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
                
                shader.Start();
                
                shader.SetShininess(0.01f);
                shader.SetReflectivity(0.01f);
                shader.SetLightColor(Color4.White);
                shader.SetLightPosition(Vector3.One * 9999);
                shader.SetProjectionMatrix(camera.ProjectionMatrix);
                shader.SetViewMatrix(camera.ViewMatrix);
                shader.SetModelMatrix(rb.GetModelMatrix());
                
                GL.ActiveTexture(TextureUnit.Texture0);
                texture.Bind();
                
                mesh.PrepareForRendering();
                
                shader.Stop();
                
                guiController.OnRenderFrame(eventArgs, display);
                
                display.SwapBuffers();
            };
            
            display.Run();
        }
    }
}