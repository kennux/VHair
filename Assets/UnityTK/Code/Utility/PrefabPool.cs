using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK
{
    /// <summary>
    /// Prefab pool implementation.
    /// Can be added to a gameobject in the scene in order to provide prefab pooling for everything running!
    /// 
    /// This implementation is not static and once the prefab pool will be destroyed it will destroy all pooled objects!
    /// When objects are returned to the pool they are being set inactive (<see cref="GameObject.SetActive(bool)"/>).
    /// When retrieved from the pool they are always enabled!
    /// </summary>
    public class PrefabPool : SingletonBehaviour<PrefabPool>
    {
        /// <summary>
        /// Datastructure for storing prefab warmup configurations.
        /// </summary>
		[System.Serializable]
        public struct WarmupConfig
        {
            /// <summary>
            /// The prefab of this config entry.
            /// </summary>
            public GameObject prefab;

            /// <summary>
            /// The amount of instances to be created at warmup.
            /// </summary>
            public int instanceCount;
        }

        /// <summary>
        /// The warmup configuration for this pool.
        /// </summary>
        public List<WarmupConfig> warmupConfig = new List<WarmupConfig>();

        /// <summary>
        /// The pools mapped to the prefab.
        /// </summary>
        private Dictionary<GameObject, Stack<GameObject>> pools = new Dictionary<GameObject, Stack<GameObject>>();

        /// <summary>
        /// Instance lookup table.
        /// Key = Instance,
        /// Value = Prefab
        /// </summary>
        private Dictionary<GameObject, GameObject> instanceLookup = new Dictionary<GameObject, GameObject>();

        public override void Awake()
        {
            base.Awake();

            // Warmup
            List<GameObject> instances = ListPool<GameObject>.Get();
            for (int i = 0; i < this.warmupConfig.Count; i++)
            {
                // Read config
                var prefab = this.warmupConfig[i].prefab;
                var count = this.warmupConfig[i].instanceCount;

                // Instantiate
                for (int j = 0; j < count; j++)
                    instances.Add(GetInstance(prefab));

                // Return
                for (int j = 0; j < count; j++)
                    Return(instances[j]);
            }
            ListPool<GameObject>.Return(instances);
        }

        /// <summary>
        /// Retrieves or creates a new object pool for the specified prefab.
        /// <see cref="pools"/>
        /// </summary>
        /// <param name="prefab">The prefab to lookup a pool for.</param>
        /// <returns>The pool for the prefab</returns>
        private Stack<GameObject> GetPrefabPool(GameObject prefab)
        {
            // Look up pool
            Stack<GameObject> pool;
            if (!this.pools.TryGetValue(prefab, out pool))
            {
                // Create pool
                pool = new Stack<GameObject>();
                this.pools.Add(prefab, pool);
            }

            return pool;
        }

        /// <summary>
        /// Retrieves an instance of the specified prefab from pool or creates a new one.
        /// </summary>
        /// <param name="prefab">The prefab.</param>
        /// <returns>The instance.</returns>
        public GameObject GetInstance(GameObject prefab)
        {
            var pool = GetPrefabPool(prefab);

            // Pool or instantiate?
            GameObject obj;

            if (pool.Count > 0)
                obj = pool.Pop();
            else
            {
                obj = Instantiate(prefab);
                this.instanceLookup.Add(obj, prefab);
            }

            // Set object active
            obj.SetActive(true);
            return obj;
        }

        /// <summary>
        /// <see cref="GetInstance(GameObject)"/>, but initializes transform.
        /// </summary>
        /// 
        public GameObject GetInstance(GameObject prefab, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion))
        {
            var instance = GetInstance(prefab);
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.transform.parent = null;
            return instance;
        }

        /// <summary>
        /// Returns the specified object that was previously drawn from the pool.
        /// </summary>
        /// <param name="instance">Instance to return.</param>
        public void Return(GameObject instance)
        {
            GameObject prefab;
            if (!this.instanceLookup.TryGetValue(instance, out prefab))
                throw new System.InvalidOperationException("Cannot return object to pool that was not drawn from pool");

            instance.transform.parent = this.transform;
            instance.SetActive(false);
            GetPrefabPool(prefab).Push(instance);
        }
    }

}