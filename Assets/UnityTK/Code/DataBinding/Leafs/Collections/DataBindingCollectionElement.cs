using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityTK.DataBinding
{
    /// <summary>
    /// Represents a single element in a <see cref="DataBindingCollectionLeaf"/> binding.
    /// </summary>
    public class DataBindingCollectionElement : DataBindingReflectionNode
    {
        public override DataBinding parent
        {
            get { return null; }
        }

        /// <summary>
        /// The target object.
        /// </summary>
        private object target;

        [SerializeField]
        [ReadOnlyInspectorAttribute]
        private string targetType;

        /// <summary>
        /// Called in order to set the element type of the collection this element is assigned to.
        /// The collection leaf will call this in the editor in order to make its prefab aware of the type that is being bound to!
        /// </summary>
        /// <param name="type">The type this element will bind to.</param>
        public void SetElementType(System.Type type)
        {
            this.targetType = type.AssemblyQualifiedName;
        }

        /// <summary>
        /// Sets <see cref="target"/>, which is the object returned by this node as <see cref="DataBinding.boundObject"/>.
        /// </summary>
        public void SetTargetObject(object obj)
        {
            this.target = obj;
            this.UpdateBinding();
        }

        protected override void DoUpdateBinding()
        {

        }

        protected override object GetBoundObject()
        {
            return this.target;
        }

        protected override Type GetBoundType()
        {
            // If no type set we just assume the primitive object type.
            if (string.IsNullOrEmpty(this.targetType))
                return typeof(object);

            // Get type
            return ReflectionHelper.TypeFromString(this.targetType);
        }
    }
}
