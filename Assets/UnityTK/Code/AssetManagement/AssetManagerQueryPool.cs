using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityTK.AssetManagement
{
    /// <summary>
    /// Static asset manager query pool.
    /// This pool can be used to dynamically retrieve and return <see cref="IAssetManagerQuery"/> implementations.
    /// </summary>
    public static class AssetManagerQueryPool<T> where T : class, IAssetManagerQuery, new()
    {
        private static ObjectPool<T> pool = new ObjectPool<T>(() => new T(), 1000, (query) => query.Reset());

        /// <summary>
        /// Draws a new query from the pool or creates a new object.
        /// </summary>
        public static T Get()
        {
            return pool.Get();
        }

        /// <summary>
        /// Returns the specified query to its pool.
        /// </summary>
        public static void Return(T query)
        {
            pool.Return(query);
        }
    }
}
