namespace OpenTucan.Graphics
{
    public class BasicShader : ExternalShader
    {
        private const string VertexShader = @"
#version 150

in vec3 InVertex;
in vec2 InUV;
in vec3 InNormal;
in int InBoneId;

out vec2 PassUV;

const int MAX_BONES = 128;

uniform bool HasBones;
uniform mat4 ModelMatrix;
uniform mat4 BonesMatrices[MAX_BONES];

uniform mat4 ProjectionMatrix;
uniform mat4 ViewMatrix;

void main(void) {
    mat4 skinnedPosition = ModelMatrix;
	gl_Position = ProjectionMatrix * ViewMatrix * skinnedPosition * vec4(InVertex, 1.0);
	PassUV = InUV;
}
";
        
        private const string FragmentShader = @"
#version 150

in vec2 PassUV;

out vec4 OutColor;

uniform sampler2D MainTexture;

void main(void) {
	OutColor = texture(MainTexture, PassUV);
}
";
        
        public BasicShader() : base(VertexShader, FragmentShader) { }
    }
}