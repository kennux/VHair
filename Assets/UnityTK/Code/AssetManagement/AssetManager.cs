using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.AssetManagement
{
    /// <summary>
    /// Main asset management class.
    /// Provides high-level api to working with UnityTK's asset management system at runtime.
    /// Note that the AssetManagement system should only be used for small to medium sized assets like for example weapons in a first person shooter.
    /// Large assets like levels should not be loaded by the assetmanager as it always keeps all assets loaded by it in memory.
    /// 
    /// This system provides the following functionality:
    /// - Loading and unloading asset bundles at runtime
    /// - Indexing the loaded asset bundle assets via <see cref="ManagedScriptableObject"/>
    /// - Easy runtime access to the loaded assets via a flexible api
    /// 
    /// When the asset manager loads an asset bundle, it will read all <see cref="ManagedScriptableObject"/> and GameObjects.
    /// The managed scriptable objects will directly be registered as is.
    /// The GameObjects will be queried for a component implementing <see cref="IManagedAsset"/> (<see cref="ManagedGameObject"/>).
    /// Every implementation found is registered to the asset manager.
    /// </summary>
    public class AssetManager : MonoBehaviour
    {
        #region Singleton
        /// <summary>
        /// Asset manager singleton, will create a new gameobject and DontDestroyOnLoad's it.
        /// </summary>
        public static AssetManager instance
        {
            get
            {
                if (Essentials.UnityIsNull(_instance))
                {
                    // Try to find a (not yet) registered manager object in the scene
                    _instance = FindObjectOfType<AssetManager>();
                    if (!Essentials.UnityIsNull(_instance))
                        return _instance;

                    var go = new GameObject("_AssetManager_");

                    try
                    {
                        DontDestroyOnLoad(go);
                    }
                    catch (System.InvalidOperationException)
                    {
                        // Happens when ran as unit test
                    }
                    _instance = go.AddComponent<AssetManager>();
                }

                return _instance;
            }
        }
        private static AssetManager _instance;
        #endregion

        /// <summary>
        /// All assets registered to the manager.
        /// </summary>
        private HashSet<IManagedAsset> registeredAssets = new HashSet<IManagedAsset>();
        private List<IManagedAsset> _registeredAssets = new List<IManagedAsset>();

        /// <summary>
        /// Maps <see cref="IManagedAsset.identifier"/> to the index of <see cref="_registeredAssets"/>.
        /// </summary>
        private Dictionary<string, int> identifierAssetMap = new Dictionary<string, int>();

        #region Asset registration

        /// <summary>
        /// Registers the specified asset identifier to this asset manager.
        /// If the passed in asset has an <see cref="IManagedAsset.identifier"/> that is already known, the known asset will be overridden!
        /// This can be used to implement modding or override specific assets purpusefully.
        /// </summary>
        public void RegisterAsset(IManagedAsset asset)
        {
            if (registeredAssets.Contains(asset))
                return;

            int idx;
            if (identifierAssetMap.TryGetValue(asset.identifier, out idx))
            {
                Debug.Log("Replaced existing asset with identifier " + asset.identifier + " with new asset! It was overridden!");

                // Overwrite prefab
                var registeredAsset = _registeredAssets[idx];
                registeredAssets.Remove(registeredAsset);
                registeredAssets.Add(asset);
                _registeredAssets[idx] = asset;
            }
            else
            {
                // Register prefab
                Debug.Log("Registered asset " + asset);
                registeredAssets.Add(asset);
                _registeredAssets.Add(asset);
                identifierAssetMap.Add(asset.identifier, _registeredAssets.Count - 1);
            }
        }

        /// <summary>
        /// Deregisters the specified asset from this asset manager.
        /// Must have previously been registered by either asset bundle loading or <see cref="RegisterAsset(IManagedAsset)"/>
        /// </summary>
        public void DeregisterAsset(IManagedAsset asset)
        {
            if (!registeredAssets.Contains(asset))
                return;

            identifierAssetMap.Remove(asset.identifier);
            _registeredAssets.Remove(asset);
            registeredAssets.Remove(asset);
        }

        #endregion

        #region Query logic

        /// <summary>
        /// Will retrieve the object registered with the specified identifier (<see cref="IManagedAsset.identifier"/>).
        /// </summary>
        /// <typeparam name="T">The type the objects must have to end up in the result set. Scriptable objects are being checked if they are assignable to the specified type.
        /// GameObjects will be checked whether or not they have a component of the specified type.
        /// If T is GameObject, the IManagedAsset implementation will be casted to Component to retrieve the gameobject.</typeparam>
        /// <param name="identifier"></param>
        /// <param name="throwCastException">whether or not an <see cref="System.InvalidCastException"/> will be thrown if the type T cannot be retrieved from an asset that has the specified tag.</param>
        /// <returns>The object, or null if no object was found.</returns>
        public T GetObject<T>(string identifier, bool throwCastException = false) where T : UnityEngine.Object
        {
            int assetIdx;
            if (!this.identifierAssetMap.TryGetValue(identifier, out assetIdx))
                return null;

            // Try casting and throw exception if requested
            IManagedAsset asset = this._registeredAssets[assetIdx];
            T obj = asset.GetAs<T>();
            if (Essentials.UnityIsNull(obj) && throwCastException)
                throw new System.InvalidCastException("Object " + asset.name + " wasnt castable to " + typeof(T));

            return obj;
        }

        /// <summary>
        /// Queries the asset manager for assets.
        /// </summary>
        /// <typeparam name="T">The type the objects must have to end up in the result set. Scriptable objects are being checked if they are assignable to the specified type.
        /// GameObjects will be checked whether or not they have a component of the specified type.
        /// If T is GameObject, the IManagedAsset implementation will be casted to Component to retrieve the gameobject.</typeparam>
        /// <param name="query">The query parameters</param>
        /// <param name="preAlloc">A pre-allocated list that will be used as return list. If not supplied, a list from the <see cref="ListPool{T}"/> is being drawn. This list can also already be containing objects.</param>
        /// <param name="limit">The limit of how many objects to look for maximum. -1 employs no limit.</param>
        /// <param name="throwCastException">whether or not an <see cref="System.InvalidCastException"/> will be thrown if the type T cannot be retrieved from an asset that has the specified tag.</param>
        public List<T> Query<T>(IAssetManagerQuery query, List<T> preAlloc = null, int limit = -1, bool throwCastException = false) where T : UnityEngine.Object
        {
            ListPool<T>.GetIfNull(ref preAlloc);

            int selected = 0;
            for (int i = 0; i < _registeredAssets.Count; i++)
            {
                // Read asset and cast
                var asset = _registeredAssets[i];
                if (!query.MatchesCriterias(asset))
                    continue;

                var casted = asset.GetAs<T>();

                // Was casting not successfull?
                if (Essentials.UnityIsNull(casted))
                {
                    if (throwCastException)
                        throw new System.InvalidCastException("Object " + asset.name + " wasnt castable to " + typeof(T));
                }
                else
                {
                    preAlloc.Add(casted);

                    if (limit != -1 && selected >= limit)
                        break;
                }
            }

            return preAlloc;
        }

        #endregion
    }
}