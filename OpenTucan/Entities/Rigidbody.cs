using System.Collections.Generic;
using OpenTK;
using OpenTucan.Physics;

namespace OpenTucan.Entities
{
    public delegate void CollisionCallback(Rigidbody other, Normal normal);
    
    public class Rigidbody : Entity
    {
        private readonly ICollection<Rigidbody> _rigidbodies;
        private float _fallingSpeed;
        
        private Rigidbody _previousContact;
        private bool _previousCollide;
        private Normal _previousCollisionNormal;

        public Rigidbody(ICollection<Rigidbody> rigidbodies, Vector3 min, Vector3 max)
        {
            _previousCollide = false;
            _previousCollisionNormal = Normal.None;
            _rigidbodies = rigidbodies;
            
            CollisionShape = new AABB(min, max);
            
            rigidbodies.Add(this);
        }

        public CollisionCallback CollisionEnter { get; set; }
        
        public CollisionCallback CollisionExit { get; set; }

        public AABB CollisionShape { get; }

        public float Gravity { get; set; } = -80.0f;
        
        public bool UseGravity { get; set; }
        
        public bool IsKinematic { get; set; }
        
        public void TossUp(float force)
        {
            if (UseGravity)
            {
                _fallingSpeed = force;
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
                _fallingSpeed += Gravity * interpolation;
                WorldSpaceLocation += new Vector3
                {
                    Y = _fallingSpeed * interpolation
                };
            }

            Rigidbody contact = null;
            var collide = false;
            var collisionNormal = Normal.None;
            
            foreach (var rigidbody in _rigidbodies)
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
                        _fallingSpeed = 0.0f;
                    }

                    if (!_previousCollide)
                    {
                        CollisionEnter?.Invoke(rigidbody, normal);
                    }

                    contact = rigidbody;
                    collide = true;
                    collisionNormal = normal;
                }
            }
            
            if (!collide && _previousCollide)
            {
                CollisionExit?.Invoke(_previousContact, _previousCollisionNormal);
            }

            _previousContact = contact;
            _previousCollide = collide;
            _previousCollisionNormal = collisionNormal;
        }

        protected override void OnTransformMatrices()
        {
            //Not implemented
        }
    }
}