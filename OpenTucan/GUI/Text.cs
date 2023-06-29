using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenTucan.GUI
{
    public class Text : GUIControl
    {
        private string text;
        private Font font;

        public Text(string text, Font font, GUIController controller)
        {
            this.text = text;
            this.font = font;
            Render += args =>
            {
                var shader = controller.ShaderProgram;
                
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, font.AtlasTexture.Id);

                var completelyScale = LocalSpaceScale;
                
                var charHalfWidth = completelyScale.X / text.Length;
                
                completelyScale.X = charHalfWidth;
                
                var horizontalPos = LocalSpaceLocation.X - LocalSpaceScale.X + charHalfWidth;
                
                foreach (var c in text)
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
                                 GetModelMatrix();

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
    }
}