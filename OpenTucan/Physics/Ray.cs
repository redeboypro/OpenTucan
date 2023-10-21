using System;
using OpenTK;
using OpenTucan.Common;
using OpenTucan.Entities;
using OpenTucan.Graphics;

namespace OpenTucan.Physics
{
    public readonly struct Ray
    {
        public readonly Vector3 Origin;
        public readonly Vector3 Direction;

        public Ray(Vector3 origin, Vector3 direction)
        {
            Origin = origin;
            Direction = direction;
        }
        
        private bool IntersectPlane(Vector3 planeCenter, Vector3 planeNormal, out Vector3 hitPoint)
        {
            var planeNormalDotRayDirection = Vector3.Dot(planeNormal, Direction);
            hitPoint = Origin + Direction;
            
            if (Math.Abs(planeNormalDotRayDirection) <= float.Epsilon) 
            {
                return false;
            }
            
            var distanceToIntersectionPoint = Vector3.Dot(planeCenter - Origin, planeNormal) / planeNormalDotRayDirection;
            hitPoint = Origin + Direction * distanceToIntersectionPoint;
            
            return distanceToIntersectionPoint >= 0;
        }

        public bool IntersectAABB(Vector3 min, Vector3 max, out Vector3 hitPoint)
        {
            hitPoint = Origin + Direction;
            
            Vector3 fracDirection;
            fracDirection.X = 1.0f / Direction.X;
            fracDirection.Y = 1.0f / Direction.Y;
            fracDirection.Z = 1.0f / Direction.Z;
            
            var distanceToMinX = (min.X - Origin.X) * fracDirection.X;
            var distanceToMaxX = (max.X - Origin.X) * fracDirection.X;
            
            var distanceToMinY = (min.Y - Origin.Y) * fracDirection.Y;
            var distanceToMaxY = (max.Y - Origin.Y) * fracDirection.Y;
            
            var distanceToMinZ = (min.Z - Origin.Z) * fracDirection.Z;
            var distanceToMaxZ = (max.Z - Origin.Z) * fracDirection.Z;

            var distanceToMin = Math.Max(Math.Max(
                    Math.Min(distanceToMinX, distanceToMaxX), 
                    Math.Min(distanceToMinY, distanceToMaxY)), 
                Math.Min(distanceToMinZ, distanceToMaxZ));
            
            var distanceToMax = Math.Min(Math.Min(
                    Math.Max(distanceToMinX, distanceToMaxX),
                    Math.Max(distanceToMinY, distanceToMaxY)),
                Math.Max(distanceToMinZ, distanceToMaxZ));

            if (distanceToMax < 0) 
            {
                return false;
            }
            
            if (distanceToMin > distanceToMax) 
            {
                return false;
            }

            hitPoint = Origin + distanceToMin * Direction;
            return true;
        }

        public bool IntersectTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 normal, out Vector3 hitPoint)
        {
            var normalDotRayDirection = Vector3.Dot(normal, Direction);
            hitPoint = Origin + Direction;
            
            if (Math.Abs(normalDotRayDirection) < float.Epsilon) 
            {
                return false;
            }

            var normalDotPoint = -Vector3.Dot(normal, a);
            var distanceToHitPoint = -(Vector3.Dot(normal, Origin) + normalDotPoint) / normalDotRayDirection;

            if (distanceToHitPoint < 0) 
            {
                return false;
            }

            hitPoint = Origin + distanceToHitPoint * Direction;

            var triangleEdge1 = b - a;
            var rayTriangleDelta1 = hitPoint - a;
            var crossProduct = Vector3.Cross(triangleEdge1, rayTriangleDelta1);
            if (Vector3.Dot(normal, crossProduct) < 0) 
            {
                return false;
            }

            var triangleEdge2 = c - b;
            var rayTriangleDelta2 = hitPoint - b;
            crossProduct = Vector3.Cross(triangleEdge2, rayTriangleDelta2);
            if (Vector3.Dot(normal, crossProduct) < 0) 
            {
                return false;
            }

            var triangleEdge3 = a - c;
            var rayTriangleDelta3 = hitPoint - c;
            crossProduct = Vector3.Cross(triangleEdge3, rayTriangleDelta3);
            return !(Vector3.Dot(normal, crossProduct) < 0);
        }

        public bool IntersectMesh(Mesh mesh, Vector3 location, Quaternion rotation, Vector3 scale, out Vector3 hitPoint)
        {
            hitPoint = Origin + Direction;
            
            var face = new int[3];
            var vertexId = 0;
            
            var intersectAny = false;
            var closestDistance = float.PositiveInfinity;
            
            foreach (var index in mesh.Indices)
            {
                face[vertexId] = index;

                vertexId++;
                
                if (vertexId <= 2)
                {
                    continue;
                }

                var a = mesh.Vertices[face[0]].Transform(location, rotation, scale);
                var b = mesh.Vertices[face[1]].Transform(location, rotation, scale);
                var c = mesh.Vertices[face[2]].Transform(location, rotation, scale);
                var normal = rotation * mesh.Normals[face[0]];

                if (IntersectTriangle(a, b, c, normal, out var triangleHitPoint))
                {
                    intersectAny = true;
                    var distance = Vector3.Distance(triangleHitPoint, hitPoint);
                    if (distance < closestDistance)
                    {
                        hitPoint = triangleHitPoint;
                        closestDistance = distance;
                    }
                }

                vertexId = 0;
            }

            return intersectAny;
        }

        public bool IntersectMesh(Mesh mesh, Entity entity, out Vector3 hitPoint)
        {
            return IntersectMesh(mesh, entity.WorldSpaceLocation, entity.WorldSpaceRotation, entity.WorldSpaceScale, out hitPoint);
        }
    }
}