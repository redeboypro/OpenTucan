using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTucan.Components;
using OpenTucan.Graphics;
using OpenTucan.Physics;

namespace OpenTucan.Entities
{
    public class World
    {
        private readonly List<GameObject> _gameObjects;

        public World()
        {
            _gameObjects = new List<GameObject>();
        }

        public string GetAvailableName(string name)
        {
            if (!FindObjectWithName(name, out _))
            {
                return name;
            }

            var i = 0;
            while (FindObjectWithName($"{name} ({i})", out _))
            {
                i++;
            }

            return $"{name} ({i})";
        }

        public bool FindObjectWithName(string name, out GameObject toFind)
        {
            foreach (var obj in _gameObjects.Where(obj => obj.Name == name))
            {
                toFind = obj;
                return true;
            }

            toFind = null;
            return false;
        }

        public void Instantiate(GameObject gameObject)
        {
            _gameObjects.Add(gameObject);
        }
        
        public GameObject Instantiate(string name)
        {
            var instance = new GameObject(name, this);
            _gameObjects.Add(instance);
            return instance;
        }
        
        public GameObject Instantiate(string name, Mesh mesh, Texture texture, Shader shader)
        {
            var instance = Instantiate(name);
            instance.SetMesh(mesh);
            return instance;
        }

        public void Start()
        {
            foreach (var obj in _gameObjects.Where(obj => obj.IsActive))
            {
                CallFromBehaviour(obj, behaviour =>
                {
                    behaviour.Start();
                });
            }
        }
        
        public void Update(FrameEventArgs eventArgs)
        {
            var deltaTime = (float) eventArgs.Time;
            
            for (var index1 = 0; index1 < _gameObjects.Count; index1++)
            {
                var rigidbody1 = _gameObjects[index1];

                if (rigidbody1.IsStatic || !rigidbody1.IsActive || rigidbody1.IsKinematic || !rigidbody1.IsTrigger)
                {
                    continue;
                }
                
                CallFromBehaviour(rigidbody1, behaviour =>
                {
                    behaviour.Update(eventArgs);
                });

                rigidbody1.Accelerate(deltaTime);
                rigidbody1.LocalSpaceLocation += new Vector3
                {
                    Y = rigidbody1.FallingVelocity * deltaTime
                };
                
                var triggers = new List<Rigidbody>();
                var responses = new Dictionary<Rigidbody, CollisionInfo>();
                
                for (var index2 = 0; index2 < _gameObjects.Count; index2++)
                {
                    var rigidbody2 = _gameObjects[index2];
                    
                    if (index1 == index2 || !rigidbody2.IsActive || rigidbody2.IsKinematic)
                    {
                        continue;
                    }
                    
                    rigidbody1.ResolveCollision(rigidbody2, triggers, responses);
                }
                
                rigidbody1.RefreshTriggers(triggers);
                rigidbody1.RefreshResponses(responses);
            }
        }
        
        public void Render(FrameEventArgs eventArgs)
        {
            foreach (var obj in _gameObjects.Where(obj => obj.IsActive))
            {
                obj.Shader.Start();
                
                CallFromBehaviour(obj, behaviour =>
                {
                    behaviour.Render(eventArgs);
                });
                
                if (!obj.ReadyForRendering(Camera.Main))
                {
                    continue;
                }
                
                obj.Mesh.Draw();
                obj.Shader.Stop();
            }
        }

        private static void CallFromBehaviour(GameObject obj, Action<Behaviour> action)
        {
            var behaviours = obj.GetBehaviours();
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