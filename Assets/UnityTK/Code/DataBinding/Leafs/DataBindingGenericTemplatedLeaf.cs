using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityTK.DataBinding
{
    /// <summary>
    /// Generic templated data binding leaf.
    /// This leaf will bind any arbitrary field from parent bindings to a <see cref="DataBindingGenericTemplate"/> field on a specific target object.
    /// 
    /// This implementation can be used to create leaf types very flexibly.
    /// A template for example can bind to the <see cref="UnityEngine.UI.Text.text"/> of <see cref="UnityEngine.UI.Text"/>.
    /// The leaf then in turn can bind any arbitrary string value to the bound object using the bind template.
    /// </summary>
    public class DataBindingGenericTemplatedLeaf : DataBindingLeaf
    {
        /// <summary>
        /// The bind target object for the template.
        /// </summary>
        public UnityEngine.Object bindTarget;

        /// <summary>
        /// The template this leaf is using to bind.
        /// </summary>
        public DataBindingGenericTemplate template;

        /// <summary>
        /// The node with the field this leaf is binding to the templated field.
        /// </summary>
        [ParentComponent(typeof(DataBindingNode))]
        public DataBindingNode parentNode;

        /// <summary>
        /// The field this leaf is binding to the templated field.
        /// </summary>
        [DataBindingField]
        public string field;

        public override DataBinding parent
        {
            get
            {
                return this.parentNode;
            }
        }

        /// <summary>
        /// Returns the field type <see cref="template"/> if set, typeof(<see cref="object"/>) if not.
        /// </summary>
        public override Type GetBindTargetType()
        {
            if (Essentials.UnityIsNull(this.template))
                return typeof(object);
            else
            {
                var type = this.template.GetFieldType();

                if (type == typeof(string))
                    return typeof(object); // String targets use the ToString() method to convert. See UpdateBinding for special string handling

                return type;
            }
        }

        private void OnValidate()
        {
            if (!Essentials.UnityIsNull(this.template) && !Essentials.UnityIsNull(null))
            {
                if (this.bindTarget.GetType() != this.template.GetTargetType())
                {
                    Debug.LogError("Cannot bind object of type " + this.bindTarget.GetType() + " with template " + this.template + " whose target type is " + this.template.GetTargetType());
                    this.bindTarget = null;
                }
            }
        }

        /// <summary>
        /// Replicates the parent node field this leaf binds to onto the bind target.
        /// </summary>
        public override void UpdateBinding()
        {
            // Bind target valid?
            if (Essentials.UnityIsNull(this.bindTarget))
            {
                Debug.LogError("Bind target of generic templated leaf is null!", this.gameObject);
                return;
            }
            if (Essentials.UnityIsNull(this.template))
            {
                Debug.LogError("Bind target of generic templated leaf template is null!", this.gameObject);
                return;
            }

            // Read template and bind target object data
            var type = this.template.GetTargetType();
            var field = this.template.GetField();
            var bindTargetType = this.bindTarget.GetType();

            // Test for type compatibility
            if (!type.IsAssignableFrom(bindTargetType))
            {
                Debug.LogError("Bind target type of generic templated leaf is not assignable to template target type!", this.gameObject);
                return;
            }

            // Set value
            field.SetValue(this.bindTarget, GetBoundObject());
        }

        protected override object GetBoundObject()
        {
            var obj = this.parentNode.GetFieldValue(this.field);
            if (this.template.GetFieldType() == typeof(string))
            {
                // Special case for strings, if the object retrieved is not a string we run type conversion to string!
                if (obj == null)
                    obj = "null";
                else if (obj.GetType() != typeof(string))
                    obj = obj.ToString();
            }

            return obj;
        }

        /// <summary>
        /// Writes value from the bind template field to the bound field.
        /// </summary>
        public override void OnChanged()
        {
            // Read template and bind target object data
            var field = this.template.GetField();

            // Set value
            this.parentNode.SetFieldValue(this.field, field.GetValue(this.bindTarget));
        }
    }
}
