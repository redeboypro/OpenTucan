using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Assimp;
using OpenTK;
using OpenTucan.Common;
using OpenTucan.Components;
using Quaternion = OpenTK.Quaternion;

namespace OpenTucan.Entities
{
    public enum Space
    {
        Local,
        Global
    }

    public abstract class Entity
    {
        private readonly List<Entity> _children;
        private Entity _parent;

        private Vector3 _globalLocation;
        private Quaternion _globalRotation;
        private Vector3 _globalScale;

        private Vector3 _localLocation;
        private Quaternion _localRotation;
        private Vector3 _localScale;

        private Matrix4 _modelMatrix;

        private bool _isStatic;
        private bool _isActive;

        protected Entity()
        { 
            _globalLocation = _localLocation = Vector3.Zero; 
            _globalRotation = _localRotation = Quaternion.Identity; 
            _globalScale = _localScale = Vector3.One;
            _modelMatrix = Matrix4.Identity;
            _children = new List<Entity>();
            _isActive = true;
            TransformMatrix(Space.Local);
        }
        
        /// <summary>
        /// Entity tag
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// The world space position of the Entity
        /// </summary>
        public Vector3 WorldSpaceLocation
        {
            get
            {
                return _globalLocation;
            }
            set
            {
                _globalLocation = value;
                TransformMatrix(Space.Global);
            }
        }

        /// <summary>
        /// The world space rotation of the Entity
        /// </summary>
        public Quaternion WorldSpaceRotation
        {
            get
            {
                return _globalRotation;
            }
            set
            {
                _globalRotation = value;
                TransformMatrix(Space.Global);
            }
        }

        /// <summary>
        /// The world space euler angles of the Entity
        /// </summary>
        public Vector3 WorldSpaceEulerAngles
        {
            get
            {
                return _globalRotation.ToEulerAngles();
            }
            set
            {
                WorldSpaceRotation = Quaternion.FromEulerAngles(value);
                TransformMatrix(Space.Local);
            }
        }

        /// <summary>
        /// The lossy scale of the Entity
        /// </summary>
        public Vector3 WorldSpaceScale
        {
            get
            {
                return _globalScale;
            }
            set
            {
                _globalScale = value;
                TransformMatrix(Space.Global);
            }
        }

        /// <summary>
        /// The local space position of the Entity
        /// </summary>
        public Vector3 LocalSpaceLocation
        {
            get
            {
                return _localLocation;
            }
            set
            {
                _localLocation = value;
                TransformMatrix(Space.Local);
            }
        }

        /// <summary>
        /// The local space rotation of the Entity
        /// </summary>
        public Quaternion LocalSpaceRotation
        {
            get
            {
                return _localRotation;
            }
            set
            {
                _localRotation = value.Normalized();
                TransformMatrix(Space.Local);
            }
        }

        /// <summary>
        /// The local space euler angles of the Entity
        /// </summary>
        public Vector3 LocalSpaceEulerAngles
        {
            get
            {
                return _localRotation.ToEulerAngles();
            }
            set
            {
                _localRotation = Quaternion.FromEulerAngles(value);
                TransformMatrix(Space.Local);
            }
        }

        /// <summary>
        /// The local space position of the Entity
        /// </summary>
        public Vector3 LocalSpaceScale
        {
            get
            {
                return _localScale;
            }
            set
            {
                _localScale = value;
                TransformMatrix(Space.Local);
            }
        }

        public bool IsActive
        {
            get
            {
                return _isActive;
            }
        }
        
        public bool IsStatic
        {
            get
            {
                return _isStatic;
            }
        }
        
        public Action<bool> ChangeActiveState { get; set; }

        public void SetStatic(bool staticState)
        {
            _isStatic = staticState;
        }
        
        public void SetActive(bool activeState)
        {
            _isActive = activeState;
            ChangeActiveState?.Invoke(activeState);
        }

        /// <summary>
        /// Gives the amount of _children
        /// </summary>
        public int GetChildrenAmount()
        {
            return _children.Count;
        }

        /// <summary>
        /// Gives the child with index
        /// </summary>
        public Entity GetChild(int index)
        {
            return _children[index];
        }
        
        /// <summary>
        /// Assigns an entity as a child
        /// </summary>
        public void AddChild(Entity child)
        {
            if (_children.Contains(child))
            {
                return;
            }

            _children.Add(child);
            child.SetParent(this);
        }

        /// <summary>
        /// Disconnects the child
        /// </summary>
        public void RemoveChild(Entity child)
        {
            if (_children.Contains(child))
            {
                _children.Remove(child);
            }
        }

        /// <summary>
        /// Performs the specified action on each child
        /// </summary>
        public void ChildrenForEach(Action<Entity> forEachEvent)
        {
            _children.ForEach(forEachEvent);
        }

        /// <summary>
        /// Assigns an entity as a _parent
        /// </summary>
        public void SetParent(Entity assignableEntity, bool freezeGlobalParameters = true)
        {
            if (_parent != assignableEntity)
            {
                _parent?.RemoveChild(this);
            }

            var location = _globalLocation;
            var rotation = _globalRotation;
            var scale = _globalScale; 
            
            _parent = assignableEntity;
            TransformMatrix(Space.Local);

            if (freezeGlobalParameters)
            {
                _globalLocation = location;
                _globalRotation = rotation;
                _globalScale = scale;
                TransformMatrix(Space.Global);
            }

            _parent?.AddChild(this);
        }

        /// <summary>
        /// Gives assigned _parent
        /// </summary>
        public Entity GetParent()
        {
            return _parent;
        }

        /// <summary>
        /// Gives current model matrix
        /// </summary>
        public Matrix4 GetModelMatrix()
        {
            return _modelMatrix;
        }

        /// <summary>
        /// Gives _parent model matrix
        /// </summary>
        public Matrix4 GetParentMatrix()
        {
            switch (_parent)
            {
                case null:
                    return Matrix4.Identity;
                case Camera camera:
                    return camera.ViewMatrix.Inverted();
                default:
                    return _parent.GetModelMatrix();
            }
        }

        /// <summary>
        /// Gives forward direction in wish space
        /// </summary>
        public Vector3 Front(Space space)
        {
            switch (space)
            {
                case Space.Global:
                    return WorldSpaceRotation.Front();
                case Space.Local:
                    return LocalSpaceRotation.Front();
                default: throw new Exception("Unknown space");
            }
        }

        /// <summary>
        /// Gives up direction in wish space
        /// </summary>
        public Vector3 Up(Space space)
        {
            switch (space)
            {
                case Space.Global:
                    return WorldSpaceRotation.Up();
                case Space.Local:
                    return LocalSpaceRotation.Up();
                default: throw new Exception("Unknown space");
            }
        }

        /// <summary>
        /// Gives right direction in wish space
        /// </summary>
        public Vector3 Right(Space space)
        {
            switch (space)
            {
                case Space.Global:
                    return WorldSpaceRotation.Right();
                case Space.Local:
                    return LocalSpaceRotation.Right();
                default: throw new Exception("Unknown space");
            }
        }
        
        /// <summary>
        /// Recalculates 
        /// </summary>
        private void TransformMatrix(Space space)
        {
            var parentMatrix = GetParentMatrix();
            
            if (space is Space.Local)
            {
                _modelMatrix = Matrix4.CreateScale(_localScale)
                               * Matrix4.CreateFromQuaternion(_localRotation)
                               * Matrix4.CreateTranslation(_localLocation) * parentMatrix;

                _globalLocation = _modelMatrix.ExtractTranslation();
                _globalRotation = _modelMatrix.ExtractRotation().Normalized();
                _globalScale = _modelMatrix.ExtractScale();
                
                foreach (var child in _children)
                {
                    child.TransformMatrix(Space.Local);
                }
            
                OnTransformMatrices();
                return;
            }
            
            var localMatrix = Matrix4.CreateScale(_globalScale)
                              * Matrix4.CreateFromQuaternion(_globalRotation)
                              * Matrix4.CreateTranslation(_globalLocation) * parentMatrix.Inverted();

            _localLocation = localMatrix.ExtractTranslation();
            _localRotation = localMatrix.ExtractRotation();
            _localRotation.Normalize();
            _localScale = localMatrix.ExtractScale();
            TransformMatrix(Space.Local);
        }

        /// <summary>
        /// Turns to the target
        /// </summary>
        public void LookAt(Vector3 target, Vector3 up)
        {
            if (target.Equals(Vector3.Zero))
            {
                target.Y = float.Epsilon;
            }

            WorldSpaceRotation = MathTools.GetLookRotation((target - WorldSpaceLocation).Normalized(), up);
        }

        /// <summary>
        /// Rotates with quaternion
        /// </summary>
        public void Rotate(Quaternion rotation, Space space = Space.Global)
        {
            switch (space)
            {
                case Space.Global:
                    WorldSpaceRotation = rotation * _globalRotation;
                    break;
                case Space.Local:
                    LocalSpaceRotation = rotation * _localRotation;
                    break;
            }
        }

        /// <summary>
        /// Rotates with angle axis
        /// </summary>
        public void Rotate(float angle, Vector3 axis, Space space = Space.Global)
        {
            Rotate(Quaternion.FromAxisAngle(axis, angle), space);
        }

        protected abstract void OnTransformMatrices();
    }
}