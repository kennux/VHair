using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.BehaviourModel
{
    /// <summary>
    /// Base class for implementing mechanic components.
    /// </summary>
    [RequireComponent(typeof(BehaviourModel))]
    public abstract class BehaviourModelMechanicComponent<T> : BehaviourModelComponent where T : BehaviourModelMechanic
    {
        public T mechanic
        {
            get { return this._mechanic.Get(this); }
        }
        private LazyLoadedComponentRef<T> _mechanic = new LazyLoadedComponentRef<T>();

        /// <summary>
        /// Called in order to bind to all handlers of the <see cref="mechanic"/>.
        /// Called from <see cref="Awake"/>
        /// </summary>
        protected abstract void BindHandlers();

        protected virtual void Awake()
        {
            this.BindHandlers();
        }
    }
}