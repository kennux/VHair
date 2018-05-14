using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.AssetManagement
{
    /// <summary>
    /// Abstract class that defines and partially implements an interface for the <see cref="AssetManager"/> that is able to load assets.
    /// </summary>
    public abstract class AssetLoader : MonoBehaviour
    {
        /// <summary>
        /// Called in order to load all assets and retrieve all <see cref="IManagedAsset"/> assets.
        /// </summary>
        protected abstract List<IManagedAsset> LoadAssets();

        /// <summary>
        /// Called in order to unload all assets loaded by this loader.
        /// The specified list of assets was previously created by <see cref="LoadAssets"/>
        /// </summary>
        /// <param name="assets"></param>
        protected abstract void UnloadAssets(List<IManagedAsset> assets);

        /// <summary>
        /// Whether or not this asset loader should unload the assets it has loaded when it is destroyed (for example in scene load).
        /// </summary>
        public bool unloadOnDestroy = false;
        private List<IManagedAsset> assets;

        public virtual void Awake()
        {
            this.assets = this.LoadAssets();

            for (int i = 0; i < assets.Count; i++)
                AssetManager.instance.RegisterAsset(this.assets[i]);
        }

        public virtual void OnDestroy()
        {
            if (!this.unloadOnDestroy)
                return;

            // Deregister
            for (int i = 0; i < assets.Count; i++)
                AssetManager.instance.DeregisterAsset(this.assets[i]);

            this.UnloadAssets(this.assets);
        }
    }
}