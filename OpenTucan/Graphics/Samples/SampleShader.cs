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
out vec3 fragNormal;
out vec3 vertexRelativeLight;
out vec3 vertexRelativeCamera;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;
uniform vec3 lightPosition;

void main(void) 
{
    vec4 wldPosition = modelMatrix * vec4(vertex, 1.0);
	gl_Position = projectionMatrix * viewMatrix * wldPosition;
    fragUV = uv;
	
    fragNormal = (modelMatrix * vec4(normal, 0.0)).xyz;
    vertexRelativeLight = lightPosition - wldPosition.xyz;
    vertexRelativeCamera = (inverse(viewMatrix) * vec4(0.0, 0.0, 0.0, 1.0)).xyz - wldPosition.xyz;
}
";
            
        private const string fragmentShaderCode = @"
#version 400 core

in vec2 fragUV;
in vec3 fragNormal;
in vec3 vertexRelativeLight;
in vec3 vertexRelativeCamera;

out vec4 outColor;

uniform float reflectivity;
uniform float shininess;
uniform vec4 lightColor;
uniform sampler2D imgTexture;

void main(void) 
{
    vec3 unitNormal = normalize(fragNormal);
    vec3 lightDirection = normalize(vertexRelativeLight);

    float normalLightAngle = dot(unitNormal, lightDirection);
    float brightness = max(normalLightAngle, 0.2);
    vec3 diffuse = brightness * lightColor.rgb;
    
    vec3 cameraDirection = normalize(vertexRelativeCamera);
    lightDirection = -lightDirection;
    vec3 reflectedLightDirection = reflect(lightDirection, unitNormal);

    float specularFactor = dot(reflectedLightDirection, cameraDirection);
    specularFactor = max(specularFactor, 0.0);
    float dampedFactor = pow(specularFactor, shininess);
	vec3 resultSpecular = dampedFactor * reflectivity * lightColor.rgb;

	outColor = vec4(diffuse, 1.0) * texture(imgTexture, fragUV) * vec4(resultSpecular, 1.0);
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

        public void SetLightColor(Color4 lightColor)
        {
            SetUniform("lightColor", lightColor);
        }
        
        public void SetLightPosition(Vector3 lightPosition)
        {
            SetUniform("lightPosition", lightPosition);
        }
        
        public void SetShininess(float shininess)
        {
            SetUniform("shininess", shininess);
        }
        
        public void SetReflectivity(float reflectivity)
        {
            SetUniform("reflectivity", reflectivity);
        }
    }
}