using System;
using System.Linq;
using OpenTK;
using OpenTucan.Entities;
using OpenTucan.Graphics;

namespace OpenTucan.Physics
{
    public static class ComponentNumber
    {
        public const int X = 1;
        public const int Y = 2;
        public const int Z = 3;
    }
    
    public enum Normal
    {
        Forward = -ComponentNumber.Z,
        Back = ComponentNumber.Z,
        
        Right = -ComponentNumber.X,
        Left = ComponentNumber.X,

        Up = -ComponentNumber.Y,
        Down = ComponentNumber.Y,
        
        None
    }
    
    public enum IgnoreParameter
    {
        FreezeTranslation,
        FreezeRotation,
        FreezeScale
    }
    
    public class AABB
    {
        public const int VertexCount = 8;

        private readonly Vector3[] vertices;
        private readonly Vector3[] sharedVertices;
        
        private Vector3 center;
        private Vector3 min;
        private Vector3 max;
        
        private Vector3 sharedCenter;
        private Vector3 sharedMin;
        private Vector3 sharedMax;

        public AABB(Vector3 min, Vector3 max)
        {
            vertices = new Vector3[VertexCount];
            sharedVertices = new Vector3[VertexCount];
            SetBounds(min, max);
        }
        
        public Vector3[] InitialVertices
        {
            get
            {
                return vertices;
            }
        }
        
        public Vector3[] SharedVertices
        {
            get
            {
                return sharedVertices;
            }
        }

        public Vector3 InitialMinimum
        {
            get
            {
                return min;
            }
        }

        public Vector3 InitialMaximum
        {
            get
            {
                return max;
            }
        }
        
        public Vector3 SharedMinimum
        {
            get
            {
                return sharedMin;
            }
        }

        public Vector3 SharedMaximum
        {
            get
            {
                return sharedMax;
            }
        }

        public Vector3 InitialCenter
        {
            get
            {
                return center;
            }
        }
        
        public Vector3 SharedCenter
        {
            get
            {
                return sharedCenter;
            }
        }

        public void SetBounds(Vector3 newMin, Vector3 newMax)
        {
            min = sharedMin = newMin;
            max = sharedMax = newMax;
            center = sharedCenter = (newMax + newMin) * 0.5f;
            Reset();
        }

        public void AssignFromMesh(Mesh mesh)
        {
            SetBounds(mesh.GetBoundsMinimum(), mesh.GetBoundsMaximum());
        }

        public void Reset()
        {
            vertices[0] = min;
            vertices[1] = new Vector3(min.X, min.Y, max.Z);
            vertices[2] = new Vector3(min.X, max.Y, min.Z);
            vertices[3] = new Vector3(max.X, min.Y, min.Z);
            vertices[4] = new Vector3(min.X, max.Y, max.Z);
            vertices[5] = new Vector3(max.X, min.Y, max.Z);
            vertices[6] = new Vector3(max.X, max.Y, min.Z);
            vertices[7] = max;
        }

        public void Transform(Entity transform, params IgnoreParameter[] ignoreParameters)
        {
            Reset();
            
            var assignsScale = !ignoreParameters.Any(x => x is IgnoreParameter.FreezeScale);
            var assignsRotation = !ignoreParameters.Any(x => x is IgnoreParameter.FreezeRotation);
            var assignsTranslation = !ignoreParameters.Any(x => x is IgnoreParameter.FreezeTranslation);
            
            var entityCenter = transform.WorldSpaceLocation;
            var entityRotation = transform.WorldSpaceRotation;
            var entityScale = transform.WorldSpaceScale;

            sharedMin = Vector3.One * float.PositiveInfinity;
            sharedMax = Vector3.One * float.NegativeInfinity;

            for (var i = 0; i < VertexCount; i++)
            {
                var vertex = vertices[i];
                
                if (assignsScale)
                {
                    vertex *= entityScale;
                }
                
                if (assignsRotation)
                {
                    vertex = entityRotation * vertex;
                }
                
                if (assignsTranslation)
                {
                    vertex += entityCenter;
                }

                sharedVertices[i] = vertex;
            }

            foreach (var modifiedVertex in sharedVertices)
            {
                var resultMin = sharedMin;
                var resultMax = sharedMax;

                for (var axisIndex = 0; axisIndex < 3; axisIndex++)
                {
                    if (modifiedVertex[axisIndex] < resultMin[axisIndex])
                    {
                        resultMin[axisIndex] = modifiedVertex[axisIndex];
                    }

                    if (modifiedVertex[axisIndex] > resultMax[axisIndex])
                    {
                        resultMax[axisIndex] = modifiedVertex[axisIndex];
                    }
                }

                sharedMin = resultMin;
                sharedMax = resultMax;
            }
            
            sharedCenter = (sharedMax + sharedMin) * 0.5f;
        }

        public bool Collide(AABB other, out Vector3 mtv, out Normal normal)
        {
            var otherCenter = other.sharedCenter;
            var distance = otherCenter - sharedCenter;

            mtv = Vector3.Zero;

            distance.X = Math.Abs(distance.X);
            distance.Y = Math.Abs(distance.Y);
            distance.Z = Math.Abs(distance.Z);

            var mHalfExtent = sharedMax - sharedCenter;
            var oHalfExtent = other.sharedMax - otherCenter;
            
            distance -= mHalfExtent + oHalfExtent;
            normal = Normal.None;

            if (distance.X < 0 && distance.Y < 0 && distance.Z < 0)
            {
                var correctionDistance = otherCenter - sharedCenter;

                if (distance.X > distance.Y && distance.X > distance.Z)
                {
                    var directionMultiplier = Math.Sign(correctionDistance.X);
                    mtv.X = distance.X * directionMultiplier;
                    normal = (Normal) (directionMultiplier * ComponentNumber.X);
                }

                if (distance.Y > distance.X && distance.Y > distance.Z)
                {
                    var directionMultiplier = Math.Sign(correctionDistance.Y);
                    mtv.Y = distance.Y * directionMultiplier;
                    normal = (Normal) (directionMultiplier * ComponentNumber.Y);
                }

                if (distance.Z > distance.Y && distance.Z > distance.X)
                {
                    var directionMultiplier = Math.Sign(correctionDistance.Z);
                    mtv.Z = distance.Z * directionMultiplier;
                    normal = (Normal) (directionMultiplier * ComponentNumber.Z);
                }

                return true;
            }

            return false;
        }

        public bool Raycast(Vector3 start, Vector3 direction, out Vector3 hitPoint)
        {
            hitPoint = start + direction;

            Vector3 fracDirection;
            fracDirection.X = 1.0f / direction.X;
            fracDirection.Y = 1.0f / direction.Y;
            fracDirection.Z = 1.0f / direction.Z;

            var distanceToMinX = (sharedMin.X - start.X) * fracDirection.X;
            var distanceToMaxX = (sharedMax.X - start.X) * fracDirection.X;

            var distanceToMinY = (sharedMin.Y - start.Y) * fracDirection.Y;
            var distanceToMaxY = (sharedMax.Y - start.Y) * fracDirection.Y;

            var distanceToMinZ = (sharedMin.Z - start.Z) * fracDirection.Z;
            var distanceToMaxZ = (sharedMax.Z - start.Z) * fracDirection.Z;

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

            hitPoint = start + distanceToMin * direction;
            return true;
        }
    }
}