using System;
using System.Collections.Generic;
using Assimp;
using Assimp.Configs;
using OpenTucan.Bridges;
using OpenTucan.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType;

namespace OpenTucan.Graphics
{
    public class Mesh
    {
        public const int DefaultVertexArrayAttribLocation = 0;
        public const int DefaultUVArrayAttribLocation = 1;
        public const int DefaultNormalsArrayAttribLocation = 2;
        
        private readonly int vertexArrayAttribLocation;
        private readonly int uvArrayAttribLocation;
        private readonly int normalsArrayAttribLocation;
        
        private Vector3[] vertices;
        private Vector2[] uv;
        private Vector3[] normals;
        private int[] indices;

        private bool verticesIsDirty;
        private bool uvIsDirty;
        private bool normalsIsDirty;
        private bool indicesIsDirty;

        private Vector3 boundsMinimum;
        private Vector3 boundsMaximum;

        public Mesh(
            int vertexArrayAttribLocation = DefaultVertexArrayAttribLocation,
            int uvArrayAttribLocation = DefaultUVArrayAttribLocation,
            int normalsArrayAttribLocation = DefaultNormalsArrayAttribLocation)
        {
            VertexArrayObject = new VAO();

            this.vertexArrayAttribLocation = vertexArrayAttribLocation;
            this.uvArrayAttribLocation = uvArrayAttribLocation;
            this.normalsArrayAttribLocation = normalsArrayAttribLocation;
        }
        
        public VAO VertexArrayObject { get; }

        public Vector3[] Vertices
        {
            get
            {
                return vertices;
            }
            set
            {
                vertices = value;

                if (!verticesIsDirty)
                {
                    VertexArrayObject.CreateVertexBufferObject(vertexArrayAttribLocation, 3, vertices);
                    verticesIsDirty = true;
                    RecalculateBounds();
                    return;
                }
                
                VertexArrayObject.UpdateVertexBufferObject(vertexArrayAttribLocation, vertices);
                
                RecalculateBounds();
            }
        }
        
        public Vector2[] UV
        {
            get
            {
                return uv;
            }
            set
            {
                uv = value;

                if (!uvIsDirty)
                {
                    VertexArrayObject.CreateVertexBufferObject(uvArrayAttribLocation, 2, uv);
                    uvIsDirty = true;
                    return;
                }
                
                VertexArrayObject.UpdateVertexBufferObject(uvArrayAttribLocation, uv);
            }
        }
        
        public Vector3[] Normals
        {
            get
            {
                return normals;
            }
            set
            {
                normals = value;

                if (!normalsIsDirty)
                {
                    VertexArrayObject.CreateVertexBufferObject(normalsArrayAttribLocation, 3, normals);
                    normalsIsDirty = true;
                    return;
                }
                
                VertexArrayObject.UpdateVertexBufferObject(normalsArrayAttribLocation, normals);
            }
        }
        
        public int[] Indices
        {
            get
            {
                return indices;
            }
            set
            {
                indices = value;

                if (!indicesIsDirty)
                {
                    VertexArrayObject.CreateElementBufferObject(indices);
                    indicesIsDirty = true;
                    return;
                }
                
                VertexArrayObject.UpdateElementBufferObject(indices);
            }
        }

        public Vector3 GetBoundsMinimum()
        {
            return boundsMinimum;
        }
        
        public Vector3 GetBoundsMaximum()
        {
            return boundsMaximum;
        }
        
        public void RecalculateNormals()
        {
            var resultNormals = new Vector3[vertices.Length];
            
            var face = new int[3];
            var vertexId = 0;
            foreach (var index in indices)
            {
                face[vertexId] = index;

                vertexId++;
                if (vertexId > 2)
                {
                    var inx1 = face[0];
                    var inx2 = face[1];
                    var inx3 = face[2];
                    
                    var a = vertices[inx1];
                    var b = vertices[inx2];
                    var c = vertices[inx3];
                    
                    var normal = Vector3.Cross(b - a, c - a).Normalized();

                    resultNormals[inx1] = normal;
                    resultNormals[inx2] = normal;
                    resultNormals[inx3] = normal;
                        
                    vertexId = 0;
                }
            }

            Normals = resultNormals;
        }

        private void RecalculateBounds()
        {
            boundsMinimum.X = float.PositiveInfinity;
            boundsMinimum.Y = float.PositiveInfinity;
            boundsMinimum.Z = float.PositiveInfinity;
            
            boundsMaximum.X = float.NegativeInfinity;
            boundsMaximum.Y = float.NegativeInfinity;
            boundsMaximum.Z = float.NegativeInfinity;

            foreach (var vertex in vertices)
            {
                if (vertex.X < boundsMinimum.X)
                {
                    boundsMinimum.X = vertex.X;
                }
                
                if (vertex.Y < boundsMinimum.Y)
                {
                    boundsMinimum.Y = vertex.Y;
                }
                
                if (vertex.Z < boundsMinimum.Z)
                {
                    boundsMinimum.Z = vertex.Z;
                }
                
                if (vertex.X > boundsMaximum.X)
                {
                    boundsMaximum.X = vertex.X;
                }
                
                if (vertex.Y > boundsMaximum.Y)
                {
                    boundsMaximum.Y = vertex.Y;
                }
                
                if (vertex.Z > boundsMaximum.Z)
                {
                    boundsMaximum.Z = vertex.Z;
                }
            }
        }

        public void PrepareForRendering(CullFaceMode cullFaceMode = CullFaceMode.Back)
        {
            GL.BindVertexArray(VertexArrayObject.Id);
            GL.EnableVertexAttribArray(vertexArrayAttribLocation);
            GL.EnableVertexAttribArray(uvArrayAttribLocation);
            GL.EnableVertexAttribArray(normalsArrayAttribLocation);
            
            GL.CullFace(cullFaceMode);
            GL.DrawElements(PrimitiveType.Triangles, vertices.Length, DrawElementsType.UnsignedInt, indices);

            GL.DisableVertexAttribArray(vertexArrayAttribLocation);
            GL.DisableVertexAttribArray(uvArrayAttribLocation);
            GL.DisableVertexAttribArray(normalsArrayAttribLocation);
            GL.BindVertexArray(0);
        }

        public static Mesh FromFile(string fileName)
        {
            var mesh = new Mesh();
            
            var assimpContext = new AssimpContext();
            assimpContext.SetConfig(new NormalSmoothingAngleConfig(66.0f));
            
            var assimpScene = assimpContext.ImportFile(fileName, PostProcessSteps.FlipUVs);
            var assimpMesh = assimpScene.Meshes[0];
            var assimpMeshFaces = assimpMesh.Faces;

            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var textureCoordinates = new List<Vector2>();
            var indices = new List<int>();

            foreach (var face in assimpMeshFaces)
            {
                for (var faceInx = 0; faceInx < face.IndexCount; faceInx++) 
                {
                    var index = face.Indices[faceInx];
                    var uv = assimpMesh.TextureCoordinateChannels[0][index].ToOpenTK();
                    var origin = assimpMesh.Vertices[index].ToOpenTK();
                    var normal = assimpMesh.Normals[index].ToOpenTK();

                    vertices.Add(origin);
                    normals.Add(normal);
                    textureCoordinates.Add(uv.Xy);
                    indices.Add(index);
                }
            }

            mesh.Vertices = vertices.ToArray();
            mesh.UV = textureCoordinates.ToArray();
            mesh.Normals = normals.ToArray();
            mesh.Indices = indices.ToArray();

            return mesh;
        }
    }
}