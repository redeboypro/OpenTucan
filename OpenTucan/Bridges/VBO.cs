using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace OpenTucan.Bridges
{
    public struct VBO : IBO
    {
        private readonly VertexAttribPointerType _attribPointerType;
        
        public VBO(int attributeLocation, int dim, VertexAttribPointerType pointerType = VertexAttribPointerType.Float)
        {
            AttributeLocation = attributeLocation;
            Dim = dim;
            Id = -0x01;
            _attribPointerType = pointerType;
        }
        
        public int AttributeLocation { get; }
        
        public int Dim { get; }
        
        public int Id { get; private set; }

        public void Create<T>(T[] data) where T : struct
        {
            GL.GenBuffers(1, out int id);
            Id = id;
            GL.BindBuffer(BufferTarget.ArrayBuffer, Id);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) (data.Length * Marshal.SizeOf<T>()), data, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(AttributeLocation, Dim, _attribPointerType, false, 0, 0);
        }
        
        public void Update<T>(T[] data) where T : struct
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, Id);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr) (data.Length * Marshal.SizeOf<T>()), data);
        }
        
        public void Delete()
        {
            GL.DeleteBuffer(Id);
        }
    }
}