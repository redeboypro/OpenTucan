using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenTucan.Components;
using OpenTucan.Entities;

namespace OpenTucan.Physics
{
    public class Rigidbody : Entity
    {
        private IReadOnlyList<ConvexShape> _convexShapes;
        private IReadOnlyList<Rigidbody> _triggerContacts;
        private bool _isKinematic;
        private bool _isTrigger;
        private float _gravity;
        private float _fallingSpeed;

        public Rigidbody()
        {
            _gravity = -80.0f;
            _convexShapes = new ConvexShape[0];
        }
        
        public void TossUp(float force)
        {
            _fallingSpeed = force;
        }

        public void SetConvexShapes(IReadOnlyList<ConvexShape> convexShapes)
        {
            _convexShapes = convexShapes;
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

        public bool IsKinematic
        {
            get
            {
                return _isKinematic;
            }
        }

        public void SetKinematic(bool kinematicState)
        {
            _isKinematic = kinematicState;
        }
        
        public bool IsTrigger
        {
            get
            {
                return _isTrigger;
            }
        }

        public void SetTrigger(bool triggerState)
        {
            _isTrigger = triggerState;
        }

        public Action<CollisionInfo> CollisionResolution { get; set; }
        public Action<Rigidbody> TriggerEnter { get; set; }
        public Action<Rigidbody> TriggerExit { get; set; }

        public Rigidbody Clone()
        {
            var instance = new Rigidbody();
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

            return instance;
        }

        public void ResolveCollision(Rigidbody other, List<Rigidbody> triggers)
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
                        if (other._isTrigger)
                        {
                            triggers.Add(other);
                        }
                        else
                        {
                            var collisionResponse = EPA.GetResponse(ref a, ref b, simplex);
                            LocalSpaceLocation += collisionResponse.Normal * collisionResponse.PenetrationDepth;
                            CollisionResolution?.Invoke(collisionResponse);
                        }
                    }
                }
            }
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

        protected override void OnTransformMatrices()
        {
            //Not implemented
        }
    }
}