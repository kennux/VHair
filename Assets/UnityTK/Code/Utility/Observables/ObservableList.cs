using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnityTK
{
    /// <summary>
    /// Datastructure for creating object and structure <see cref="List{T}"/> fields, which can be "observed".
    /// Observers are able to bind handlers to events regarding the property.
    /// </summary>
    public struct ObservableList<T> : IList<T>
    {
        public delegate void ListElementEvent(T element);

        /// <summary>
        /// Invoked every time an element is removed from the list.
        /// Called before the element is removed.
        /// </summary>
        public event ListElementEvent onRemove;

        /// <summary>
        /// Invoked every time an element is removed from the list.
        /// Called before the element is added.
        /// </summary>
        public event ListElementEvent onAdd;

        /// <summary>
        /// Invoked every time the list is being cleared.
        /// Called before the clear is executed.
        /// </summary>
        public event Action onClear;

        /// <summary>
        /// Read-only view on the internal list.
        /// </summary>
        public ReadOnlyList<T> roList
        {
            get { return new ReadOnlyList<T>(this.list); }
        }

        /// <summary>
        /// The internal list object.
        /// </summary>
        private List<T> list;

        public int Count => this.list.Count;

        public bool IsReadOnly => false;

        public T this[int index]
        {
            get { return this.list[index]; }
            set { this.list[index] = value; }
        }

        public ObservableList(List<T> list)
        {
            this.onClear = null;
            this.onRemove = null;
            this.onAdd = null;
            this.list = list;
        }

        public List<T>.Enumerator GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return this.list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            this.onAdd?.Invoke(item);
            this.list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            this.onRemove?.Invoke(this.list[index]);
            this.list.RemoveAt(index);
        }

        public void Add(T item)
        {
            this.onAdd?.Invoke(item);
            this.list.Add(item);
        }

        /// <summary>
        /// <see cref="ICollection{T}.Clear"/>
        /// Calls <see cref="onRemove"/> for every element, and <see cref="onClear"/> after clearing.
        /// </summary>
        public void Clear()
        {
            foreach (var item in list)
                this.onRemove?.Invoke(item);
            this.onClear?.Invoke();

            this.list.Clear();
        }

        public bool Contains(T item)
        {
            return this.list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            if (this.list.Remove(item))
            {
                this.onRemove?.Invoke(item);
                return true;
            }
            else
                return false;
        }
    }
}