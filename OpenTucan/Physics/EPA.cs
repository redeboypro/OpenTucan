using System;
using System.Collections.Generic;
using OpenTK;
using OpenTucan.Entities;

namespace OpenTucan.Physics
{
    public class EPA
    {
        public static float Epsilon { get; set; } = 0.0001f;
        
        public static CollisionInfo GetResponse(ref (Rigidbody rigidbody, ConvexShape shape) a, ref (Rigidbody rigidbody, ConvexShape shape) b, Simplex simplex)
        {
            var polytope = new List<Vector3>(simplex.Begin());
            polytope.AddRange(simplex.End());

            var faces = new List<int>
            {
                0, 1, 2,
                0, 3, 1,
                0, 2, 3,
                1, 3, 2
            };
            
            GetFaceNormals(polytope, faces, out var normals, out var minFace);

            var minNormal = normals[minFace].Direction;
            var minDistance = float.MaxValue;

            var iterations = 0;
            while (Math.Abs(minDistance - float.MaxValue) < float.Epsilon && iterations++ < a.shape.Size + b.shape.Size)
            {
                if (minFace < 0 || minFace >= normals.Count)
                {
                    continue;
                }
                
                var normal = normals[minFace];
                minNormal = normal.Direction;
                minDistance = normal.Distance;

                var support = GJK.Support(ref a, ref b, minNormal);
                var sDistance = Vector3.Dot(minNormal, support);

                if (Math.Abs(sDistance - minDistance) > Epsilon)
                {
                    minDistance = float.MaxValue;
                    var uniqueEdges = new List<(int, int)>();

                    var f = 0;
                    foreach (var currentNormal in normals)
                    {
                        if (GJK.SameDirection(currentNormal.Direction, support))
                        {
                            AddIfUniqueEdge(uniqueEdges, faces, f, f + 1);
                            AddIfUniqueEdge(uniqueEdges, faces, f + 1, f + 2);
                            AddIfUniqueEdge(uniqueEdges, faces, f + 2, f);
                            
                            faces.RemoveAt(f);
                            faces.RemoveAt(f);
                            faces.RemoveAt(f);
                        }
                        else
                        {
                            f += 3;
                        }
                    }

                    var newFaces = new List<int>
                    {
                        Capacity = uniqueEdges.Count * 3
                    };
                    
                    foreach (var (edgeIndex1, edgeIndex2) in uniqueEdges)
                    {
                        newFaces.Add(edgeIndex1);
                        newFaces.Add(edgeIndex2);
                        newFaces.Add(polytope.Count);
                    }
                    polytope.Add(support);

                    faces.AddRange(newFaces);

                    GetFaceNormals(polytope, faces, out normals, out minFace);
                }
            }

            return Math.Abs(minDistance - float.MaxValue) < float.Epsilon ?
                new CollisionInfo(minNormal, 0) :
                new CollisionInfo(minNormal, minDistance + Epsilon);
        }

        private static void GetFaceNormals(
            IReadOnlyList<Vector3> polytope, IReadOnlyList<int> faces, 
            out List<Normal> normals, out int minTriangle)
        {
            normals = new List<Normal>
            {
                Capacity = faces.Count / 3
            };
            minTriangle = 0;
            var minDistance = float.MaxValue;
            
            for (var i = 0; i < faces.Count; i += 3) {
                var a = polytope[faces[i]];
                var b = polytope[faces[i + 1]];
                var c = polytope[faces[i + 2]];

                var normal = Vector3.Cross(b - a, c - a).Normalized();
                var distance = Vector3.Dot(normal, a);

                if (distance < -float.Epsilon) {
                    normal = -normal;
                    distance *= -1;
                }

                normals.Add(new Normal(normal, distance));

                if (distance < minDistance) {
                    minTriangle = i / 3;
                    minDistance = distance;
                }
            }
        }

        private static void AddIfUniqueEdge(
            IList<(int, int)> edges,
            IReadOnlyList<int> faces,
            int a,
            int b)
        {
            var faceA = faces[a];
            var faceB = faces[b];
            for(var i = 0; i < edges.Count; i++)
            {
                if (edges[i] == (faceB, faceA))
                {
                    edges.RemoveAt(i);
                    return;
                }
            }
    
            edges.Add((faceA, faceB));
        }
    }
}