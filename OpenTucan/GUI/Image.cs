using OpenTK.Graphics.OpenGL;
using OpenTucan.Graphics;

namespace OpenTucan.GUI
{
    public class Image : GUIControl
    {
        private Texture texture;

        public Image(Texture texture, GUIController controller)
        {
            this.texture = texture;
            Render += args =>
            {
                var shader = controller.ShaderProgram;

                GL.ActiveTexture(TextureUnit.Texture0);
                texture.Bind();

                shader.SetUniform("modelMatrix", GetModelMatrix());

                GL.BindVertexArray(controller.SimpleVAO.Id);
                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);

                GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

                GL.DisableVertexAttribArray(0);
                GL.DisableVertexAttribArray(1);
                GL.BindVertexArray(0);
            };
        }
    }
}