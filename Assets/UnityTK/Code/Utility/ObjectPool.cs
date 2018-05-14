using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityTK
{
    /// <summary>
    /// Object pool implementation that can be used to very efficiently pool objects.
    /// This pool implementation is thread-safe!
    /// </summary>
    public class ObjectPool<T> where T : class
    {
        /// <summary>
        /// Constructor call delegate that can be used to specify the action performed to create a new instance of T.
        /// </summary>
        /// <returns>A new instance of type T</returns>
        public delegate T Constructor();
        
        /// <summary>
        /// This delegate is used to define an method that is invoked everytime an item is returned back to the pool.
        /// </summary>
        /// <param name="obj">The object that was just returned.</param>
        public delegate void OnReturn(T obj);

        /// <summary>
        /// This delegate is used to define a method that is invoked everytime an item is drawn from the pool and returned to the user.
        /// </summary>
        /// <param name="obj">The item that was drawn.</param>
        public delegate void OnGet(T obj);

        /// <summary>
        /// <see cref="Constructor"/>
        /// </summary>
        private Constructor constructor;

        /// <summary>
        /// <see cref="OnReturn"/>
        /// </summary>
        private OnReturn onReturn;

        /// <summary>
        /// <see cref="OnGet"/>
        /// </summary>
        private OnGet onGet;

        /// <summary>
        /// The current amount of pooled objects.
        /// </summary>
        public int count
        {
            get
            {
                lock (this.poolLock)
                {
                    return this.pool.Count;
                }
            }
        }

        /// <summary>
        /// The maximum size of <see cref="pool"/>
        /// </summary>
        private int maxPoolSize;

        /// <summary>
        /// The stack that is being used to store the currently pooled objects.
        /// </summary>
        private Stack<T> pool = new Stack<T>();

        /// <summary>
        /// Fast contains checks to prevent duplicate entries.
        /// </summary>
        private HashSet<T> poolHashSet = new HashSet<T>();
        private object poolLock = new object();

        /// <summary>
        /// Creates a new instance of the object pool and initializes it.
        /// </summary>
        /// <param name="maxPoolSize">The maximum amount of objects that should be pooled maximum.</param>
        public ObjectPool(Constructor constructor, int maxPoolSize = 1000, OnReturn onReturn = null, OnGet onGet = null)
        {
            this.constructor = constructor;
            this.onReturn = onReturn;
            this.onGet = onGet;
            this.maxPoolSize = maxPoolSize;
        }

        /// <summary>
        /// Helper method that can be called to replace the obj reference with an object from the pool if it is pointing to null.
        /// </summary>
        /// <param name="obj">Object refernece</param>
        public void GetIfNull(ref T obj)
        {
            if (object.ReferenceEquals(obj, null))
                obj = Get();
        }

        /// <summary>
        /// Returns a new or a pooled object of type T.
        /// </summary>
        public T Get()
        {
            T obj = null;
            // Pool check
            lock (this.poolLock)
            {
                if (this.pool.Count > 0)
                {
                    obj = this.pool.Pop();
                    this.poolHashSet.Remove(obj);
                }
            }

            // Construct new object if necessary
            if (object.ReferenceEquals(obj, null))
                obj = this.constructor();

            // Invoke onGet
            if (!object.ReferenceEquals(this.onGet, null))
                this.onGet(obj);

            return obj;
        }

        /// <summary>
        /// Returns an object to the pool.
        /// The object can be any arbitrary instance of type T.
        /// </summary>
        /// <param name="obj">The object that is being returned to the pool.</param>
        /// <param name="throwOnDuplicate">Can be enable to throw an <see cref="InvalidOperationException"/> when the object is already in the pool. If this is false, the method will simply return if the object is already in the pool.</param>
        public void Return(T obj, bool throwOnDuplicate = false)
        {
            if (obj == null)
                return;

            // Contains check
            lock (this.poolLock)
            {
                if (this.poolHashSet.Contains(obj))
                {
                    if (throwOnDuplicate)
                        throw new InvalidOperationException("Cannot return object to pool twice");
                    return; // Duplicate but not error throw requested
                }
            }

            // Invoke onReturn
            if (!object.ReferenceEquals(this.onReturn, null))
                this.onReturn(obj);

            // Insert
            lock (this.poolLock)
            {
                if (this.pool.Count < this.maxPoolSize)
                {
                    this.pool.Push(obj);
                    this.poolHashSet.Add(obj);
                }
            }
        }

    }
}
