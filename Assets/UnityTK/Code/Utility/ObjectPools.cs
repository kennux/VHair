using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UnityTK
{
    public static class ListPool<T>
    {
        public static ObjectPool<List<T>> pool = new ObjectPool<List<T>>(() => new List<T>(), 1000, (lst) => lst.Clear());

        /// <summary>
        /// <see cref="ObjectPool{T}.GetIfNull(ref T)"/>
        /// </summary>
        public static void GetIfNull(ref List<T> lst)
        {
            pool.GetIfNull(ref lst);
        }

        /// <summary>
        /// <see cref="ObjectPool{T}.Get"/>
        /// </summary>
        public static List<T> Get()
        {
            return pool.Get();
        }

        /// <summary>
        /// <see cref="ObjectPool{T}.Return(T, bool)"/>
        /// </summary>
        public static void Return(List<T> lst)
        {
            pool.Return(lst);
        }
    }

    public static class QueuePool<T>
    {
        public static ObjectPool<Queue<T>> pool = new ObjectPool<Queue<T>>(() => new Queue<T>(), 1000, (queue) => queue.Clear());

        /// <summary>
        /// <see cref="ObjectPool{T}.GetIfNull(ref T)"/>
        /// </summary>
        public static void GetIfNull(ref Queue<T> lst)
        {
            pool.GetIfNull(ref lst);
        }

        /// <summary>
        /// <see cref="ObjectPool{T}.Get"/>
        /// </summary>
        public static Queue<T> Get()
        {
            return pool.Get();
        }

        /// <summary>
        /// <see cref="ObjectPool{T}.Return(T, bool)"/>
        /// </summary>
        public static void Return(Queue<T> lst)
        {
            pool.Return(lst);
        }
    }

    public static class HashSetPool<T>
    {
        public static ObjectPool<HashSet<T>> pool = new ObjectPool<HashSet<T>>(() => new HashSet<T>(), 1000, (lst) => lst.Clear());

        /// <summary>
        /// <see cref="ObjectPool{T}.GetIfNull(ref T)"/>
        /// </summary>
        public static void GetIfNull(ref HashSet<T> lst)
        {
            pool.GetIfNull(ref lst);
        }

        /// <summary>
        /// <see cref="ObjectPool{T}.Get"/>
        /// </summary>
        public static HashSet<T> Get()
        {
            return pool.Get();
        }

        /// <summary>
        /// <see cref="ObjectPool{T}.Return(T, bool)"/>
        /// </summary>
        public static void Return(HashSet<T> lst)
        {
            pool.Return(lst);
        }
    }

    public static class DictionaryPool<TKey, TValue>
    {
        public static ObjectPool<Dictionary<TKey, TValue>> pool = new ObjectPool<Dictionary<TKey, TValue>>(() => new Dictionary<TKey, TValue>(), 1000, (lst) => lst.Clear());

        /// <summary>
        /// <see cref="ObjectPool{T}.GetIfNull(ref T)"/>
        /// </summary>
        public static void GetIfNull(ref Dictionary<TKey, TValue> dict)
        {
            pool.GetIfNull(ref dict);
        }

        /// <summary>
        /// <see cref="ObjectPool{T}.Get"/>
        /// </summary>
        public static Dictionary<TKey, TValue> Get()
        {
            return pool.Get();
        }

        /// <summary>
        /// <see cref="ObjectPool{T}.Return(T, bool)"/>
        /// </summary>
        public static void Return(Dictionary<TKey, TValue> dict)
        {
            pool.Return(dict);
        }
    }

    public static class GenericObjectPool<T> where T : class, new()
    {
        private static ObjectPool<T> pool = new ObjectPool<T>(() => new T());

        /// <summary>
        /// <see cref="ObjectPool{T}.GetIfNull(ref T)"/>
        /// </summary>
        public static void GetIfNull(ref T obj)
        {
            pool.GetIfNull(ref obj);
        }

        /// <summary>
        /// <see cref="ObjectPool{T}.Get"/>
        /// </summary>
        public static T Get()
        {
            return pool.Get();
        }

        /// <summary>
        /// <see cref="ObjectPool{T}.Return(T, bool)"/>
        /// </summary>
        public static void Return(T obj)
        {
            pool.Return(obj);
        }
    }
}
