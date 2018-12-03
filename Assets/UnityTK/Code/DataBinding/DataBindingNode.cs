using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.DataBinding
{
    /// <summary>
    /// Abstract implementation of a databinding node.
    /// 
    /// A node is a databinding that exists in the unity scene graph.
    /// Unlike leaves, the nodes provide an array of fields children can bind to.
    /// Each of these fields is identified by a unique string identifier.
    /// </summary>
    public abstract class DataBindingNode : DataBinding
    {
        /// <summary>
        /// All children that bind to this databinding.
        /// </summary>
        protected List<DataBinding> children = new List<DataBinding>();

        /// <summary>
        /// <see cref="DataBinding.boundObject"/> object equality to null check.
        /// </summary>
        public bool hasBoundObject { get { return !Essentials.UnityIsNull(this.boundObject); } }

        /// <summary>
        /// <see cref="DataBinding.boundType"/> object equality to null check.
        /// </summary>
        public bool hasBoundType { get { return !object.ReferenceEquals(this.boundType, null); } }

        /// <summary>
        /// Called in order to register a children of this binding.
        /// Called in <see cref="Awake"/>
        /// </summary>
        /// <param name="child">The children that should be registered</param>
        protected override void RegisterChild(DataBinding child)
        {
            if (this.children.Contains(child))
                return;

            this.children.Add(child);
        }

        /// <summary>
        /// Updates this databinding and all its children.
        /// Usually called when the bound object changed and a binding update is required.
        /// </summary>
        public override void UpdateBinding()
        {
            // Update self
            this.DoUpdateBinding();

            // Update children
            for (int i = 0; i < this.children.Count; i++)
                this.children[i].UpdateBinding();
        }

        /// <summary>
        /// Called from <see cref="UpdateBinding"/> in order to update this node.
        /// </summary>
        protected abstract void DoUpdateBinding();

        /// <summary>
        /// Returns all fields this databinding node can provide.
        /// </summary>
        /// <param name="type">The type the fields must be assignable to. You can pass in null to get all fields (since everything is assignable to object).</param>
        /// <param name="preAlloc">Pre-allocated string list can be passed in to avoid memory allocations. If this is null, a new list is created.</param>
        /// <returns>preAlloc filled with all fields this node can provide.</returns>
        public abstract List<string> GetFields(System.Type type = null, List<string> preAlloc = null);

        /// <summary>
        /// Determines the type of the specified field.
        /// </summary>
        /// <param name="field">The field name retrieved by <see cref="GetFields(System.Type, List{string})"/></param>
        /// <returns>The reference to the type of the specified field.</returns>
        public abstract System.Type GetFieldType(string field);

        /// <summary>
        /// Returns the value of the specified field.
        /// The field string refers to a field string returned by <see cref="GetFields(List{string})"/>
        /// </summary>
        /// <param name="field">The field whose value should be read.</param>
        /// <returns>The field value.</returns>
        public abstract object GetFieldValue(string field);

        /// <summary>
        /// Sets the value of the specified field.
        /// In case the field cannot write a value, an <see cref="System.InvalidOperationException"/> is thrown.
        /// </summary>
        public abstract void SetFieldValue(string field, object value);

        /// <summary>
        /// Returns all methods this node can provide.
        /// </summary>
        /// <param name="signature">The signature to filter for, if null any signature is assumed to be acceptable and added to the return list.</param>
        /// <param name="returnType">The return type to filter for, if null any return type is assumed to be acceptable.</param>
        /// <param name="preAlloc">Pre-allocated string list can be passed in to avoid memory allocations. If this is null, a new list is created.</param>
        /// <returns>List of unique strings mapping to a method provided by this node. The strings map to the method parameter of <see cref="InvokeMethod(string, object[])"/></returns>
        public abstract List<string> GetMethods(System.Type[] signature = null, System.Type returnType = null, List<string> preAlloc = null);

        /// <summary>
        /// Executes the specified method with the specified parameters and returns the result.
        /// </summary>
        /// <param name="method">The method identifier of the method to invoke. Maps to a string returned from <see cref="GetMethods(Type[], Type, List{string})"/>.</param>
        /// <param name="parameters">The parameters passed into the method invoke call. You can pass null in to save object array creation if its a parameterless method signature.</param>
        /// <returns>The object the method returned.</returns>
        public abstract object InvokeMethod(string method, object[] parameters);

        /// <summary>
        /// Returns the signature of the specified method.
        /// </summary>
        public abstract List<System.Type> GetMethodSignature(string method, List<System.Type> preAlloc = null);

        /// <summary>
        /// Returns the return type of the specified method.
        /// </summary>
        public abstract System.Type GetMethodReturnType(string method);

        /// <summary>
        /// Custom string conversion for databinding nodes.
        /// </summary>
        /// <returns>A string with format {0} - {1} with 0 = base.ToString(), 1 = this.boundType.ToString()</returns>
        public override string ToString()
        {
            return string.Format("{0} - {1}", base.ToString(), this.boundType.Name);
        }
    }
}