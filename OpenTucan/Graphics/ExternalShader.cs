using OpenTucan.Common;

namespace OpenTucan.Graphics
{
    public class ExternalShader : Shader
    {
        public ExternalShader(string vertexShader, string fragmentShader) : base(vertexShader, fragmentShader) { }

        protected override void BindAttributes()
        {
            BindAttribute(0, ShaderConsts.InVertex);
            BindAttribute(1, ShaderConsts.InNormal);
            BindAttribute(2, ShaderConsts.InUV);
        }
    }
}