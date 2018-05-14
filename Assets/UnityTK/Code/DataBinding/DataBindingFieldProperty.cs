using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UnityTK.DataBinding
{
    /// <summary>
    /// Datastructure that can be used to access a reflected field or property in a generalized way.
    /// </summary>
    public class DataBindingFieldProperty
    {
        /// <summary>
        /// Cache that caches field properties for FieldInfos.
        /// </summary>
        private static Cache<FieldInfo, DataBindingFieldProperty> fieldCache = new Cache<FieldInfo, DataBindingFieldProperty>((fi) => new DataBindingFieldProperty(fi));

        /// <summary>
        /// Cache that caches field properties for PropertyInfos.
        /// </summary>
        private static Cache<PropertyInfo, DataBindingFieldProperty> propCache = new Cache<PropertyInfo, DataBindingFieldProperty>((pi) => new DataBindingFieldProperty(pi));

        /// <summary>
        /// Returns a list of cached field properties for the specified type.
        /// </summary>
        /// <param name="t">The type</param>
        public static List<DataBindingFieldProperty> Get(System.Type t, List<DataBindingFieldProperty> preAlloc = null)
        {
            ListPool<DataBindingFieldProperty>.GetIfNull(ref preAlloc);

            // Get fields and props
            var fis = t.GetFields(BindingFlags.Instance | BindingFlags.Public);
            var pis = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            // Write fields
            for (int i = 0; i < fis.Length; i++)
            {
                preAlloc.Add(fieldCache.Get(fis[i]));
            }

            // Write properties
            for (int i = 0; i < pis.Length; i++)
            {
                preAlloc.Add(propCache.Get(pis[i]));
            }

            return preAlloc;
        }

        /// <summary>
        /// Returns a cached field property for the specified field name on the specified type.
        /// </summary>
        public static DataBindingFieldProperty Get(System.Type t, string name)
        {
            var fi = t.GetField(name);
            if (!object.ReferenceEquals(fi, null))
                return fieldCache.Get(fi);

            var pi = t.GetProperty(name);
            if (!object.ReferenceEquals(pi, null))
                return propCache.Get(pi);

            return null;
        }

        /// <summary>
        /// Returns a cached field property for the specified field info.
        /// </summary>
        public static DataBindingFieldProperty Get(FieldInfo fieldInfo)
        {
            return fieldCache.Get(fieldInfo);
        }

        /// <summary>
        /// Returns a cached field property for the specified property info.
        /// </summary>
        public static DataBindingFieldProperty Get(PropertyInfo propertyInfo)
        {
            return propCache.Get(propertyInfo);
        }

        /// <summary>
        /// The type this field property is.
        /// It can either be binding to <see cref="fieldInfo"/> or <see cref="propertyInfo"/>.
        /// </summary>
        private enum FieldPropertyType
        {
            FIELD,
            PROPERTY
        }

        /// <summary>
        /// The name of the field or property.
        /// </summary>
        public string name
        {
            get
            {
                switch (this.type)
                {
                    case FieldPropertyType.FIELD: return this.fieldInfo.Name;
                    case FieldPropertyType.PROPERTY: return this.propertyInfo.Name;
                }
                return null;
            }
        }

        /// <summary>
        /// The type of this field property.
        /// </summary>
        public System.Type fieldType
        {
            get
            {
                switch (this.type)
                {
                    case FieldPropertyType.FIELD: return this.fieldInfo.FieldType;
                    case FieldPropertyType.PROPERTY: return this.propertyInfo.PropertyType;
                }
                return null;
            }
        }

        /// <summary>
        /// The type where this field property was declared on.
        /// </summary>
        public System.Type declaringType
        {
            get
            {
                switch (this.type)
                {
                    case FieldPropertyType.FIELD: return this.fieldInfo.DeclaringType;
                    case FieldPropertyType.PROPERTY: return this.propertyInfo.DeclaringType;
                }
                return null;
            }
        }

        /// <summary>
        /// The field info this object binds to.
        /// </summary>
        private FieldInfo fieldInfo;

        /// <summary>
        /// The property info this object binds to.
        /// </summary>
        private PropertyInfo propertyInfo;

        /// <summary>
        /// The type of this field property type (<see cref="FieldPropertyType"/>).
        /// </summary>
        private FieldPropertyType type;

        private DataBindingFieldProperty(FieldInfo fieldInfo)
        {
            this.fieldInfo = fieldInfo;
            this.propertyInfo = null;
            this.type = FieldPropertyType.FIELD;
        }

        private DataBindingFieldProperty(PropertyInfo propertyInfo)
        {
            this.fieldInfo = null;
            this.propertyInfo = propertyInfo;
            this.type = FieldPropertyType.PROPERTY;
        }

        /// <summary>
        /// Retrives the value of this field property from the specified object.
        /// </summary>
        public object GetValue(object obj)
        {
            switch (this.type)
            {
                case FieldPropertyType.FIELD: return this.fieldInfo.GetValue(obj);
                case FieldPropertyType.PROPERTY: return this.propertyInfo.GetValue(obj, null);
            }

            return null;
        }

        /// <summary>
        /// Sets the specified value on this field property on the specified target object.
        /// </summary>
        public void SetValue(object obj, object value)
        {
            switch (this.type)
            {
                case FieldPropertyType.FIELD: this.fieldInfo.SetValue(obj, value); break;
                case FieldPropertyType.PROPERTY:
                    {
                        if (!this.propertyInfo.CanWrite)
                            throw new System.InvalidOperationException("Cannot set value to read-only property!");

                        this.propertyInfo.SetValue(obj, value, null);
                    }
                    break;
            }
        }
    }
}