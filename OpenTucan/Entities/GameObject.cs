using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTucan.Animations;
using OpenTucan.Common;
using OpenTucan.Components;
using OpenTucan.Graphics;
using OpenTucan.Physics;

namespace OpenTucan.Entities
{
    public class GameObject : Rigidbody
    {
        public readonly string Name;
        public readonly World World;
        
        private readonly List<Behaviour> _behaviours;

        private Mesh _mesh;
        private Texture _texture;
        private Shader _shader;
        
        private AnimationRoot _animationRoot;
        private IReadOnlyList<Matrix4> _bonesMatrices;

        public GameObject(string name, World world)
        {
            Name = world.GetAvailableName(name);
            World = world;
            _behaviours = new List<Behaviour>();
        }

        public Mesh Mesh
        {
            get
            {
                return _mesh;
            }
        }
        
        public Texture Texture
        {
            get
            {
                return _texture;
            }
        }
        
        public Shader Shader
        {
            get
            {
                return _shader;
            }
        }

        public AnimationRoot AnimationRoot
        {
            get
            {
                return _animationRoot;
            }
        }

        public bool ReadyForRendering(Camera camera)
        {
            if (_mesh is null || camera is null || _shader is null)
            {
                return false;
            }
            
            _shader.Start();

            var isSkinned = _mesh.IsSkinned && _animationRoot != null;
            _shader.SetUniform(ShaderConsts.HasBones, isSkinned);
            if (isSkinned)
            {
                for (var boneIndex = 0; boneIndex < _bonesMatrices.Count; boneIndex++)
                {
                    _shader.SetUniform(ShaderConsts.BonesMatrices + "[" + boneIndex + "]", _bonesMatrices[boneIndex]);
                }
            }
            _shader.SetUniform(ShaderConsts.ViewMatrix, camera.ViewMatrix);
            _shader.SetUniform(ShaderConsts.ProjectionMatrix, camera.ProjectionMatrix);
            _shader.SetUniform(ShaderConsts.ModelMatrix, GetGlobalMatrix());

            if (_texture != null)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                _texture.Bind();
            }
            
            return true;
        }
        
        public GameObject Clone()
        {
            var instance = new GameObject(Name, World)
            {
                _mesh = _mesh,
                _texture = _texture,
                _shader = _shader,
                Tag = Tag
            };

            instance.SetParent(GetParent());
            instance.WorldSpaceLocation = WorldSpaceLocation;
            instance.WorldSpaceRotation = WorldSpaceRotation;
            instance.WorldSpaceScale = WorldSpaceScale;

            var behaviours = GetBehaviours();

            foreach (var behaviour in behaviours)
            {
                var type = behaviour.GetType();
                var behaviourInstance = (Behaviour) Activator.CreateInstance(type);
                var fields = type.GetFields(BindingFlags.Instance);
                foreach (var field in fields)
                {
                    field.SetValue(behaviourInstance, field.GetValue(behaviour));
                }
                instance.AddBehaviour(behaviourInstance);
            }

            for (var i = 0; i < GetChildrenAmount(); i++)
            {
                var child = (GameObject) GetChild(i);
                var childPosition = child.WorldSpaceLocation;
                var childRotation = child.WorldSpaceRotation;
                var childScale = child.WorldSpaceScale;
                
                var childClone = child.Clone();
                childClone.SetParent(instance);
                child.WorldSpaceLocation = childPosition;
                child.WorldSpaceRotation = childRotation;
                child.WorldSpaceScale = childScale;
            }

            World.Instantiate(instance);
            return instance;
        }
        
        public IReadOnlyList<Behaviour> GetBehaviours()
        {
            return _behaviours;
        }
        
        public T GetBehaviour<T>() where T : Behaviour
        {
            return (T) _behaviours.FirstOrDefault(behaviour => behaviour.GetType().IsAssignableFrom(typeof(T)));
        }
        
        public Behaviour GetBehaviour(Type behaviourType)
        {
            if (!behaviourType.BaseType.IsAssignableFrom(typeof(Behaviour)))
            {
                throw new Exception("Input behaviour base type should be assignable from \"OpenTucan.Components.Behaviour\"");
            }

            return _behaviours.FirstOrDefault(behaviour => behaviour.GetType().IsAssignableFrom(behaviourType));
        }

        public void AddBehaviour<T>() where T : Behaviour
        {
            AddBehaviour(typeof(T));
        }
        
        public void AddBehaviour(Type behaviourType)
        {
            if (!behaviourType.BaseType.IsAssignableFrom(typeof(Behaviour)))
            {
                throw new Exception("Input behaviour base type should be assignable from \"OpenTucan.Components.Behaviour\"");
            }
            
            var behaviour = (Behaviour) Activator.CreateInstance(behaviourType);
            AddBehaviour(behaviour);
        }
        
        public void AddBehaviour(Behaviour behaviour)
        {
            var holder = typeof(Behaviour).GetField(nameof(GameObject), BindingFlags.Instance | BindingFlags.Public);
            holder.SetValue(behaviour, this);
            _behaviours.Add(behaviour);
        }

        public void SetMesh(Mesh mesh)
        {
            _mesh = mesh;
        }

        public void SetAnimationRoot(AnimationRoot animationRoot)
        {
            _animationRoot = animationRoot;
            var bonesMatrices = new Matrix4[_animationRoot.WeightPointCount];
            for (var i = 0; i < bonesMatrices.Length; i++)
            {
                bonesMatrices[i] = Matrix4.Identity;
            }

            _bonesMatrices = bonesMatrices;
        }

        public void SetBonesMatrices(IReadOnlyList<Matrix4> bonesMatrices)
        {
            _bonesMatrices = bonesMatrices;
        }

        public void InterpolateAnimation(string clipName, float time)
        {
            _bonesMatrices = _animationRoot.Interpolate(clipName, time);
        }

        public void SetTexture(Texture texture)
        {
            _texture = texture;
        }
        
        public void SetShader(Shader shader)
        {
            _shader = shader;
        }
    }
}