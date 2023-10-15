using System;
using System.Collections.Generic;
using OpenTK;
using OpenTucan.Components;
using OpenTucan.Physics;

namespace OpenTucan.Entities
{
    public class World
    {
        private readonly List<Rigidbody> _rigidbodies;

        public World()
        {
            _rigidbodies = new List<Rigidbody>();
        }

        public void AddRigidbody(Rigidbody rigidbody)
        {
            _rigidbodies.Add(rigidbody);
        }

        public void Start()
        {
            CallFromBehaviourForEach(behaviour =>
            {
                behaviour.Start();
            });
        }
        
        public void Update(FrameEventArgs eventArgs)
        {
            var deltaTime = (float) eventArgs.Time;
            
            for (var index1 = 0; index1 < _rigidbodies.Count; index1++)
            {
                var rigidbody1 = _rigidbodies[index1];

                if (rigidbody1.IsStatic || !rigidbody1.IsActive || rigidbody1.IsKinematic || !rigidbody1.IsTrigger)
                {
                    continue;
                }
                
                rigidbody1.Accelerate(deltaTime);
                rigidbody1.LocalSpaceLocation += new Vector3
                {
                    Y = rigidbody1.FallingVelocity * deltaTime
                };
                
                var triggers = new List<Rigidbody>();
                var responses = new Dictionary<Rigidbody, CollisionInfo>();
                
                for (var index2 = 0; index2 < _rigidbodies.Count; index2++)
                {
                    var rigidbody2 = _rigidbodies[index2];
                    
                    if (index1 == index2 || !rigidbody2.IsActive || rigidbody2.IsKinematic)
                    {
                        continue;
                    }
                    
                    rigidbody1.ResolveCollision(rigidbody2, triggers, responses);
                }
                
                rigidbody1.RefreshTriggers(triggers);
                rigidbody1.RefreshResponses(responses);
            }

            CallFromBehaviourForEach(behaviour =>
            {
                behaviour.Update(eventArgs);
            });
        }
        
        public void Render(FrameEventArgs eventArgs)
        {
            CallFromBehaviourForEach(behaviour =>
            {
                behaviour.Render(eventArgs);
            });
        }

        private void CallFromBehaviourForEach(Action<Behaviour> action)
        {
            foreach (var rigidbody in _rigidbodies)
            {
                if (!rigidbody.IsActive)
                {
                    continue;
                }

                var behaviours = rigidbody.GetBehaviours();
                foreach (var behaviour in behaviours)
                {
                    if (behaviour.Enabled)
                    {
                        action?.Invoke(behaviour);
                    }
                }
            }
        }
    }
}