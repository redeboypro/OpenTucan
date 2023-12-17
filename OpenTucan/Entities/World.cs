using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTucan.Components;
using OpenTucan.Graphics;
using OpenTucan.Physics;

namespace OpenTucan.Entities
{
    public class World
    {
        public const string DefaultLayer = "Default";
        
        private readonly IDictionary<string, List<GameObject>> _layers;
        private ICollection<string> _layersOrder;

        public World()
        {
            _layers = new Dictionary<string, List<GameObject>>()
            {
                {DefaultLayer, new List<GameObject>()}
            };
            
            _layersOrder = new List<string>
            {
                DefaultLayer
            };
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
            foreach (var layer in _layers)
            {
                foreach (var obj in layer.Value.Where(obj => obj.Name == name))
                {
                    toFind = obj;
                    return true;
                }
            }

            toFind = null;
            return false;
        }

        public void Instantiate(GameObject gameObject, string layer = DefaultLayer)
        {
            if (!_layers.ContainsKey(layer))
            {
                _layers.Add(layer, new List<GameObject>());
                _layersOrder.Add(layer);
            }
            
            _layers[layer].Add(gameObject);
        }
        
        public GameObject Instantiate(string name, string layer = DefaultLayer)
        {
            var instance = new GameObject(name, this);
            Instantiate(instance, layer);
            return instance;
        }
        
        public GameObject Instantiate(string name, Mesh mesh, Texture texture, Shader shader, string layer = DefaultLayer)
        {
            var instance = Instantiate(name, layer);
            instance.SetMesh(mesh);
            instance.SetTexture(texture);
            instance.SetShader(shader);
            return instance;
        }

        public void SetLayersOrder(ICollection<string> layersOrder)
        {
            _layersOrder = layersOrder;
        }

        public void Start()
        {
            foreach (var layer in _layers)
            {
                foreach (var obj in layer.Value.Where(obj => obj.IsActive))
                {
                    CallFromBehaviour(obj, behaviour => { behaviour.Start(); });
                }
            }
        }
        
        public void Update(FrameEventArgs eventArgs)
        {
            var deltaTime = (float) eventArgs.Time;

            foreach (var firstLayer in _layers)
            {
                var firstLayerObjects = firstLayer.Value;
                for (var index1 = 0; index1 < firstLayerObjects.Count; index1++)
                {
                    var rigidbody1 = firstLayerObjects[index1];

                    if (!rigidbody1.IsActive)
                    {
                        continue;
                    }

                    CallFromBehaviour(rigidbody1, behaviour => { behaviour.Update(eventArgs); });

                    if (rigidbody1.IsStatic || rigidbody1.IsKinematic || rigidbody1.IsTrigger)
                    {
                        continue;
                    }

                    rigidbody1.Accelerate(deltaTime);
                    rigidbody1.LocalSpaceLocation += new Vector3
                    {
                        Y = rigidbody1.FallingVelocity * deltaTime
                    };

                    var triggers = new List<Rigidbody>();
                    var responses = new List<(Rigidbody, CollisionInfo)>();

                    foreach (var secondLayer in _layers)
                    {
                        var secondLayerObjects = secondLayer.Value;
                        for (var index2 = 0; index2 < secondLayerObjects.Count; index2++)
                        {
                            var rigidbody2 = secondLayerObjects[index2];

                            if (index1 == index2 || !rigidbody2.IsActive || rigidbody2.IsKinematic ||
                                rigidbody1.IgnoreCollision(rigidbody2.Tag) ||
                                rigidbody2.IgnoreCollision(rigidbody1.Tag))
                            {
                                continue;
                            }

                            rigidbody1.ResolveCollision(rigidbody2, triggers, responses);
                        }
                    }

                    rigidbody1.RefreshTriggers(triggers);
                    rigidbody1.RefreshResponses(responses);
                }
            }
        }
        
        public void Render(FrameEventArgs eventArgs)
        {
            foreach (var layer in _layersOrder)
            {
                var layerObjects = _layers[layer];
                GL.Clear(ClearBufferMask.DepthBufferBit);
                foreach (var obj in layerObjects.Where(obj => obj.IsActive))
                {
                    CallFromBehaviour(obj, behaviour => { behaviour.Render(eventArgs); });

                    if (!obj.ReadyForRendering(Camera.Main))
                    {
                        continue;
                    }

                    obj.Mesh.Draw();
                    obj.Shader.Stop();
                }
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