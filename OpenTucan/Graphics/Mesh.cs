﻿using System;
using System.Collections.Generic;
using Assimp;
using Assimp.Configs;
using OpenTucan.Bridges;
using OpenTucan.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
 using OpenTucan.Physics;
 using PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType;

namespace OpenTucan.Graphics
{
    public class Mesh
    {
        public const int DefaultVertexArrayAttribLocation = 0;
        public const int DefaultUVArrayAttribLocation = 1;
        public const int DefaultNormalsArrayAttribLocation = 2;
        
        private readonly int _vertexArrayAttribLocation;
        private readonly int _uvArrayAttribLocation;
        private readonly int _normalsArrayAttribLocation;
        
        private Vector3[] _vertices;
        private Vector2[] _uv;
        private Vector3[] _normals;
        private int[] _indices;

        private bool _verticesIsDirty;
        private bool _uvIsDirty;
        private bool _normalsIsDirty;
        private bool _indicesIsDirty;

        private Vector3 _boundsMinimum;
        private Vector3 _boundsMaximum;

        private IReadOnlyList<ConvexShape> _concaveCollisionCollection;
        private ConvexShape _convexCollisionShape;
        private ConvexShape _boundsCollisionShape;

        public Mesh(
            int vertexArrayAttribLocation = DefaultVertexArrayAttribLocation,
            int uvArrayAttribLocation = DefaultUVArrayAttribLocation,
            int normalsArrayAttribLocation = DefaultNormalsArrayAttribLocation)
        {
            VertexArrayObject = new VAO();

            _vertexArrayAttribLocation = vertexArrayAttribLocation;
            _uvArrayAttribLocation = uvArrayAttribLocation;
            _normalsArrayAttribLocation = normalsArrayAttribLocation;
        }
        
        public VAO VertexArrayObject { get; }

        public Vector3[] Vertices
        {
            get
            {
                return _vertices;
            }
            set
            {
                _vertices = value;

                if (!_verticesIsDirty)
                {
                    VertexArrayObject.CreateVertexBufferObject(_vertexArrayAttribLocation, 3, _vertices);
                    _verticesIsDirty = true;
                    RecalculateBounds();
                    return;
                }
                
                VertexArrayObject.UpdateVertexBufferObject(_vertexArrayAttribLocation, _vertices);
                RecalculateBounds();
            }
        }
        
        public Vector2[] UV
        {
            get
            {
                return _uv;
            }
            set
            {
                _uv = value;

                if (!_uvIsDirty)
                {
                    VertexArrayObject.CreateVertexBufferObject(_uvArrayAttribLocation, 2, _uv);
                    _uvIsDirty = true;
                    return;
                }
                
                VertexArrayObject.UpdateVertexBufferObject(_uvArrayAttribLocation, _uv);
            }
        }
        
        public Vector3[] Normals
        {
            get
            {
                return _normals;
            }
            set
            {
                _normals = value;

                if (!_normalsIsDirty)
                {
                    VertexArrayObject.CreateVertexBufferObject(_normalsArrayAttribLocation, 3, _normals);
                    _normalsIsDirty = true;
                    return;
                }
                
                VertexArrayObject.UpdateVertexBufferObject(_normalsArrayAttribLocation, _normals);
            }
        }
        
        public int[] Indices
        {
            get
            {
                return _indices;
            }
            set
            {
                _indices = value;

                if (!_indicesIsDirty)
                {
                    VertexArrayObject.CreateElementBufferObject(_indices);
                    _indicesIsDirty = true;
                    return;
                }
                
                VertexArrayObject.UpdateElementBufferObject(_indices);
            }
        }

        public IReadOnlyList<ConvexShape> ConcaveCollisionCollection
        {
            get
            {
                return _concaveCollisionCollection;
            }
        }
        
        public ConvexShape ConvexCollisionShape
        {
            get
            {
                return _convexCollisionShape;
            }
        }
        
        public ConvexShape BoundsCollisionShape
        {
            get
            {
                return _boundsCollisionShape;
            }
        }

        public Vector3 GetBoundsMinimum()
        {
            return _boundsMinimum;
        }
        
        public Vector3 GetBoundsMaximum()
        {
            return _boundsMaximum;
        }
        
        public void RecalculateNormals()
        {
            var resultNormals = new Vector3[_vertices.Length];
            
            var face = new int[3];
            var vertexId = 0;
            foreach (var index in _indices)
            {
                face[vertexId] = index;

                vertexId++;
                if (vertexId > 2)
                {
                    var inx1 = face[0];
                    var inx2 = face[1];
                    var inx3 = face[2];
                    
                    var a = _vertices[inx1];
                    var b = _vertices[inx2];
                    var c = _vertices[inx3];
                    
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
            _boundsMinimum.X = float.PositiveInfinity;
            _boundsMinimum.Y = float.PositiveInfinity;
            _boundsMinimum.Z = float.PositiveInfinity;
            
            _boundsMaximum.X = float.NegativeInfinity;
            _boundsMaximum.Y = float.NegativeInfinity;
            _boundsMaximum.Z = float.NegativeInfinity;

            foreach (var vertex in _vertices)
            {
                if (vertex.X < _boundsMinimum.X)
                {
                    _boundsMinimum.X = vertex.X;
                }
                
                if (vertex.Y < _boundsMinimum.Y)
                {
                    _boundsMinimum.Y = vertex.Y;
                }
                
                if (vertex.Z < _boundsMinimum.Z)
                {
                    _boundsMinimum.Z = vertex.Z;
                }
                
                if (vertex.X > _boundsMaximum.X)
                {
                    _boundsMaximum.X = vertex.X;
                }
                
                if (vertex.Y > _boundsMaximum.Y)
                {
                    _boundsMaximum.Y = vertex.Y;
                }
                
                if (vertex.Z > _boundsMaximum.Z)
                {
                    _boundsMaximum.Z = vertex.Z;
                }
            }
        }
        
        public void RecalculateCollisionShapes()
        {
            _concaveCollisionCollection = ConvexShape.GetConcaveCollection(this);
            _convexCollisionShape = new ConvexShape(this);
            _boundsCollisionShape = new ConvexShape(GetBoundsMinimum(), GetBoundsMaximum());
        }

        public void Draw(CullFaceMode cullFaceMode = CullFaceMode.Back)
        {
            GL.BindVertexArray(VertexArrayObject.Id);
            GL.EnableVertexAttribArray(_vertexArrayAttribLocation);
            GL.EnableVertexAttribArray(_uvArrayAttribLocation);
            GL.EnableVertexAttribArray(_normalsArrayAttribLocation);
            
            GL.CullFace(cullFaceMode);
            GL.DrawElements(PrimitiveType.Triangles, _vertices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);

            GL.DisableVertexAttribArray(_vertexArrayAttribLocation);
            GL.DisableVertexAttribArray(_uvArrayAttribLocation);
            GL.DisableVertexAttribArray(_normalsArrayAttribLocation);
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

            mesh.Indices = indices.ToArray();
            mesh.Vertices = vertices.ToArray();
            mesh.UV = textureCoordinates.ToArray();
            mesh.Normals = normals.ToArray();
            
            mesh.RecalculateCollisionShapes();

            return mesh;
        }

        public static Mesh Cube(
            int vertexArrayAttribLocation = DefaultVertexArrayAttribLocation,
            int uvArrayAttribLocation = DefaultUVArrayAttribLocation,
            int normalsArrayAttribLocation = DefaultNormalsArrayAttribLocation)
        {
            var cubeMesh = new Mesh(vertexArrayAttribLocation, uvArrayAttribLocation, normalsArrayAttribLocation)
            {
                Indices = new[]
                {
                    0, 1, 2, 3, 0, 2,
                    1, 5, 6, 2, 1, 6,
                    4, 5, 1, 0, 4, 1,
                    3, 2, 6, 7, 3, 6,
                    6, 5, 4, 6, 4, 7,
                    4, 0, 3, 7, 4, 3
                },
                
                Vertices = new[]
                {
                    new Vector3(-1.0f, -1.0f, -1.0f),
                    new Vector3(-1.0f, 1.0f, -1.0f),
                    new Vector3(1.0f, 1.0f, -1.0f),
                    new Vector3(1.0f, -1.0f, -1.0f),

                    new Vector3(-1.0f, -1.0f, 1.0f),
                    new Vector3(-1.0f, 1.0f, 1.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    new Vector3(1.0f, -1.0f, 1.0f)
                },

                UV = new[]
                {
                    new Vector2(0.0f, 0.0f), 
                    new Vector2(0.0f, 1.0f), 
                    new Vector2(1.0f, 1.0f),
                    new Vector2(1.0f, 0.0f),

                    new Vector2(1.0f, 0.0f),
                    new Vector2(1.0f, 1.0f),
                    new Vector2(0.0f, 1.0f),
                    new Vector2(0.0f, 0.0f)
                }
            };

            cubeMesh.RecalculateNormals();
            cubeMesh.RecalculateCollisionShapes();
            return cubeMesh;
        }
    }
}