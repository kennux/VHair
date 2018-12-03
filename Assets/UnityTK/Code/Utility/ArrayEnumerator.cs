using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityTK
{
    /// <summary>
    /// Array enumerator type.
    /// </summary>
    public struct ArrayEnumerator<T> : IEnumerator<T>
    {
        private T[] array;
        private int ptr;

        public readonly int start;
        public readonly int end;

        /// <summary>
        /// Creates a new enumerator.
        /// </summary>
        /// <param name="array">The array to use for enumeration.</param>
        /// <param name="start">The start index of the enumeration.</param>
        /// <param name="end">The last index of the enumeration.</param>
        public ArrayEnumerator(T[] array, int start = 0, int end = -1)
        {
            this.array = array;
            this.ptr = start-1;

            this.start = start;
            this.end = end == -1 ? array.Length : end;
        }

        public T Current
        {
            get
            {
                return this.array[this.ptr];
            }
        }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            this.ptr++;

            return this.ptr < this.end;
        }

        public void Reset()
        {
            this.ptr = this.start;
        }
    }
}
