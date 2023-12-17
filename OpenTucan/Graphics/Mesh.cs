using System;
using System.Collections.Generic;
using System.Linq;
using Assimp;
using Assimp.Configs;
using OpenTucan.Bridges;
using OpenTucan.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTucan.Animations;
using OpenTucan.Physics;
 using PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType;

namespace OpenTucan.Graphics
{
    public class Mesh
    {
        public const int DefaultVertexArrayAttribLocation = 0;
        public const int DefaultUVArrayAttribLocation = 1;
        public const int DefaultNormalArrayAttribLocation = 2;
        public const int DefaultBoneIndexArrayAttribLocation = 3;

        private readonly int _vertexArrayAttribLocation;
        private readonly int _uvArrayAttribLocation;
        private readonly int _normalArrayAttribLocation;
        private readonly int _boneIndexArrayAttribLocation;

        private Vector3[] _vertices;
        private Vector2[] _uv;
        private Vector3[] _normals;
        private int[] _bonesIndices;
        private int[] _indices;

        private bool _verticesBufferIsDirty;
        private bool _uvBufferIsDirty;
        private bool _normalsBufferIsDirty;
        private bool _boneIndicesBufferIsDirty;
        private bool _indicesBufferIsDirty;

        private Vector3 _boundsMinimum;
        private Vector3 _boundsMaximum;

        private IReadOnlyList<ConvexShape> _concaveCollisionCollection;
        private ConvexShape _convexCollisionShape;
        private ConvexShape _boundsCollisionShape;

        public Mesh(
            int vertexArrayAttribLocation = DefaultVertexArrayAttribLocation,
            int uvArrayAttribLocation = DefaultUVArrayAttribLocation,
            int normalArrayAttribLocation = DefaultNormalArrayAttribLocation,
            int boneIndexArrayAttribLocation = DefaultBoneIndexArrayAttribLocation)
        {
            VertexArrayObject = new VAO();

            _vertexArrayAttribLocation = vertexArrayAttribLocation;
            _uvArrayAttribLocation = uvArrayAttribLocation;
            _normalArrayAttribLocation = normalArrayAttribLocation;
            _boneIndexArrayAttribLocation = boneIndexArrayAttribLocation;
        }
        
        public VAO VertexArrayObject { get; }
        public bool IsSkinned { get; private set; }

        public Vector3[] Vertices
        {
            get
            {
                return _vertices;
            }
            set
            {
                _vertices = value;

                if (!_verticesBufferIsDirty)
                {
                    VertexArrayObject.CreateVertexBufferObject(_vertexArrayAttribLocation, 3, _vertices);
                    _verticesBufferIsDirty = true;
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

                if (!_uvBufferIsDirty)
                {
                    VertexArrayObject.CreateVertexBufferObject(_uvArrayAttribLocation, 2, _uv);
                    _uvBufferIsDirty = true;
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

                if (!_normalsBufferIsDirty)
                {
                    VertexArrayObject.CreateVertexBufferObject(_normalArrayAttribLocation, 3, _normals);
                    _normalsBufferIsDirty = true;
                    return;
                }
                
                VertexArrayObject.UpdateVertexBufferObject(_normalArrayAttribLocation, _normals);
            }
        }

        public int[] BoneIndices
        {
            get
            {
                return _bonesIndices;
            }
            set
            {
                _bonesIndices = value;
                IsSkinned = true;

                if (!_boneIndicesBufferIsDirty)
                {
                    VertexArrayObject.CreateVertexBufferObject(_boneIndexArrayAttribLocation, 1, _bonesIndices, VertexAttribPointerType.Int);
                    _boneIndicesBufferIsDirty = true;
                    return;
                }
                
                VertexArrayObject.UpdateVertexBufferObject(_boneIndexArrayAttribLocation, _bonesIndices);
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

                if (!_indicesBufferIsDirty)
                {
                    VertexArrayObject.CreateElementBufferObject(_indices);
                    _indicesBufferIsDirty = true;
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
                if (vertexId <= 2)
                {
                    continue;
                }
                
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
            GL.EnableVertexAttribArray(_normalArrayAttribLocation);

            if (IsSkinned)
            {
                GL.EnableVertexAttribArray(_boneIndexArrayAttribLocation);
            }

            GL.CullFace(cullFaceMode);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);

            GL.DisableVertexAttribArray(_vertexArrayAttribLocation);
            GL.DisableVertexAttribArray(_uvArrayAttribLocation);
            GL.DisableVertexAttribArray(_normalArrayAttribLocation);

            if (IsSkinned)
            {
                GL.DisableVertexAttribArray(_boneIndexArrayAttribLocation);
            }

            GL.BindVertexArray(0);
        }

        public void Rig(AnimationRoot animationRoot)
        {
            BoneIndices = animationRoot.GetJointsIds(_vertices);
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

            for (var i = 0; i < assimpMesh.VertexCount; i++)
            {
                vertices.Add(assimpMesh.Vertices[i].ToOpenTK());
                textureCoordinates.Add(assimpMesh.TextureCoordinateChannels[0][i].ToOpenTK().Xy);
                normals.Add(assimpMesh.Normals[i].ToOpenTK());
            }

            foreach (var face in assimpMeshFaces.Where(face => face.IndexCount == 3))
            {
                indices.Add(face.Indices[0]);
                indices.Add(face.Indices[1]);
                indices.Add(face.Indices[2]);
            }

            mesh.Indices = indices.ToArray();
            mesh.Vertices = vertices.ToArray();
            mesh.UV = textureCoordinates.ToArray();
            mesh.Normals = normals.ToArray();
            
            mesh.RecalculateCollisionShapes();

            return mesh;
        }

        public static Mesh Plane(
            int vertexArrayAttribLocation = DefaultVertexArrayAttribLocation,
            int uvArrayAttribLocation = DefaultUVArrayAttribLocation,
            int normalArrayAttribLocation = DefaultNormalArrayAttribLocation)
        {
            var planeMesh = new Mesh(vertexArrayAttribLocation, uvArrayAttribLocation, normalArrayAttribLocation)
            {
                Indices = new []
                {
                    2, 0, 1, 1, 3, 2
                },
                
                Vertices = new []
                {
                    new Vector3(0.5f, 0.0f, -0.5f),
                    new Vector3(-0.5f, 0.0f, -0.5f),
                    new Vector3(0.5f, 0.0f, 0.5f),
                    new Vector3(-0.5f, 0.0f, 0.5f)
                },

                UV = new []
                {
                    new Vector2(1, 1),
                    new Vector2(0, 1),
                    new Vector2(0, 0),
                    new Vector2(1, 0)
                }
            };

            planeMesh.RecalculateNormals();
            planeMesh.RecalculateCollisionShapes();
            return planeMesh;
        }

        public static Mesh Cube(
            int vertexArrayAttribLocation = DefaultVertexArrayAttribLocation,
            int uvArrayAttribLocation = DefaultUVArrayAttribLocation,
            int normalArrayAttribLocation = DefaultNormalArrayAttribLocation)
        {
            var cubeMesh = new Mesh(vertexArrayAttribLocation, uvArrayAttribLocation, normalArrayAttribLocation)
            {
                Indices = new []
                {
                    0, 1, 2, 0, 2, 3,
                    1, 5, 6, 1, 6, 2,
                    4, 7, 6, 4, 6, 5,
                    0, 3, 7, 0, 7, 4,
                    3, 2, 6, 3, 6, 7,
                    0, 4, 5, 0, 5, 1 
                },
                
                Vertices = new []
                {
                    new Vector3(-0.5f, -0.5f, -0.5f),
                    new Vector3(0.5f, -0.5f, -0.5f),
                    new Vector3(0.5f, 0.5f, -0.5f),
                    new Vector3(-0.5f, 0.5f, -0.5f),
                    new Vector3(-0.5f, -0.5f, 0.5f),
                    new Vector3(0.5f, -0.5f, 0.5f),
                    new Vector3(0.5f, 0.5f, 0.5f),
                    new Vector3(-0.5f, 0.5f, 0.5f)
                },

                UV = new []
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(1, 1),
                    new Vector2(0, 1),
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(1, 1),
                    new Vector2(0, 1)
                }
            };

            cubeMesh.RecalculateNormals();
            cubeMesh.RecalculateCollisionShapes();
            return cubeMesh;
        }
        
        public static Mesh Sphere(
            int stacks, int slices,
            int vertexArrayAttribLocation = DefaultVertexArrayAttribLocation,
            int uvArrayAttribLocation = DefaultUVArrayAttribLocation,
            int normalArrayAttribLocation = DefaultNormalArrayAttribLocation)
        {
            var sphereMesh = new Mesh(vertexArrayAttribLocation, uvArrayAttribLocation, normalArrayAttribLocation);
            var vertexCount = (stacks + 1) * (slices + 1);
            
            var vertices = new Vector3[vertexCount];
            var uv = new Vector2[vertexCount];
            var indices = new int[stacks * slices * 6];

            var vertexIndex = 0;
            var uvIndex = 0;
            for (var stack = 0; stack <= stacks; stack++)
            {
                var stackAngle = stack / (float)stacks * MathHelper.TwoPi;
                var stackSin = (float)Math.Sin(stackAngle);
                var stackCos = (float)Math.Cos(stackAngle);

                for (var slice = 0; slice <= slices; slice++)
                {
                    var sliceAngle = slice / (float)slices * MathHelper.TwoPi;
                    var sliceSin = (float)Math.Sin(sliceAngle);
                    var sliceCos = (float)Math.Cos(sliceAngle);

                    var x = stackSin * sliceSin;
                    var y = stackCos;
                    var z = stackSin * sliceCos;

                    vertices[vertexIndex++] = new Vector3(x, y, z);
                    uv[uvIndex++] = new Vector2(slice / (float)slices, stack / (float)stacks);
                }
            }
            
            var index = 0;
            for (var stack = 0; stack < stacks; stack++)
            {
                for (var slice = 0; slice < slices; slice++)
                {
                    var topLeft = stack * (slices + 1) + slice;
                    var topRight = topLeft + 1;
                    var bottomLeft = ((stack + 1) * (slices + 1)) + slice;
                    var bottomRight = bottomLeft + 1;

                    indices[index++] = topLeft;
                    indices[index++] = bottomLeft;
                    indices[index++] = topRight;
                    indices[index++] = topRight;
                    indices[index++] = bottomLeft;
                    indices[index++] = bottomRight;
                }
            }

            sphereMesh.Vertices = vertices;
            sphereMesh.UV = uv;
            sphereMesh.Indices = indices;
            sphereMesh.RecalculateNormals();
            sphereMesh.RecalculateCollisionShapes();
            return sphereMesh;
        }
    }
}