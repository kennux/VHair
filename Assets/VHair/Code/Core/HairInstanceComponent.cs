using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK;

namespace VHair
{
    [RequireComponent(typeof(HairInstance))]
    public abstract class HairInstanceComponent<T> : MonoBehaviour where T : HairInstance
    {
        public T instance
        {
            get
            {
                return this._instance.Get(this);
            }
        }
        private LazyLoadedComponentRef<T> _instance = new LazyLoadedComponentRef<T>();
    }
}