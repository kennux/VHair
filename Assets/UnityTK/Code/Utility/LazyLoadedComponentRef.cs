using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK
{
    /// <summary>
    /// Lazy loaded unity component reference structure.
    /// 
    /// This structure can be used to cache a reference to a specific component type on a gameobject.
    /// The component will be retrieved from the object if it isnt known yet (lazily loaded).
    /// </summary>
    public struct LazyLoadedComponentRef<T>
    {
        private T obj;
        private Component _checked;

        public T Get(Component comp)
        {
            if (!object.ReferenceEquals(comp, this._checked))
            {
                this.obj = comp.GetComponent<T>();
                this._checked = comp;
            }
            return obj;
        }
    }
}
