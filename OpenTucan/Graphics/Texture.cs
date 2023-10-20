using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace OpenTucan.Graphics
{
    public delegate void LockIntoSysMemoryEvent(BitmapData data);
    
    public class Texture
    {
        private const int ColorLimit = 0xFF;
        
        private readonly Bitmap _bitmap;
        
        public Texture(Bitmap bitmap)
        {
            _bitmap = bitmap;
            
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.GenTextures(1, out int id);
            
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            LockIntoSystemMemory(data =>
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, 
                    PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            });
            
            GL.BindTexture(TextureTarget.Texture2D, 0);
            Id = id;
        }

        public Texture(int width, int height) : this(new Bitmap(width, height)) { }

        public int Id { get; }

        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, Id);
        }

        public void SetPixel(int x, int y, Color4 color)
        {
            _bitmap.SetPixel(x, y, Color.FromArgb(
                (int) (color.A * ColorLimit),
                (int) (color.R * ColorLimit),
                (int) (color.G * ColorLimit),
                (int) (color.B * ColorLimit)
            ));
        }

        public Color4 GetPixel(int x, int y)
        {
            var pixel = _bitmap.GetPixel(x, y);
            return new Color4
            {
                A = (float) pixel.A / ColorLimit,
                R = (float) pixel.R / ColorLimit,
                G = (float) pixel.G / ColorLimit,
                B = (float) pixel.B / ColorLimit,
            };
        }

        public void Apply()
        {
            GL.BindTexture(TextureTarget.Texture2D, Id);
            LockIntoSystemMemory(data =>
            {
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, data.Width, data.Height, PixelFormat.Bgra,
                    PixelType.UnsignedByte, data.Scan0);
            });
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Delete()
        {
            GL.DeleteTexture(Id);
        }

        private void LockIntoSystemMemory(LockIntoSysMemoryEvent e)
        {
            var safeData = _bitmap.LockBits(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            
            e.Invoke(safeData);
            
            _bitmap.UnlockBits(safeData);
        }
    }
}