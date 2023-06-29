using System.Collections.Generic;
using OpenTK;
using OpenTucan.Bridges;
using OpenTucan.Graphics;

namespace OpenTucan.GUI
{
    public class GUIController
    {
        private readonly List<GUIControl> elements;

        public GUIController(INativeWindow window)
        {
            elements = new List<GUIControl>();
            SimpleVAO = new VAO();
            SimpleVAO.CreateVertexBufferObject(0, 2, SimpleVertices);
            SimpleVAO.CreateVertexBufferObject(1, 2, SimpleUV);
            ShaderProgram = new GUIShader();
        }
        
        public Shader ShaderProgram { get; }

        public VAO SimpleVAO { get; }

        public Text Text(string text, Font font, float x, float y, float width, float height)
        {
            var textElementInstance = new Text(text, font, this)
            {
                WorldSpaceLocation = new Vector3(x, y, 0),
                WorldSpaceScale = new Vector3(width, height, 1)
            };
            AddItem(textElementInstance);

            return textElementInstance;
        }
        
        public Image Image(Texture texture, float x, float y, float width, float height)
        {
            var textElementInstance = new Image(texture, this)
            {
                WorldSpaceLocation = new Vector3(x, y, 0),
                WorldSpaceScale = new Vector3(width, height, 1)
            };
            AddItem(textElementInstance);

            return textElementInstance;
        }

        public void AddItem(GUIControl item)
        {
            elements.Add(item);
        }
        
        public void RemoveItem(GUIControl item)
        {
            elements.Remove(item);
        }

        public void OnRenderFrame(FrameEventArgs eventArgs, INativeWindow window)
        {
            ShaderProgram.Start();
            foreach (var element in elements)
            {
                element.OnRenderFrame(eventArgs, window);
            }
            ShaderProgram.Stop();
        }

        public static readonly float[] SimpleVertices = {
            //Northwest
            -1, 1,
            
            //Southwest
            -1, -1,
            
            //Northeast
            1, 1,
            
            //Southeast
            1, -1
        };
        
        public static readonly float[] SimpleUV = {
            //Northwest
            0, 0,

            //Southwest
            0, 1,
            
            //Northeast
            1, 0,
            
            //Southeast
            1, 1
        };

        private class GUIShader : Shader
        {
            private const string vertexShaderCode = @"
#version 150

in vec2 vertex;
in vec2 uv;

out vec2 passUV;

uniform mat4 modelMatrix;

void main(void) 
{
	gl_Position = modelMatrix * vec4(vertex, 0.0, 1.0);
	passUV = uv;
}
";
            
            private const string fragmentShaderCode = @"
#version 150

in vec2 passUV;

out vec4 outColor;

uniform sampler2D img;

void main(void) 
{
	outColor = texture(img, passUV);
}
";
            
            public GUIShader() : base(vertexShaderCode, fragmentShaderCode) { }

            protected override void BindAttributes()
            {
                BindAttribute(0, "vertex");
                BindAttribute(1, "uv");
            }
        }
    }
}