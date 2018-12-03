using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace UnityTK.BehaviourModel
{
    /// <summary>
    /// <see cref="ModelCollectionPropertyBase{TElement, TCollection, TEnumerator}"/> for lists.
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    public class ModelListCollectionProperty<T> : ModelCollectionPropertyBase<T, List<T>, List<T>.Enumerator> { }

    /// <summary>
    /// <see cref="ModelCollectionPropertyBase{TElement, TCollection, TEnumerator}"/> for hashsets.
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    public class ModelHashSetProperty<T> : ModelCollectionPropertyBase<T, HashSet<T>, HashSet<T>.Enumerator> { }

    /// <summary>
    /// <see cref="ModelCollectionPropertyBase{TElement, TCollection, TEnumerator}"/> for generic collections.
    /// 
    /// It is not recommended to use this version of the collection property as it will cause boxing and other memory allocations generally (implementation dependent, .Net collections will cause memory allocations on getting and other operations).
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    public class ModelCollectionProperty<T> : ModelCollectionPropertyBase<T, ICollection<T>, IEnumerator<T>> { }

    /// <summary>
    /// Similar to ModelProperty but provides the ability to concat collections returned from the getters.
    /// This is useful in situations where you have for example an inventory mechanic property that lists all items but the items are held by multiple logic components(for example multiple bags).
    /// Setters can consume objects from the set call in order to claim them being set on themselves.
    /// </summary>
    /// <typeparam name="TElement">Element type</typeparam>
    public class ModelCollectionPropertyBase<TElement, TCollection, TEnumerator> : ICollection<TElement> where TCollection : ICollection<TElement> where TEnumerator : IEnumerator<TElement>
	{
        /// <summary>
        /// Internal enumerator used for enumerating over a set of objects.
        /// </summary>
        public struct ConcatEnumerator : IEnumerator<TElement>
        {
            private int indexBounds;
            private int index;
            private List<TCollection> collections;

            public void Init()
            {
                this.indexBounds = 0;
                this.index = -1;
                this.collections = ListPool<TCollection>.Get();
            }

            public TElement Current
            {
                get
                {
                    // TODO: Optimize :P
                    int id = this.index;
                    for (int i = 0; i < collections.Count; i++)
                    {
                        var collection = collections[i];
                        foreach (var element in collection)
                        {
                            if (id == 0)
                                return element;
                            id--;
                        }
                    }

                    return default(TElement);
                }
            }

            object IEnumerator.Current
            {
                get { return this.Current; }
            }

            public void Dispose()
            {
                ListPool<TCollection>.Return(this.collections);
                this.collections = null;
            }

            public bool MoveNext()
            {
                this.index++;
                return this.index < this.indexBounds;
            }

            public void AddCollection(TCollection collection)
            {
                this.collections.Add(collection);
                this.indexBounds += collection.Count;
            }

            public void Reset()
            {
                this.index = 0;
                this.indexBounds = 0;
                this.collections.Clear();
            }
        }

        /// <summary>
        /// Getter delegate that is used to retrieve collections of registered getters <see cref="RegisterGetter(Getter)"/>
        /// </summary>
        public delegate TCollection Getter();

        /// <summary>
        /// Insertion handler that can be used to handle object insertions.
        /// </summary>
        /// <param name="obj">The object to insert</param>
        /// <returns>Whether or not the object could be inserted</returns>
        public delegate bool InsertHandler(TElement obj);

        /// <summary>
        /// Removal handler that can be used to handle object removals.
        /// </summary>
        /// <param name="obj">The object to remove from this collection property</param>
        /// <returns>Whether or not the object could be removed</returns>
        public delegate bool RemovalHandler(TElement obj);

        /// <summary>
        /// The getters bound to this property.
        /// </summary>
        private List<Getter> getters = new List<Getter>();

        /// <summary>
        /// List of insertion handlers.
        /// </summary>
        private List<InsertHandler> insertionHandlers = new List<InsertHandler>();

        /// <summary>
        /// List of removal handlers.
        /// </summary>
        private List<RemovalHandler> removalHandlers = new List<RemovalHandler>();

        /// <summary>
        /// Registers a collection getter that is being used in <see cref="Get"/>.
        /// </summary>
        public void RegisterGetter(Getter getter)
		{
            this.getters.Add(getter);
		}

        /// <summary>
        /// Registers an insertion handler.
        /// <see cref="InsertHandler"/>
        /// </summary>
        public void RegisterInsertHandler(InsertHandler inserter)
        {
            this.insertionHandlers.Add(inserter);
        }

        /// <summary>
        /// Registers a removal handler.
        /// <see cref="RemovalHandler"/>
        /// </summary>
        public void RegisterRemovalHandler(RemovalHandler handler)
        {
            this.removalHandlers.Add(handler);
        }

        /// <summary>
        /// Returns the content of the collection as <see cref="ConcatEnumerator"/>.
        /// </summary>
		public ConcatEnumerator Get()
		{
			if (this.getters == null)
			{
				Debug.LogWarning("Tried getting value event with no getter!");
				return default(ConcatEnumerator);
			}

            // Setup enumerator
            ConcatEnumerator enumerator = new ConcatEnumerator();
            enumerator.Init();
            for (int i = 0; i < this.getters.Count; i++)
                enumerator.AddCollection(this.getters[i]());

            return enumerator;
		}

        /// <summary>
        /// Removes the specified object from this collection property.
        /// <see cref="RemovalHandler"/>, <see cref="RegisterRemovalHandler(RemovalHandler)"/>
        /// </summary>
        /// <param name="obj">The object to be removed</param>
        /// <returns>Whether or not the specified object could be removed.</returns>
        public bool Remove(TElement obj)
        {
            if (this.removalHandlers.Count == 0)
                Debug.LogWarning("Tried removing value from model collection property without removal handlers!");
            else
            {
                foreach (var removalHandler in this.removalHandlers)
                    if (removalHandler.Invoke(obj))
                        return true;
            }

            return false;
        }

        /// <summary>
        /// Inserts the specified object into this colletion.
        /// This operation is only supported if <see cref="RegisterInsertHandler(InsertHandler)"/> inserters are registered.
        /// </summary>
        /// <returns>Whether or not the item could be inserted.</returns>
        public bool Insert(TElement obj)
        {
            if (this.insertionHandlers.Count == 0)
                Debug.LogWarning("Tried inserting value into model collection property without inserters bound!");
            else
            {
                foreach (var insertionHandler in this.insertionHandlers)
                    if (insertionHandler.Invoke(obj))
                        return true; // insertion successfull!
            }

            // No insertion possible!
            return false;
        }

        #region Interface implementation

        int ICollection<TElement>.Count
        {
            get
            {
                int c = 0;
                foreach (var getter in this.getters)
                    c += getter.Invoke().Count;

                return c;
            }
        }

        bool ICollection<TElement>.IsReadOnly
        {
            get { return this.insertionHandlers.Count == 0 && this.removalHandlers.Count == 0; }
        }

        void ICollection<TElement>.Add(TElement item)
        {
            Insert(item);
        }

        void ICollection<TElement>.Clear()
        {
            List<TElement> lst = ListPool<TElement>.Get();
            lst.AddRange(this);

            for (int i = 0; i < lst.Count; i++)
                if (!Remove(lst[i]))
                    Debug.LogError("Could not remove object " + lst[i] + " from model collection property tho it was in there?!");

            ListPool<TElement>.Return(lst);
        }

        bool ICollection<TElement>.Contains(TElement item)
        {
            // TODO: allow user defined equality comparator
            foreach (var val in this)
                if (EqualityComparer<TElement>.Default.Equals(val, item))
                    return true;

            return false;
        }

        void ICollection<TElement>.CopyTo(TElement[] array, int arrayIndex)
        {
            int ptr = arrayIndex;

            foreach (var obj in this)
            {
                array[ptr] = obj;
                ptr++;
            }
        }

        IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
        {
            return Get();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Get();
        }
        #endregion
    }
}