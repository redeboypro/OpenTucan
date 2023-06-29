using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace OpenTucan.Bridges
{
    public class VAO
    {
        public readonly Dictionary<int, VBO> VertexBufferObjects;

        public VAO()
        {
            GL.GenVertexArrays(1, out int id);
            VertexBufferObjects = new Dictionary<int, VBO>();
            ElementBufferObject = new EBO();
            Id = id;
        }

        public int Id { get; }
        
        public EBO ElementBufferObject { get; }
        

        public void CreateElementBufferObject<T>(T[] data) where T : struct
        {
            GL.BindVertexArray(Id);
            ElementBufferObject.Create(data);
            GL.BindVertexArray(0);
        }
        
        public void UpdateElementBufferObject<T>(T[] data) where T : struct
        {
            GL.BindVertexArray(Id);
            ElementBufferObject.Update(data);
            GL.BindVertexArray(0);
        }
        
        public void CreateVertexBufferObject<T>(int attributeLocation, int dim, T[] data) where T : struct
        {
            if (VertexBufferObjects.ContainsKey(attributeLocation))
            {
                throw new Exception("VAO: Vertex buffer is already instantiated!");
            }
            GL.BindVertexArray(Id);
            var vbo = new VBO(attributeLocation, dim);
            vbo.Create(data);
            VertexBufferObjects.Add(attributeLocation, vbo);
            GL.BindVertexArray(0);
        }

        public void UpdateVertexBufferObject<T>(int attributeLocation, T[] data) where T : struct
        {
            if (!VertexBufferObjects.ContainsKey(attributeLocation))
            {
                throw new Exception("VAO: Vertex buffer is not instantiated!");
            }
            GL.BindVertexArray(Id);
            VertexBufferObjects[attributeLocation].Update(data);
            GL.BindVertexArray(0);
        }

        public void Delete()
        {
            GL.BindVertexArray(Id);
            foreach (var vbo in VertexBufferObjects.Values)
            {
                vbo.Delete();
            }
            ElementBufferObject.Delete();
            GL.DeleteVertexArray(Id);
            GL.BindVertexArray(0);
        }
    }
}