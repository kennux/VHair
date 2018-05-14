using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.DataBinding
{
    /// <summary>
    /// Abstract implementation of an arbitrary databinding.
    /// The binding can be a node (root or branch) or a leaf.
    /// </summary>
    public abstract class DataBinding : MonoBehaviour, IDataBinding
    {
        /// <summary>
        /// <see cref="IDataBinding.boundObject"/>
        /// </summary>
        public object boundObject
        {
            get
            {
                return this.GetBoundObject();
            }
        }

        /// <summary>
        /// <see cref="IDataBinding.boundType"/>
        /// </summary>
        public System.Type boundType
        {
            get
            {
                return this.GetBoundType();
            }
        }

        /// <summary>
        /// <see cref="IDataBinding.parent"/>
        /// </summary>
        public abstract DataBinding parent
        {
            get;
        }

        /// <summary>
        /// Registers this binding to its parent, if there is a parent.
        /// 
        /// When a databinding is not being created in the editor (i.e. its parent isnt being assigned when Awake in Instantiate) you have to call this manually after setting up the binding.
        /// </summary>
        public virtual void Awake()
        {
            var parent = this.parent;
            if (object.ReferenceEquals(parent, null))
                return;

            parent.RegisterChild(this);
        }

        /// <summary>
        /// Returns the type this databinding was bound to.
        /// </summary>
        protected abstract System.Type GetBoundType();

        /// <summary>
        /// Retrives the object this binding is binding to.
        /// </summary>
        protected abstract object GetBoundObject();
        
        /// <summary>
        /// <see cref="IDataBinding.UpdateBinding"/>
        /// </summary>
        public abstract void UpdateBinding();

        /// <summary>
        /// <see cref="IDataBinding.GetBindTargetType"/>
        /// </summary>
        public abstract System.Type GetBindTargetType();

        /// <summary>
        /// Can be overridden to implement child registration.
        /// Default implementation throws <see cref="System.NotImplementedException"/>
        /// </summary>
        protected virtual void RegisterChild(DataBinding child)
        {
            throw new System.NotImplementedException();
        }
    }
}