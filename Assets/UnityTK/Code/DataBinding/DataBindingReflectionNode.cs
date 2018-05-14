using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace UnityTK.DataBinding
{
    /// <summary>
    /// Base class for <see cref="DataBindingNode"/>s which bind to an arbitrary object (<see cref="DataBinding.boundObject"/>) and provide its fields and properties via reflections as binding fields.
    /// </summary>
    public abstract class DataBindingReflectionNode : DataBindingNode
    {
        /// <summary>
        /// Cache mapping <see cref="GetFieldValue(string)"/> to their field property.
        /// </summary>
        protected Cache<string, DataBindingFieldProperty> fieldCache
        {
            get
            {
                if (object.ReferenceEquals(this._fieldCache, null))
                    this._fieldCache = new Cache<string, DataBindingFieldProperty>(CacheConstructor);

                return this._fieldCache;
            }
        }
        private Cache<string, DataBindingFieldProperty> _fieldCache;

        public override Type GetBindTargetType()
        {
            return typeof(object);
        }

        /// <summary>
        /// Cache element constructor for <see cref="fieldCache"/>
        /// </summary>
        private DataBindingFieldProperty CacheConstructor(string field)
        {
            return DataBindingFieldProperty.Get(this.boundObject.GetType(), field);
        }

        public override void SetFieldValue(string field, object value)
        {
            object boundObject = this.boundObject; // Invoke getter once
            if (object.ReferenceEquals(boundObject, null))
                throw new NullReferenceException("Bound object is null, cannot set a field of null!");

            // Get field prop & set
            DataBindingFieldProperty fieldProperty = DataBindingFieldProperty.Get(boundObject.GetType(), field);
            fieldProperty.SetValue(boundObject, value);
        }

        public override List<string> GetFields(System.Type type = null, List<string> preAlloc = null)
        {
            ListPool<string>.GetIfNull(ref preAlloc);

            // No target set? If so, return empty list
            if (!this.hasBoundType)
                return preAlloc;

            // Special field - "This"
            if (object.ReferenceEquals(type, null) || type.IsAssignableFrom(this.boundType))
                preAlloc.Add("This");

            // Write fields
            var props = DataBindingFieldProperty.Get(this.boundType);
            for (int i = 0; i < props.Count; i++)
            {
                if (object.ReferenceEquals(type, null) || type.IsAssignableFrom(props[i].fieldType))
                    preAlloc.Add(props[i].name);
            }

            return preAlloc;
        }

        public override Type GetFieldType(string field)
        {
            // Get bound type
            var type = this.boundType;
            if (type == null)
                return typeof(object);

            // Get field prop
            var fieldProp = DataBindingFieldProperty.Get(type, field);
            if (fieldProp == null)
                return typeof(object);

            return fieldProp.fieldType;
        }

        public override object GetFieldValue(string field)
        {
            if (!this.hasBoundObject || string.IsNullOrEmpty(field))
                return null;

            // Special field - "This"
            if (field.Equals("This"))
                return this.boundObject;

            return this.fieldCache.Get(field).GetValue(this.boundObject);
        }

        public override List<Type> GetMethodSignature(string method, List<Type> preAlloc = null)
        {
            ListPool<Type>.GetIfNull(ref preAlloc);

            if (!this.hasBoundType)
                return preAlloc;

            // Get method info and read parameters
            var methodInfo = ReflectionHelper.MethodFromString(method, this.boundType);
            var p = methodInfo.GetParameters();

            // Copy parameters in list
            for (int i = 0; i < p.Length; i++)
                preAlloc.Add(p[i].ParameterType);

            return preAlloc;
        }

        public override List<string> GetMethods(Type[] signature = null, Type returnType = null, List<string> preAlloc = null)
        {
            ListPool<string>.GetIfNull(ref preAlloc);

            if (!this.hasBoundType)
                return preAlloc;

            // TODO: Filtering!
            var methods = this.boundType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < methods.Length; i++)
                preAlloc.Add(ReflectionHelper.MethodToString(methods[i]));

            return preAlloc;
        }

        /// <summary>
        /// Returns the method return type for the specified method.
        /// Returns typeof(void) if no method was found or type is unbound.
        /// </summary>
        public override Type GetMethodReturnType(string method)
        {
            if (!this.hasBoundType)
                return typeof(void);

            var _method = ReflectionHelper.MethodFromString(method, this.boundType);
            if (object.ReferenceEquals(_method, null))
                return typeof(void);

            return _method.ReturnParameter.ParameterType;
        }

        public override object InvokeMethod(string method, object[] parameters)
        {
            if (!this.hasBoundType)
            {
                Debug.LogError("Tried executing method of a reflection node which isnt bound!", this.gameObject);
                return null;
            }

            var _method = ReflectionHelper.MethodFromString(method, this.boundType);
            if (object.ReferenceEquals(_method, null))
            {
                Debug.LogError("Tried executing method of a reflection node which could not be found!", this.gameObject);
                return null;
            }

            return _method.Invoke(this.boundObject, parameters);
        }
    }
}
