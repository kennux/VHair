using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnityTK.DataBinding
{
    /// <summary>
    /// Generic data binding leaf (<see cref="DataBindingGenericTemplatedLeaf"/>) template asset implementation.
    /// Bind templates can be used to define the target the leaf will be binding to.
    /// 
    /// For example a generic bind template that defines it binds to a unity ui label's text field.
    /// The generic field then only needs this template and its target binding field assigned.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(menuName = "UnityTK/DataBindings/Generic Leaf Template", fileName = "GenericLeafTemplate")]
    public class DataBindingGenericTemplate : ScriptableObject
    {
        /// <summary>
        /// The target type, the string passed in to <see cref="Type.GetType(string)"/>
        /// </summary>
        [SerializeField]
        private string targetType;

        /// <summary>
        /// The field / property name on <see cref="targetType"/> that this template will bind to.
        /// </summary>
        [SerializeField]
        private string targetField;

        /// <summary>
        /// Cached type reference.
        /// </summary>
        private Type _type;

        /// <summary>
        /// Cache type field property.
        /// </summary>
        private DataBindingFieldProperty _field;

        /// <summary>
        /// Set in <see cref="OnValidate"/> to reflect input data validity.
        /// </summary>
#pragma warning disable 414
        [SerializeField]
        [ReadOnlyInspectorAttribute]
        private bool _isValid;

        [ContextMenu("Validate")]
        public void OnValidate()
        {
            this._type = null;
            this._field = null;

            this._isValid = !object.ReferenceEquals(this.GetTargetType(), null) && !object.ReferenceEquals(this.GetField(), null);
        }
#pragma warning restore 414

        /// <summary>
        /// Sets the target type of this template (<see cref="targetType"/>).
        /// This setter can be used to set the type via code rather than the unity inspector.
        /// </summary>
        public void SetTargetType(Type type)
        {
            if (type == null)
                this.targetType = "";
            else
                this.targetType = ReflectionHelper.GetSimpleAssemblyQualifiedName(type);
        }

        public void SetTargetField(PropertyInfo propertyInfo) { SetTargetField(DataBindingFieldProperty.Get(propertyInfo)); }
        public void SetTargetField(FieldInfo fieldInfo) { SetTargetField(DataBindingFieldProperty.Get(fieldInfo)); }
        /// <summary>
        /// Sets the target field of this template (<see cref="targetField"/>).
        /// This setter can be used to set the field via code rather than the unity inspector.
        /// </summary>
        public void SetTargetField(DataBindingFieldProperty fieldProperty)
        {
            if (fieldProperty.declaringType != this.GetTargetType())
                throw new InvalidOperationException("Cannot set target field of a generic templated leaf to a field property that was not declared on the template target.");

            this.targetField = fieldProperty.name;
        }

        /// <summary>
        /// Returns the target type this template will bind to.
        /// </summary>
        public Type GetTargetType()
        {
            if (object.ReferenceEquals(this._type, null))
            {
                this._type = Type.GetType(this.targetType);

                if (object.ReferenceEquals(this._type, null))
                    Debug.LogError("Couldnt find type " + this.targetType);
            }

            return this._type;
        }

        /// <summary>
        /// Returns the field this template binds to.
        /// </summary>
        public DataBindingFieldProperty GetField()
        {
            if (object.ReferenceEquals(this._field, null))
            {
                var type = GetTargetType();
                this._field = DataBindingFieldProperty.Get(type, this.targetField);

                if (object.ReferenceEquals(this._field, null))
                    Debug.LogError("Couldnt find field " + this.targetField + " on type " + this.targetType + " ( " + type + " )");
            }

            return this._field;
        }
        
        /// <summary>
        /// Returns the type of the field this template binds to.
        /// </summary>
        public System.Type GetFieldType()
        {
            return GetField().fieldType;
        }
    }
}
