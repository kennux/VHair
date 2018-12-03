using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK
{
    /// <summary>
    /// Unity TK's implementation of a linked list.
    /// The difference to the regular c# <see cref="System.Collections.Generic.LinkedList{T}"/> is that this implementation is actively trying to be completely memory allocation free.
    /// This is achieved by recyling the linked list nodes using an <see cref="ObjectPool{T}"/>.
    /// </summary>
    public class UTKLinkedList<T> : IEnumerable<T>, ICollection<T>, IDisposable
    {
        /// <summary>
        /// Represents a single element of a linked list.
        /// Implements the doubly-linked list similar to .Net's linked list.
        /// </summary>
        public class LinkedListElement
        {
            public T value;
            public LinkedListElement prev;
            public LinkedListElement next;
        }

        /// <summary>
        /// <see cref="UTKLinkedList{T}"/> enumerator implementation.
        /// </summary>
        public struct Enumerator : IEnumerator<T>
        {
            public UTKLinkedList<T> list;
            public bool isInitialized;

            public T Current
            {
                get
                {
                    if (ReferenceEquals(_current, null))
                        return default(T);
                    return _current.value;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    if (ReferenceEquals(_current, null))
                        return default(T);
                    return _current.value;
                }
            }

            private LinkedListElement _current;

            public Enumerator(UTKLinkedList<T> list)
            {
                this.list = list;
                this._current = null;
                this.isInitialized = false;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (!isInitialized)
                {
                    this._current = list.first;
                    this.isInitialized = true;
                }
                else
                {
                    this._current = this._current.next;
                }

                return !ReferenceEquals(_current, null);
            }

            public void Reset()
            {
                isInitialized = false;
            }
        }

        /// <summary>
        /// Static list elements pool.
        /// TODO: Maybe an instance pooling mechnic (maybe even on top of this?) is more appropriate?
        /// </summary>
        private static ObjectPool<LinkedListElement> elementPool = new ObjectPool<LinkedListElement>(() => new LinkedListElement(), 10000);

        /// <summary>
        /// The first element in this linked list.
        /// </summary>
        public LinkedListElement first
        {
            get { return this._first; }
        }
        private LinkedListElement _first;

        /// <summary>
        /// The last element in this linked list.
        /// </summary>
        public LinkedListElement last
        {
            get { return this._last; }
        }
        private LinkedListElement _last;

        /// <summary>
        /// The amount of elements in this linked list.
        /// </summary>
        public int Count
        {
            get { return this._count; }
        }
        private int _count;

        /// <summary>
        /// The equality comparer for the linked list.
        /// </summary>
        private IEqualityComparer<T> equalityComparer;

        /// <summary>
        /// Always returns false.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        public UTKLinkedList()
        {
            this.equalityComparer = EqualityComparer<T>.Default;
        }

        public UTKLinkedList(IEqualityComparer<T> equalityComparer)
        {
            this.equalityComparer = equalityComparer;
        }

        /// <summary>
        /// Returns the <see cref="UTKLinkedList{T}"/> enumeration structure for this list.
        /// </summary>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// Adds the specified value as <see cref="LinkedListElement{T}"/> to the front of the linked list.
        /// Calling this will replace <see cref="first"/>
        /// </summary>
        /// <param name="item">The item to add</param>
        public void AddFirst(T item)
        {
            var element = elementPool.Get();
            element.prev = null;
            element.next = this._first;
            element.value = item;

            bool hasFirst = !ReferenceEquals(this._first, null);
            if (hasFirst)
                this._first.prev = element;

            if (hasFirst && ReferenceEquals(this._last, null))
                this._last = this._first;

            this._first = element;
            this._count++;
        }

        /// <summary>
        /// Adds the specified value as <see cref="LinkedListElement{T}"/> to the end of the linked list.
        /// Calling this will replace <see cref="last"/>
        /// </summary>
        /// <param name="item">The item to add</param>
        public void AddLast(T item)
        {
            var element = elementPool.Get();
            element.prev = this._last;
            element.next = null;
            element.value = item;

            if (!ReferenceEquals(this._last, null))
                this._last.next = element;
            if (ReferenceEquals(this._first, null))
                this._first = element;
            this._last = element;
            this._count++;
        }

        /// <summary>
        /// Inserts the specified value next to the specified element.
        /// </summary>
        /// <param name="element">The element after which to insert value.</param>
        /// <param name="value">The value to insert.</param>
        /// <param name="before">Whether or not to insert the specifiedd value before or after the specified element.</param>
        public void Insert(LinkedListElement element, T value, bool before = true)
        {
            var newElement = elementPool.Get();
            newElement.value = value;

            if (before)
            {
                // Insert before
                newElement.prev = element.prev;
                newElement.next = element;

                // Update entry point and elements
                if (ReferenceEquals(this._first, element))
                    this._first = newElement;
                else
                    element.prev.next = newElement;

                element.prev = newElement;
            }
            else
            {
                // Insert after
                newElement.prev = element;
                newElement.next = element.next;

                // Update entry point and elements
                if (ReferenceEquals(this._last, element))
                    this._last = newElement;
                else
                    element.next.prev = newElement;

                element.next = newElement;
            }
        }

        /// <summary>
        /// <see cref="AddLast(T)"/>
        /// </summary>
        /// <param name="item">The item to add</param>
        public void Add(T item)
        {
            AddLast(item);
        }

        /// <summary>
        /// Clears the linked list, releasing its nodes to the element pool.
        /// </summary>
        public void Clear()
        {
            LinkedListElement element = this.first;
            while (!ReferenceEquals(element, null))
            {
                elementPool.Return(element);
                element = element.next;
            }

            // Reset state
            this._first = null;
            this._last = null;
            this._count = 0;
        }

        public bool Contains(T item)
        {
            LinkedListElement element = this.first;
            while (!ReferenceEquals(element, null))
            {
                // Equality comparison
                if (this.equalityComparer.Equals(element.value, item))
                    return true;

                // Go to next element
                element = element.next;
            }

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            LinkedListElement element = this.first;
            for (int i = arrayIndex; i < array.Length; i++)
            {
                if (ReferenceEquals(element, null))
                    return;

                array[i] = element.value;

                // Go to next element
                element = element.next;
            }
        }

        /// <summary>
        /// Removes the specified element by "ripping" it out of the linked list.
        /// </summary>
        /// <param name="element">The element to remove.</param>
        public void RemoveElement(LinkedListElement element)
        {
            if (ReferenceEquals(element, null))
                throw new ArgumentException("Element to remove cannot be null!");

            // Update entry points
            if (ReferenceEquals(element, this._first))
                this._first = this._first.next;
            if (ReferenceEquals(element, this._last))
                this._last = this._last.prev;

            // Relink
            if (!ReferenceEquals(element.prev, null))
                element.prev.next = element.next;

            // Remove
            elementPool.Return(element, true);
            this._count--;
        }

        /// <summary>
        /// Removes the specified item from this linked list.
        /// This will remove *ALL* elements equal to item.
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <returns>Whether or not atleast one item was found in the collection and removed</returns>
        public bool Remove(T item)
        {
            bool found = false;
            LinkedListElement element = this.first;
            while (!ReferenceEquals(element, null))
            {
                // Equality comparison
                if (this.equalityComparer.Equals(element.value, item))
                {
                    found = true;
                    RemoveElement(element);
                }

                // Go to next element
                element = element.next;
            }

            return found;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Calls <see cref="Clear"/> in order to release all used elements for reuse (<see cref="elementPool"/>).
        /// </summary>
        public void Dispose()
        {
            Clear();
        }
    }
}