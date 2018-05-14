using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace UnityTK.DataBinding
{
    /// <summary>
    /// A specialized databinding leaf that can be used to bind to arbitrary collections.
    /// For every object in the collection, a prefab with a component of type 
    /// </summary>
    public class DataBindingCollectionLeaf : DataBindingLeaf
    {
        private static ObjectPool<ElementInstance> elementInstancePool = new ObjectPool<ElementInstance>(() => new ElementInstance());

        /// <summary>
        /// Essentially a struct tuple of object and representation.
        /// </summary>
        private class ElementInstance
        {
            public object obj;
            public DataBindingCollectionElement element;

            public void Init(object obj, DataBindingCollectionElement element)
            {
                this.obj = obj;
                this.element = element;
            }
        }

        /// <summary>
        /// All currently created and bound instances of elements inside the collection this leaf binds to.
        /// </summary>
        private List<ElementInstance> instances = new List<ElementInstance>();

        /// <summary>
        /// The element prefab that is being instantiated and set up for 
        /// </summary>
        public DataBindingCollectionElement elementPrefab;

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
            get { return this.parentNode; }
        }

        [ContextMenu("Run OnValidate")]
        public void OnValidate()
        {
            if (!Essentials.UnityIsNull(this.elementPrefab))
            {
                this.elementPrefab.SetElementType(this.GetElementType());
            }
        }

        /// <summary>
        /// Returns the type of the elements this collection leaf binds to.
        /// Will return typeof(object) if no valid binding is established on this leaf.
        /// </summary>
        public Type GetElementType()
        {
            var boundType = this.boundType;
            if (boundType == null)
                return typeof(object);

            if (boundType.IsArray)
                return boundType.GetElementType();

            // Look for generic type
            foreach (var i in boundType.GetInterfaces())
                if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))
                    return i.GetGenericArguments()[0];

            // No type hinting possible, we have to go with object :/
            return typeof(object);
        }

        protected override Type GetBoundType()
        {
            if (Essentials.UnityIsNull(this.parentNode))
                return typeof(object);

            return this.parentNode.GetFieldType(this.field);
        }

        public override Type GetBindTargetType()
        {
            return typeof(ICollection);
        }

        /// <summary>
        /// Not supported on collection leaves!
        /// </summary>
        public override void OnChanged()
        {
            throw new NotImplementedException();
        }

        public override void UpdateBinding()
        {
            var boundObject = this.boundObject;
            if (object.ReferenceEquals(boundObject, null))
                DestroyElements();
            else
            {
                var boundCollection = (ICollection)boundObject;
                bool wasInvalid = false;

                // Check for sequence equality in instances
                if (boundCollection.Count == this.instances.Count)
                {
                    int i = 0;
                    foreach (var obj in boundCollection)
                    {
                        var instance = this.instances[i];
                        if (!object.ReferenceEquals(instance.obj, obj))
                        {
                            wasInvalid = true;
                            break;
                        }

                        i++;
                    }
                }
                else
                    wasInvalid = true;

                // Need rebuild!
                if (wasInvalid)
                {
                    DestroyElements();
                    Rebuild(boundCollection);
                }
            }

            // Update instances
            for (int i = 0; i < this.instances.Count; i++)
                this.instances[i].element.SetTargetObject(this.instances[i].obj);
        }

        /// <summary>
        /// Destroys all elements.
        /// </summary>
        private void DestroyElements()
        {
            for (int i = 0; i < this.instances.Count; i++)
            {
                Destroy(this.instances[i].element.gameObject);
                elementInstancePool.Return(this.instances[i]);
            }

            this.instances.Clear();
        }

        /// <summary>
        /// Rebuilds the collection leaf children (element instances).
        /// </summary>
        private void Rebuild(ICollection collection)
        {
            foreach (var obj in collection)
            {
                var element = CreateElement(obj);
                var instance = elementInstancePool.Get();

                instance.Init(obj, element);
                this.instances.Add(instance);
            }
        }

        /// <summary>
        /// Instantiates <see cref="elementPrefab"/> and returns the instantiated component reference.
        /// The transform of the instantiated object will be parented to the transform of this collection leaf.
        /// </summary>
        private DataBindingCollectionElement CreateElement(object obj)
        {
            var go = Instantiate(this.elementPrefab.gameObject, this.transform);

            go.SetActive(true);

            return go.GetComponent<DataBindingCollectionElement>();
        }

        protected override object GetBoundObject()
        {
            return this.parentNode.GetFieldValue(this.field);
        }
    }
}
