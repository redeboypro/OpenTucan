using System.Collections.Generic;
using OpenTK;
using OpenTucan.Physics;

namespace OpenTucan.Entities
{
    public delegate void CollisionCallback(Rigidbody other, Normal normal);
    
    public class Rigidbody : Entity
    {
        private float fallingSpeed;
        
        private Rigidbody previousContact;
        private bool previousCollide;
        private Normal previousCollisionNormal;
        
        public Rigidbody(List<Rigidbody> physicsWorld, Vector3 min, Vector3 max)
        {
            previousCollide = false;
            previousCollisionNormal = Normal.None;
            
            PhysicsWorld = physicsWorld;
            CollisionShape = new AABB(min, max);
            
            physicsWorld.Add(this);
        }

        public CollisionCallback CollisionEnter { get; set; }
        
        public CollisionCallback CollisionExit { get; set; }

        public AABB CollisionShape { get; }

        public List<Rigidbody> PhysicsWorld { get; set; }

        public float Gravity { get; set; } = -80.0f;
        
        public bool UseGravity { get; set; }
        
        public bool IsKinematic { get; set; }
        
        public void TossUp(float force)
        {
            if (UseGravity)
            {
                fallingSpeed = force;
            }
        }

        public void Transform(float interpolation, params IgnoreParameter[] ignoreParameters)
        {
            CollisionShape.Transform(this, ignoreParameters);
            
            if (IsKinematic)
            {
                return;
            }

            if (UseGravity)
            {
                fallingSpeed += Gravity * interpolation;
                WorldSpaceLocation += new Vector3
                {
                    Y = fallingSpeed * interpolation
                };
            }

            Rigidbody contact = null;
            var collide = false;
            var collisionNormal = Normal.None;
            
            foreach (var rigidbody in PhysicsWorld)
            {
                if (rigidbody == this) 
                {
                    continue;
                }

                if (CollisionShape.Collide(rigidbody.CollisionShape, out var mtv, out var normal))
                {
                    LocalSpaceLocation += mtv;

                    if (normal is Normal.Up)
                    {
                        fallingSpeed = 0.0f;
                    }

                    if (!previousCollide)
                    {
                        CollisionEnter?.Invoke(rigidbody, normal);
                    }

                    contact = rigidbody;
                    collide = true;
                    collisionNormal = normal;
                }
            }
            
            if (!collide && previousCollide)
            {
                CollisionExit?.Invoke(previousContact, previousCollisionNormal);
            }

            previousContact = contact;
            previousCollide = collide;
            previousCollisionNormal = collisionNormal;
        }

        protected override void OnTransformMatrices()
        {
            //Not implemented
        }
    }
}