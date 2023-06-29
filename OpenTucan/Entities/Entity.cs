using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Assimp;
using OpenTK;
using OpenTucan.Common;
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
        private readonly List<Entity> children;
        private Entity parent;

        private Vector3 globalLocation = Vector3.Zero;
        private Quaternion globalRotation = Quaternion.Identity;
        private Vector3 globalScale = Vector3.One;

        private Vector3 localLocation = Vector3.Zero;
        private Quaternion localRotation = Quaternion.Identity;
        private Vector3 localScale = Vector3.One;

        private Matrix4 modelMatrix = Matrix4.Identity;
        
        protected Entity()
        {
            children = new List<Entity>();
            TransformMatrices(Space.Local);
        }

        /// <summary>
        /// The world space position of the Entity
        /// </summary>
        public Vector3 WorldSpaceLocation
        {
            get
            {
                return globalLocation;
            }
            set
            {
                globalLocation = value;
                TransformMatrices(Space.Global);
            }
        }

        /// <summary>
        /// The world space rotation of the Entity
        /// </summary>
        public Quaternion WorldSpaceRotation
        {
            get
            {
                return globalRotation;
            }
            set
            {
                globalRotation = value;
                TransformMatrices(Space.Global);
            }
        }

        /// <summary>
        /// The world space euler angles of the Entity
        /// </summary>
        public Vector3 WorldSpaceEulerAngles
        {
            get
            {
                return globalRotation.ToEulerAngles();
            }
            set
            {
                WorldSpaceRotation = Quaternion.FromEulerAngles(value);
                TransformMatrices(Space.Local);
            }
        }

        /// <summary>
        /// The lossy scale of the Entity
        /// </summary>
        public Vector3 WorldSpaceScale
        {
            get
            {
                return globalScale;
            }
            set
            {
                globalScale = value;
                TransformMatrices(Space.Global);
            }
        }

        /// <summary>
        /// The local space position of the Entity
        /// </summary>
        public Vector3 LocalSpaceLocation
        {
            get
            {
                return localLocation;
            }
            set
            {
                localLocation = value;
                TransformMatrices(Space.Local);
            }
        }

        /// <summary>
        /// The local space rotation of the Entity
        /// </summary>
        public Quaternion LocalSpaceRotation
        {
            get
            {
                return localRotation;
            }
            set
            {
                localRotation = value.Normalized();
                TransformMatrices(Space.Local);
            }
        }

        /// <summary>
        /// The local space euler angles of the Entity
        /// </summary>
        public Vector3 LocalSpaceEulerAngles
        {
            get
            {
                return localRotation.ToEulerAngles();
            }
            set
            {
                localRotation = Quaternion.FromEulerAngles(value);
                TransformMatrices(Space.Local);
            }
        }

        /// <summary>
        /// The local space position of the Entity
        /// </summary>
        public Vector3 LocalSpaceScale
        {
            get
            {
                return localScale;
            }
            set
            {
                localScale = value;
                TransformMatrices(Space.Local);
            }
        }

        /// <summary>
        /// Gives the amount of children
        /// </summary>
        public int GetChildrenAmount()
        {
            return children.Count;
        }

        /// <summary>
        /// Gives the child with index
        /// </summary>
        public Entity GetChild(int index)
        {
            return children[index];
        }
        
        /// <summary>
        /// Assigns an entity as a child
        /// </summary>
        public void AddChild(Entity child)
        {
            if (children.Contains(child))
            {
                return;
            }

            children.Add(child);
            child.SetParent(this);
        }

        /// <summary>
        /// Disconnects the child
        /// </summary>
        public void RemoveChild(Entity child)
        {
            if (children.Contains(child))
            {
                children.Remove(child);
            }
        }

        /// <summary>
        /// Assigns an entity as a parent
        /// </summary>
        public void SetParent(Entity assignableEntity, bool freezeComponents = true)
        {
            if (parent != assignableEntity)
            {
                parent?.RemoveChild(this);
            }

            var location = globalLocation;
            var rotation = globalRotation;
            var scale = globalScale; 
            
            parent = assignableEntity;
            TransformMatrices(Space.Local);

            if (!freezeComponents)
            {
                globalLocation = location;
                globalRotation = rotation;
                globalScale = scale;
                TransformMatrices(Space.Global);
            }

            parent?.AddChild(this);
        }

        /// <summary>
        /// Gives assigned parent
        /// </summary>
        public Entity GetParent()
        {
            return parent;
        }

        /// <summary>
        /// Gives current model matrix
        /// </summary>
        public Matrix4 GetModelMatrix()
        {
            return modelMatrix;
        }

        /// <summary>
        /// Gives parent model matrix
        /// </summary>
        public Matrix4 GetParentMatrix()
        {
            switch (parent)
            {
                case null:
                    return Matrix4.Identity;
                case Camera camera:
                    return camera.ViewMatrix.Inverted();
                default:
                    return parent.GetModelMatrix();
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
        
        private void TransformMatrices(Space space)
        {
            var parentMatrix = GetParentMatrix();
            
            switch (space)
            {
                case Space.Local:
                    modelMatrix = Matrix4.CreateScale(localScale)
                                  * Matrix4.CreateFromQuaternion(localRotation)
                                  * Matrix4.CreateTranslation(localLocation) * parentMatrix;

                    globalLocation = modelMatrix.ExtractTranslation();
                    globalRotation = modelMatrix.ExtractRotation().Normalized();
                    globalScale = modelMatrix.ExtractScale();
                    break;
                case Space.Global:
                    var localMatrix = Matrix4.CreateScale(globalScale)
                                      * Matrix4.CreateFromQuaternion(globalRotation)
                                      * Matrix4.CreateTranslation(globalLocation) * parentMatrix.Inverted();

                    localLocation = localMatrix.ExtractTranslation();
                    localRotation = localMatrix.ExtractRotation();
                    localRotation.Normalize();
                    localScale = localMatrix.ExtractScale();
                    TransformMatrices(Space.Local);
                    break;
            }

            foreach (var child in children)
            {
                child.TransformMatrices(Space.Local);
            }
            
            OnTransformMatrices();
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
                    WorldSpaceRotation = rotation * globalRotation;
                    break;
                case Space.Local:
                    LocalSpaceRotation = rotation * localRotation;
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