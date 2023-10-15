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
        
        public ConvexShape(Mesh mesh)
        {
            var vertices = mesh.Vertices;
            var indices = mesh.Indices;
            _vertices = indices.Select(index => vertices[index]).ToList();
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

            return entity.WorldSpaceRotation * (maxPoint * entity.WorldSpaceScale) + entity.WorldSpaceLocation;
        }
    }
}