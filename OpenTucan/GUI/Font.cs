using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using OpenTucan.Bridges;
using OpenTucan.Graphics;

namespace OpenTucan.GUI
{
    public class Font
    {
        private readonly Texture _fontAtlasTexture;
        private readonly List<VAO> _charVAOs;

        public Font(Texture fontAtlasTexture)
        {
            _fontAtlasTexture = fontAtlasTexture;
            _charVAOs = new List<VAO>();
            var uv = new float[8];
            
            foreach (var c in Sheet)
            {
                const float charSize = 1 / 16f;
                var y = c >> 4;
                var x = c & 0b1111;

                var left = x * charSize;
                var right = left + charSize;
                var top = y * charSize;
                var bottom = top + charSize;

                uv[0] = uv[2] = left;
                uv[4] = uv[6] = right;
                uv[1] = uv[5] = top;
                uv[3] = uv[7] = bottom;

                var vao = new VAO();
                vao.CreateVertexBufferObject(0, 2, GUIController.SimpleVertices);
                vao.CreateVertexBufferObject(1, 2, uv);
                _charVAOs.Add(vao);
            }
        }

        public VAO this[char c]
        {
            get
            {
                return _charVAOs[Sheet.IndexOf(c)];
            }
        }
        
        public Texture AtlasTexture
        {
            get
            {
                return _fontAtlasTexture;
            }
        }

        public bool Contains(char c)
        {
            return Sheet.Any(x => x == c);
        }

        private const string Sheet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()-=_+[]{}\\|;:'\".,<>/?`~ ";
    }
}