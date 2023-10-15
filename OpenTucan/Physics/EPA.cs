using System;
using System.Collections.Generic;
using OpenTK;
using OpenTucan.Entities;

namespace OpenTucan.Physics
{
    public class EPA
    {
        public static float Epsilon { get; set; } = 0.001f;
        
        public static CollisionInfo GetResponse(ref (Rigidbody rigidbody, ConvexShape shape) a, ref (Rigidbody rigidbody, ConvexShape shape) b, Simplex simplex)
        {
            var polytope = new List<Vector3>(simplex.Begin());
            polytope.AddRange(simplex.End());

            var faces = new List<int>()
            {
                0, 1, 2,
                0, 3, 1,
                0, 2, 3,
                1, 3, 2
            };
            
            GetFaceNormals(polytope, faces, out var normals, out var minFace);

            var minNormal = Vector3.Zero;
            var minDistance = float.MaxValue;

            while (Math.Abs(minDistance - float.MaxValue) < float.Epsilon)
            {
                var normal = normals[minFace];
                minNormal = normal.Direction;
                minDistance = normal.Distance;

                var support = GJK.Support(ref a, ref b, minNormal);
                var sDistance = Vector3.Dot(minNormal, support);

                if (Math.Abs(sDistance - minDistance) > 0.001f)
                {
                    minDistance = float.MaxValue;
                    var uniqueEdges = new List<(int, int)>();

                    for (var i = 0; i < normals.Count; i++)
                    {
                        if (GJK.SameDirection(normals[i].Direction, support))
                        {
                            var f = i * 3;

                            AddIfUniqueEdge(uniqueEdges, faces, f, f + 1);
                            AddIfUniqueEdge(uniqueEdges, faces, f + 1, f + 2);
                            AddIfUniqueEdge(uniqueEdges, faces, f + 2, f);

                            var lastFace = faces.Count - 1;

                            faces[f + 2] = faces[lastFace]; 
                            faces.RemoveAt(lastFace);
                            
                            faces[f + 1] = faces[lastFace]; 
                            faces.RemoveAt(lastFace);
                            
                            faces[f] = faces[lastFace]; 
                            faces.RemoveAt(lastFace);

                            normals[i] = normals[lastFace];
                            normals.RemoveAt(normals.Count - 1);

                            i--;
                        }
                    }
                    
                    var newFaces = new List<int>();
                    foreach (var (edgePointA, edgePointB) in uniqueEdges)
                    {
                        newFaces.Add(edgePointA);
                        newFaces.Add(edgePointB);
                        newFaces.Add(polytope.Count);
                    }
                    
                    polytope.Add(support);

                    GetFaceNormals(polytope, newFaces, out var newNormals, out var newMinFace);
                    
                    var oldMinDistance = float.MaxValue;
                    for (var i = 0; i < normals.Count; i++)
                    {
                        if (normals[i].Distance < oldMinDistance)
                        {
                            oldMinDistance = normals[i].Distance;
                            minFace = i;
                        }
                    }

                    if (newNormals[newMinFace].Distance < oldMinDistance)
                    {
                        minFace = newMinFace + normals.Count;
                    }

                    faces.AddRange(newFaces);
                    normals.AddRange(newNormals);
                }
            }
            
            return new CollisionInfo(minNormal, minDistance + Epsilon);
        }

        private static void GetFaceNormals(
            IReadOnlyList<Vector3> polytope, List<int> faces, 
            out List<Normal> normals, out int minTriangle)
        {
            normals = new List<Normal>();
            minTriangle = 0;
            var minDistance = float.MaxValue;
            
            for (var i = 0; i < faces.Count; i += 3) {
                var a = polytope[faces[i]];
                var b = polytope[faces[i + 1]];
                var c = polytope[faces[i + 2]];

                var normal = Vector3.Cross(b - a, c - a).Normalized();
                var distance = Vector3.Dot(normal, a);

                if (distance < 0) {
                    normal *= -1;
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