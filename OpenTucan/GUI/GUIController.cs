using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTucan.Bridges;
using OpenTucan.Common;
using OpenTucan.Graphics;

namespace OpenTucan.GUI
{
    public class GUIController
    {
        private readonly List<GUIControl> _elements;
        private Vector2 _lastMousePos;

        public GUIController(INativeWindow window)
        {
            _elements = new List<GUIControl>();
            SimpleVAO = new VAO();
            SimpleVAO.CreateVertexBufferObject(0, 2, SimpleVertices);
            SimpleVAO.CreateVertexBufferObject(1, 2, SimpleUV);
            ShaderProgram = new GUIShader();
            Window = window;

            window.MouseDown += (sender, args) =>
            {
                _lastMousePos = Ortho.ScreenToRect(args.X, args.Y, window.Width, window.Height);
                _elements.ForEach(element => element.OnMouseDown(_lastMousePos));
            };
            
            window.MouseUp += (sender, args) =>
            {
                _elements.ForEach(element => element.OnMouseUp());
            };
            
            window.MouseMove += (sender, args) =>
            {
                var currentMousePos = Ortho.ScreenToRect(args.X, args.Y, window.Width, window.Height);
                _elements.ForEach(element => element.OnMouseMove(currentMousePos, currentMousePos - _lastMousePos));
            };

            window.KeyPress += (sender, args) =>
            {
                _elements.ForEach(element => element.OnKeyPress(args));
            };

            window.KeyDown += (sender, args) =>
            {
                _elements.ForEach(element => element.OnKeyDown(args));
            };
            
            window.KeyUp += (sender, args) =>
            {
                _elements.ForEach(element => element.OnKeyUp(args));
            };
        }

        public INativeWindow Window { get; }

        public Shader ShaderProgram { get; }

        public VAO SimpleVAO { get; }

        public Text Text(string text, Font font, float x, float y, float width, float height)
        {
            var textElementInstance = new Text(text, font, this)
            {
                WorldSpaceLocation = new Vector3(x, y, 0),
                WorldSpaceScale = new Vector3(width, height, 1)
            };
            AddElement(textElementInstance);

            return textElementInstance;
        }
        
        public Image Image(Texture texture, float x, float y, float width, float height)
        {
            var textElementInstance = new Image(texture, this)
            {
                WorldSpaceLocation = new Vector3(x, y, 0),
                WorldSpaceScale = new Vector3(width, height, 1)
            };
            AddElement(textElementInstance);

            return textElementInstance;
        }
        
        public ListView ListView(Texture texture, float x, float y, float width, float height)
        {
            var listElementInstance = new ListView(texture, this)
            {
                WorldSpaceLocation = new Vector3(x, y, 0),
                WorldSpaceScale = new Vector3(width, height, 1)
            };
            AddElement(listElementInstance);

            return listElementInstance;
        }
        
        public InputField InputField(string message, Texture texture, Font font, float x, float y, float width, float height)
        {
            var inputFieldElementInstance = new InputField(message, texture, font, this)
            {
                WorldSpaceLocation = new Vector3(x, y, 0),
                WorldSpaceScale = new Vector3(width, height, 1)
            };
            AddElement(inputFieldElementInstance);

            return inputFieldElementInstance;
        }
        
        public Checkbox Checkbox(Texture checkTexture, Texture backgroundTexture, float x, float y, float width, float height)
        {
            var checkBoxElementInstance = new Checkbox(checkTexture, backgroundTexture, this)
            {
                WorldSpaceLocation = new Vector3(x, y, 0),
                WorldSpaceScale = new Vector3(width, height, 1)
            };
            AddElement(checkBoxElementInstance);

            return checkBoxElementInstance;
        }
        
        public int GetElementCount()
        {
            return _elements.Count;
        }

        public GUIControl GetElement(int index)
        {
            return _elements[index];
        }

        public void AddElement(GUIControl item)
        {
            _elements.Add(item);
        }
        
        public void RemoveElement(GUIControl item)
        {
            _elements.Remove(item);
        }

        public void OnRenderFrame(FrameEventArgs eventArgs, INativeWindow window)
        {
            ShaderProgram.Start();
            foreach (var element in _elements)
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

        public class GUIShader : Shader
        {
            private const string vertexShaderCode = @"
#version 150

in vec2 vertex;
in vec2 uv;

out vec2 passUV;
out vec2 passVertex;

uniform mat4 modelMatrix;

void main(void) 
{
	gl_Position = modelMatrix * vec4(vertex, 0.0, 1.0);
	passUV = uv;
    passVertex = vertex;
}
";
            
            private const string fragmentShaderCode = @"
#version 150

in vec2 passUV;
in vec2 passVertex;

out vec4 outColor;

uniform vec2 extents;
uniform float radius;
uniform sampler2D img;
uniform vec4 color;

float calcDistance() {
    vec2 coords = abs(passVertex) * (extents + radius);
    vec2 delta = max(coords - extents, 0);
    return length(delta);
}

void main(void) 
{
    float dist = calcDistance();
    if (dist > radius) discard;
	outColor = color * texture(img, passUV);
}
";
            
            public GUIShader() : base(vertexShaderCode, fragmentShaderCode) { }

            protected override void BindAttributes()
            {
                BindAttribute(0, "vertex");
                BindAttribute(1, "uv");
            }

            public void SetExtents(float x, float y)
            {
                SetUniform("extents", x, y);
            }
            
            public void SetRadius(float radius)
            {
                SetUniform("radius", radius);
            }
            
            public void SetColor(Color4 color)
            {
                SetUniform("color", color);
            }
        }
    }
}