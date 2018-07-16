using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK
{
    /// <summary>
    /// Singleton MonoBehaviour base class.
    /// Can be used as base-class for any kind of singleton pattern behaviour in unity.
    /// </summary>
    public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        public static T instance
        {
            get
            {
                return UnitySingleton<T>.Get();
            }
        }

        public virtual void Awake()
        {
            UnitySingleton<T>.Register(this as T);
        }
    }
}