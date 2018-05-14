using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.AssetManagement
{
    /// <summary>
    /// Base class that can be used for implementing manageable scriptable objects.
    /// <see cref="IManagedAsset"/>, <see cref="AssetManagement"/>
    /// </summary>
    public abstract class ManagedScriptableObject : ScriptableObject, IManagedAsset
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
            return this as T;
        }

        protected virtual void OnValidate()
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
            return this.name;
        }
    }
}