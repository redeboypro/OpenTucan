﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using OpenTK;
using OpenTucan.Entities;

namespace OpenTucan.Components
{
    public abstract class Behaviour
    {
        public readonly Entity Entity;
        private bool _enabled;

        protected Behaviour()
        {
            Entity = null;
        }

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