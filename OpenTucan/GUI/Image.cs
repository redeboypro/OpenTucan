using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTucan.Graphics;

namespace OpenTucan.GUI
{
    public class Image : GUIControl
    {
        private float _cornerRadius;

        public Image(Texture texture, GUIController controller)
        {
            _cornerRadius = 0;
            Texture = texture;
            Color = Color4.White;
            Render += args =>
            {
                var shader = (GUIController.GUIShader) controller.ShaderProgram;

                GL.ActiveTexture(TextureUnit.Texture0);
                texture.Bind();

                shader.SetUniform("modelMatrix", GetGlobalMatrix());
                shader.SetRadius(_cornerRadius);
                shader.SetExtents(WorldSpaceScale.X * 2, WorldSpaceScale.Y * 2);
                shader.SetColor(Color);

                GL.BindVertexArray(controller.SimpleVAO.Id);
                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);

                GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

                GL.DisableVertexAttribArray(0);
                GL.DisableVertexAttribArray(1);
                GL.BindVertexArray(0);
            };
        }
        
        public float GetCornerRadius()
        {
            return _cornerRadius;
        }

        public void SetCornerRadius(float radius)
        {
            _cornerRadius = radius;
        }

        public Texture Texture { get; }
    }
}