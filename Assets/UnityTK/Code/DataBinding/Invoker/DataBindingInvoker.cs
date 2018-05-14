using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UnityTK.DataBinding
{
    /// <summary>
    /// Special databinding component that can be used to invoke a method on a parent node with arbitrary amount of parameters.
    /// The invoker implements a databinding node behaviour, so it you can bind more nodes / leaves to it.
    /// 
    /// When others bind to it, the bound object is the object retrived by the method invocation.
    /// </summary>
    public class DataBindingInvoker : DataBindingReflectionNode
    {
        /// <summary>
        /// Datastructure that implements the <see cref="IDataBinding"/> interface in a minimalistic way.
        /// Describes a databinding that binds to an object that is used for delegate invocation.
        /// </summary>
        [System.Serializable]
        public class Parameter : IDataBinding
        {
            [ParentComponent(typeof(DataBindingNode), gameObjectOverrideField = "invokerGo")]
            public DataBindingNode parentNode;

            /// <summary>
            /// The gameobject of the invoker this parameter is part of.
            /// </summary>
            [ReadOnlyInspectorAttribute]
            public GameObject invokerGo;

            [ReadOnlyInspectorAttribute]
            public string type;

            [DataBindingField]
            public string field;

            public object boundObject
            {
                get
                {
                    if (Essentials.UnityIsNull(this.parentNode))
                        return null;

                    return this.parentNode.GetFieldValue(this.field);
                }
            }

            public Type boundType
            {
                get
                {
                    if (Essentials.UnityIsNull(this.parentNode))
                        return typeof(object);

                    return this.parentNode.GetFieldType(this.field);
                }
            }

            public DataBinding parent
            {
                get
                {
                    return this.parentNode;
                }
            }

            public Type GetBindTargetType()
            {
                return ReflectionHelper.TypeFromString(this.type);
            }

            public void UpdateBinding()
            {

            }
        }

        [ParentComponent(typeof(DataBindingNode))]
        public DataBindingNode parentNode;

        [DataBindingMethod]
        public string method;

        /// <summary>
        /// The parameters used to invoke the method.
        /// </summary>
        public Parameter[] parameters;

        public override DataBinding parent
        {
            get
            {
                return this.parentNode;
            }
        }


        /// <summary>
        /// Updates the <see cref="parameters"/> array.
        /// Will re-create it if its not created yet or parameter cound mismatches with bound method parameters.
        /// </summary>
        public void OnValidate()
        {
            if (Essentials.UnityIsNull(this.parentNode))
                return;

            // Retrieve method parameters
            List<Type> parameters = this.parentNode.GetMethodSignature(this.method);
            if (this.parameters == null || this.parameters.Length != parameters.Count)
            {
                // Update parameter bindings
                this.parameters = new Parameter[parameters.Count];
                for (int i = 0; i < parameters.Count; i++)
                {
                    this.parameters[i] = new Parameter()
                    {
                        invokerGo = this.gameObject,
                        type = ReflectionHelper.TypeToString(parameters[i])
                    };
                }
            }
        }

        /// <summary>
        /// Sets <see cref="method"/>, can be used for code-driven databinding.
        /// Editor created bindings will have <see cref="method"/> set by the editor.
        /// </summary>
        public void SetMethod(MethodInfo methodInfo)
        {
            if (Essentials.UnityIsNull(this.parentNode) || this.parentNode.boundType != methodInfo.DeclaringType)
                throw new InvalidOperationException("Assigned method to databinding invoker which is declared on another type not bound to by the invoker parent");

            this.method = ReflectionHelper.MethodToString(methodInfo);
            this.OnValidate();
        }

        /// <summary>
        /// This method can be used to set a invoke parameter binding.
        /// This provides an easy way to modify <see cref="parameters"/> data.
        /// </summary>
        public void SetParameterBinding(int parameter, DataBindingNode node, string field)
        {
            this.parameters[parameter].parentNode = node;
            this.parameters[parameter].field = field;
        }

        public void Invoke()
        {
            GetBoundObject();
        }

        protected override void DoUpdateBinding()
        {

        }

        private object[] _tmp;
        protected override object GetBoundObject()
        {
            if (Essentials.UnityIsNull(this.parentNode) || !this.parentNode.hasBoundType)
            {
                Debug.LogError("Tried invoking a databinding invoker with no bound parent node assigned!");
                return null;
            }

            // Update parameters tmp object array
            if (object.ReferenceEquals(this._tmp, null) || this._tmp.Length != this.parameters.Length)
                this._tmp = new object[this.parameters.Length];

            // Write parameters
            for (int i = 0; i < this._tmp.Length; i++)
                this._tmp[i] = this.parameters[i].boundObject;

            return this.parentNode.InvokeMethod(this.method, this._tmp);
        }

        protected override Type GetBoundType()
        {
            if (Essentials.UnityIsNull(this.parentNode))
                return typeof(void);

            return this.parentNode.GetMethodReturnType(this.method);
        }
    }
}