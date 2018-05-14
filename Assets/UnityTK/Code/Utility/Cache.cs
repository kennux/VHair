using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityTK
{
    /// <summary>
    /// Simple cache implementation that can map a cached entry to a specific key.
    /// This is essentially a wrapped <see cref="Dictionary{TKey, TValue}"/>.
    /// 
    /// This class is thread-safe.
    /// </summary>
    public class Cache<TKey, TValue>
    {
        /// <summary>
        /// Delegate that can be used to create a cache entry for the specified key.
        /// </summary>
        public delegate TValue Constructor(TKey key);

        private object context;
        private Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();
        private object lck = new object();

        private Constructor constructor;

        public Cache(Constructor constructor)
        {
            this.constructor = constructor;
        }

        /// <summary>
        /// Can be called to update the cache's context object.
        /// If the context object changes, the cache will be cleared!
        /// </summary>
        public void UpdateContext(object context)
        {
            lock (this.lck)
            {
                // Update culling
                if (object.ReferenceEquals(this.context, context))
                    return;

                // Context update
                this.context = context;
                this.Clear();
            }
        }

        /// <summary>
        /// Completely clears this cache.
        /// </summary>
        public void Clear()
        {
            lock (this.lck)
            {
                this.dict.Clear();
            }
        }

        public TValue Get(TKey key)
        {
            TValue val = default(TValue);
            lock (this.lck)
            {
                if (!this.dict.TryGetValue(key, out val))
                {
                    val = this.constructor(key);
                    this.dict.Add(key, val);
                }
            }

            return val;
        }
    }
}
