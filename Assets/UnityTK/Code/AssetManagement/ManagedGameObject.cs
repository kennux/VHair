using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.AssetManagement
{
    /// <summary>
    /// Implements the managed asset interface as monobehaviour component.
    /// <see cref="IManagedAsset"/>
    /// </summary>
    public class ManagedGameObject : MonoBehaviour, IManagedAsset
    {
        /// <summary>
        /// Sets <see cref="identifier"/> to the result of calling <see cref="GenerateIdentifier"/>
        /// </summary>
        [ContextMenu("Generate identifier")]
        public void SetGeneratedIdentifier()
        {
            this.identifier = this.GenerateIdentifier();
        }

        /// <summary>
        /// The unique identifier of this asset.
        /// </summary>
        public string identifier;

        /// <summary>
        /// The tags this asset has assigned.
        /// </summary>
        public string[] tags;

        string IManagedAsset.identifier
        {
            get { return this.identifier; }
        }

        string[] IManagedAsset.tags
        {
            get { return this.tags; }
        }

        public T GetAs<T>() where T : Object
        {
            if (!typeof(Component).IsAssignableFrom(typeof(T)))
                return null;

            return this.GetComponent<T>();
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(this.identifier))
                this.identifier = this.GenerateIdentifier();
        }

        /// <summary>
        /// Called in order to generate an identifier for this managed gameobject that is (most likely) unique.
        /// Called from <see cref="OnValidate"/> and an in-editor method to generate the identifier.
        /// 
        /// The standard implementation returns the gameobject name.
        /// </summary>
        protected virtual string GenerateIdentifier()
        {
            return this.gameObject.name;
        }
    }
}
