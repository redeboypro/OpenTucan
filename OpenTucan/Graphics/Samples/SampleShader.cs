using OpenTK;
using OpenTK.Graphics;

namespace OpenTucan.Graphics.Samples
{
    public class SampleShader : Shader
    {
        private const string vertexShaderCode = @"
#version 400 core

in vec3 vertex;
in vec2 uv;
in vec3 normal;

out vec2 fragUV;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

void main(void) 
{
    vec4 wldPosition = modelMatrix * vec4(vertex, 1.0);
	gl_Position = projectionMatrix * viewMatrix * wldPosition;
    fragUV = uv;
}
";
            
        private const string fragmentShaderCode = @"
#version 400 core

in vec2 fragUV;

out vec4 outColor;

uniform sampler2D imgTexture;

void main(void) 
{
	outColor = texture(imgTexture, fragUV);
}
";
            
        public SampleShader() : base(vertexShaderCode, fragmentShaderCode) { }

        protected override void BindAttributes()
        {
            BindAttribute(0, "vertex");
            BindAttribute(1, "uv");
            BindAttribute(2, "normal");
        }
        
        public void SetProjectionMatrix(Matrix4 projectionMatrix)
        {
            SetUniform("projectionMatrix", projectionMatrix);
        }
        
        public void SetViewMatrix(Matrix4 viewMatrix)
        {
            SetUniform("viewMatrix", viewMatrix);
        }
        
        public void SetModelMatrix(Matrix4 modelMatrix)
        {
            SetUniform("modelMatrix", modelMatrix);
        }
    }
}