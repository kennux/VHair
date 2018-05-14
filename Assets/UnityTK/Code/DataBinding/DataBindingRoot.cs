using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.DataBinding
{
    /// <summary>
    /// Implements a databinding root node that can be used to bind to an arbitrary unityengine object.
    /// </summary>
    public class DataBindingRoot : DataBindingReflectionNode
    {
        /// <summary>
        /// The target object this root is binding to.
        /// </summary>
        [Header("Binding")]
        public UnityEngine.Object target;

        /// <summary>
        /// Returns null always, since roots dont have a parent.
        /// </summary>
        public override DataBinding parent
        {
            get { return null; }
        }

        protected override Type GetBoundType()
        {
            if (Essentials.UnityIsNull(this.target))
                return typeof(object);
            return this.target.GetType();
        }

        protected override object GetBoundObject()
        {
            return this.target;
        }

        protected override void DoUpdateBinding()
        {

        }

        public void Update()
        {
            this.UpdateBinding();
        }
    }
}