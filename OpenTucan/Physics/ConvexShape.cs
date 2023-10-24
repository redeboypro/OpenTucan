using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTucan.Entities;
using OpenTucan.Graphics;

namespace OpenTucan.Physics
{
    public class ConvexShape
    {
        private readonly IReadOnlyList<Vector3> _vertices;

        public ConvexShape(IReadOnlyList<Vector3> vertices)
        {
            _vertices = vertices;
        }
        
        public ConvexShape(params Vector3[] vertices)
        {
            _vertices = vertices;
        }
        
        public ConvexShape(Mesh mesh)
        {
            _vertices = mesh.Vertices;
        }

        public ConvexShape(Vector3 min, Vector3 max)
        {
            _vertices = new []
            {
                new Vector3(min.X, min.Y, min.Z), 
                new Vector3(min.X, max.Y, min.Z), 
                new Vector3(max.X, min.Y, min.Z), 
                new Vector3(max.X, max.Y, min.Z), 
                
                new Vector3(min.X, min.Y, max.Z), 
                new Vector3(min.X, max.Y, max.Z), 
                new Vector3(max.X, min.Y, max.Z), 
                new Vector3(max.X, max.Y, max.Z), 
            };
        }

        public Vector3 this[int index]
        {
            get
            {
                return _vertices[index];
            }
        }

        public int Size
        {
            get
            {
                return _vertices.Count;
            }
        }

        public Vector3 FindFurthestPoint(Entity entity, Vector3 direction)
        {
            var maxPoint = _vertices[_vertices.Count - 1];
            var maxDistance = float.NegativeInfinity;

            foreach (var vertex in _vertices)
            {
                var distance = Vector3.Dot(vertex, direction);
                if (distance > maxDistance) 
                {
                    maxDistance = distance;
                    maxPoint = vertex;
                }
            }

            return maxPoint * entity.WorldSpaceScale + entity.WorldSpaceLocation;
        }

        public static IReadOnlyList<ConvexShape> GetConcaveCollection(Mesh mesh)
        {
            var faces = new List<ConvexShape>();
            
            var face = new int[3];
            var vertexId = 0;
            foreach (var index in mesh.Indices)
            {
                face[vertexId] = index;

                vertexId++;
                
                if (vertexId <= 2)
                {
                    continue;
                }

                var a = mesh.Vertices[face[0]];
                var b = mesh.Vertices[face[1]];
                var c = mesh.Vertices[face[2]];
                    
                faces.Add(new ConvexShape(a, b, c));
                        
                vertexId = 0;
            }

            return faces;
        }
    }
}