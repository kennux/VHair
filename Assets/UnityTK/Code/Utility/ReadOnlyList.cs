using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK
{
    /// <summary>
    /// Read-only wrapper around a <see cref="List{T}"/>.
    /// </summary>
    /// <typeparam name="T">Element type of the list.</typeparam>
    public struct ReadOnlyList<T> : IEnumerable<T>
    {
        /// <summary>
        /// Internal list reference, the warpped list of this readonly list.
        /// </summary>
        private List<T> list;

        /// <summary>
        /// The count of elements in the wrapped list.
        /// <see cref="list"/>
        /// </summary>
        public int count
        {
            get { return this.list.Count; }
        }

        /// <summary>
        /// Array accessor for the wrapped <see cref="list"/>.
        /// Only read access.
        /// </summary>
        /// <param name="index">The index to access</param>
        /// <returns>The value at the index</returns>
        public T this[int index]
        {
            get { return this.list[index]; }
        }

        /// <summary>
        /// Constructs a new <see cref="IReadOnlyList{T}"/>
        /// </summary>
        /// <param name="list">The list to wrap.</param>
        public ReadOnlyList(List<T> list)
        {
            this.list = list;
        }
        
        public List<T>.Enumerator GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.list.GetEnumerator();
        }
    }
}