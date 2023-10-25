using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using OpenTK;
using OpenTucan.Entities;
using OpenTucan.Physics;

namespace OpenTucan.Components
{
    public abstract class Behaviour
    {
        public GameObject GameObject;
        private bool _enabled = true;

        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;

                if (Enabled)
                {
                    OnEnable();
                    return;
                }
                
                OnDisable();
            }
        }

        public World World
        {
            get
            {
                return GameObject.World;
            }
        }

        public virtual void Start() { }
        
        public virtual void Update(FrameEventArgs eventArgs) { }
        
        public virtual void Render(FrameEventArgs eventArgs) { }

        public virtual void OnEnable() { }
        
        public virtual void OnDisable() { }

        public static IEnumerable<Type> GetTypesFromAssembly(Assembly assembly)
        {
            var inTypes = assembly.GetTypes();
            return inTypes.Where(type => type.BaseType.IsAssignableFrom(typeof(Behaviour)));
        }
    }
}