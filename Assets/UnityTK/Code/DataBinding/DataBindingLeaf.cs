using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.DataBinding
{
    /// <summary>
    /// Implements abstract data binding leaf behaviour.
    /// 
    /// Leaves can bind to a field on a parent <see cref="DataBindingNode"/> in order to replicate the bound value onto the leaf's target.
    /// </summary>
    public abstract class DataBindingLeaf : DataBinding
    {
        /// <summary>
        /// Invoked when the target this leaf binds to was changed.
        /// This will update the value this leaf binds to and can be used to achieve 2-way bindings.
        /// </summary>
        public abstract void OnChanged();

        /// <summary>
        /// Standard implementation for leafs returns <see cref="DataBinding.GetBindTargetType"/>.
        /// Can be overridden to return more specific type information.
        /// </summary>
        protected override Type GetBoundType()
        {
            return this.GetBindTargetType();
        }
    }
}