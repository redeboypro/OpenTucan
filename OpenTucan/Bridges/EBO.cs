using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace OpenTucan.Bridges
{
    public struct EBO : IBO
    {
        public int Id { get; private set; }

        public void Create<T>(T[] data) where T : struct
        {
            GL.GenBuffers(1, out int id);
            Id = id;
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Id);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr) (data.Length * Marshal.SizeOf<T>()), data, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }
        
        public void Update<T>(T[] data) where T : struct
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Id);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, (IntPtr) (data.Length * Marshal.SizeOf<T>()), data);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }
        
        public void Delete()
        {
            GL.DeleteBuffer(Id);
        }
    }
}