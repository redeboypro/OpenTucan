using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenTucan.GUI
{
    public class Text : GUIControl
    {
        public Text(string text, Font font, GUIController controller)
        {
            Color = Color4.White;
            Content = text;
            Render += args =>
            {
                var shader = (GUIController.GUIShader) controller.ShaderProgram;
                shader.SetColor(Color);
                
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, font.AtlasTexture.Id);

                var completelyScale = Vector3.One;
                
                var charHalfWidth = completelyScale.X / text.Length;
                
                completelyScale.X = charHalfWidth;

                var horizontalPos = -1f;
                horizontalPos += charHalfWidth;
                
                foreach (var c in Content)
                {
                    if (!font.Contains(c)) 
                    {
                        continue;
                    }
                    
                    var matrix = Matrix4.CreateScale(completelyScale) *
                                 Matrix4.CreateTranslation(new Vector3
                                 {
                                     X = horizontalPos
                                 }) * 
                                 GetGlobalMatrix();

                    horizontalPos += charHalfWidth * 2;
                
                    shader.SetUniform("modelMatrix", matrix);

                    GL.BindVertexArray(font[c].Id);
                    GL.EnableVertexAttribArray(0);
                    GL.EnableVertexAttribArray(1);

                    GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

                    GL.DisableVertexAttribArray(0);
                    GL.DisableVertexAttribArray(1);
                }
            
                GL.BindVertexArray(0);
            };
        }

        public string Content { get; set; }
    }
}