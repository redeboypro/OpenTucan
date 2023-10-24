using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTucan.Entities;

namespace OpenTucan.Physics
{
    public class Rigidbody : Entity
    {
        private IReadOnlyList<ConvexShape> _convexShapes;
        private IReadOnlyList<Rigidbody> _triggerContacts;
        private IReadOnlyList<string> _ignoreCollisionTags;
        private float _flatAngle;

        public Rigidbody()
        {
            FlatAngle = 30f;
            FallingAcceleration = -80.0f;
            IsKinematic = true;
            Responses = new List<(Rigidbody, CollisionInfo)>();
            _convexShapes = new ConvexShape[0];
            _triggerContacts = new Rigidbody[0];
            _ignoreCollisionTags = new string[0];
        }
        
        public ConvexShape this[int index]
        {
            get
            {
                return _convexShapes[index];
            }
        }

        public int Size
        {
            get
            {
                return _convexShapes.Count;
            }
        }
        
        public float FlatAngle
        {
            get
            {
                return _flatAngle * 90f;
            }
            set
            {
                _flatAngle = value / 90f;
            }
        }

        public bool IsKinematic { get; private set; }

        public bool IsTrigger { get; private set; }
        
        public List<(Rigidbody rigidbody, CollisionInfo collisionInfo)> Responses { get; private set; }

        public float FallingVelocity { get; private set; }

        public float FallingAcceleration { get; set; }
        
        public bool IsGrounded { get; private set; }
        
        public Action<Rigidbody, CollisionInfo, float> CollisionEnter { get; set; }
        
        public Action<Rigidbody, CollisionInfo> CollisionExit { get; set; }
        
        public Action<Rigidbody> TriggerEnter { get; set; }
        
        public Action<Rigidbody> TriggerExit { get; set; }

        public bool IgnoreCollision(string tag)
        {
            return _ignoreCollisionTags.Contains(tag);
        }

        public void TossUp(float force)
        {
            FallingVelocity = force;
        }

        public void SetConvexShapes(IReadOnlyList<ConvexShape> convexShapes)
        {
            _convexShapes = convexShapes;
        }
        
        public void SetConvexShapes(params ConvexShape[] convexShapes)
        {
            _convexShapes = convexShapes;
        }

        public void SetKinematic(bool kinematicState)
        {
            IsKinematic = kinematicState;
        }

        public void SetTrigger(bool triggerState)
        {
            IsTrigger = triggerState;
        }

        public void Accelerate(float deltaTime)
        {
            FallingVelocity += FallingAcceleration * deltaTime;
        }
        
        public void SetIgnoreCollisionTags(params string[] tags)
        {
            _ignoreCollisionTags = tags;
        }
        
        public void SetIgnoreCollisionTags(IReadOnlyList<string> tags)
        {
            _ignoreCollisionTags = tags;
        }

        public void ResolveCollision(Rigidbody other, List<Rigidbody> triggers, List<(Rigidbody, CollisionInfo)> responses)
        {
            foreach (var shape1 in _convexShapes)
            {
                if (shape1 is null)
                {
                    continue;
                }
                
                foreach (var shape2 in other._convexShapes)
                {
                    if (shape2 is null)
                    {
                        continue;
                    }
                    
                    var a = (other, shape2);
                    var b = (this, shape1);
                    
                    if (GJK.Intersects(ref a, ref b, out var simplex))
                    {
                        if (other.IsTrigger)
                        {
                            triggers.Add(other);
                        }
                        else
                        {
                            var collisionInfo = EPA.GetResponse(ref a, ref b, simplex);
                            LocalSpaceLocation += collisionInfo.Normal * collisionInfo.PenetrationDepth;
                            CollisionEnter?.Invoke(other, collisionInfo, FallingVelocity);

                            if (PlaneIsFlat(collisionInfo.Normal))
                            {
                                FallingVelocity = 0f;
                                IsGrounded = true;
                            }
                            
                            responses.Add((other, collisionInfo));
                        }
                    }
                }
            }
        }

        private bool PlaneIsFlat(Vector3 normal)
        {
            return normal.Y >= _flatAngle;
        }
        
        public void RefreshTriggers(IReadOnlyList<Rigidbody> others)
        {
            foreach (var elder in _triggerContacts)
            {
                if (!others.Contains(elder))
                {
                    TriggerExit?.Invoke(elder);
                }
            }

            foreach (var newcomer in others)
            {
                if (!_triggerContacts.Contains(newcomer))
                {
                    TriggerEnter?.Invoke(newcomer);
                }
            }

            _triggerContacts = others;
        }
        
        public void RefreshResponses(List<(Rigidbody rigidbody, CollisionInfo collisionInfo)> responses)
        {
            foreach (var response in Responses)
            {
                var rigidbody = response.rigidbody;
                if (responses.All(localResponse => localResponse.rigidbody != rigidbody))
                {
                    CollisionExit?.Invoke(rigidbody, response.collisionInfo);
                }
            }
            
            var isGrounded = false;
            foreach (var response in responses)
            {
                var collisionInfo = response.collisionInfo;
                if (PlaneIsFlat(collisionInfo.Normal))
                {
                    isGrounded = true;
                }
            }

            IsGrounded = isGrounded;
            Responses = responses;
        }

        protected override void OnTransformMatrices()
        {
            //Not implemented
        }
    }
}