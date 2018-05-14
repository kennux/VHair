using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK
{
    /// <summary>
    /// Reusable boxing object.
    /// Can be used to avoid memory allocation when casting value types to objects.
    /// 
    /// This implements a generic box with <see cref="object.Equals(object)"/>, <see cref="object.GetHashCode"/> and <see cref="object.ToString"/> implementations forwarding the value type results.
    /// The disposable interface implemented on the box will return the box to the pool on dispose (<see cref="Dispose"/>).
    /// </summary>
    /// <typeparam name="T">The value type you want to box</typeparam>
    public class ReusableBox<T> : IDisposable, IEquatable<T> where T : struct, IEquatable<T>
    {
        /// <summary>
        /// The pool that can be used to retrieve boxes.
        /// </summary>
        private static ObjectPool<ReusableBox<T>> pool = new ObjectPool<ReusableBox<T>>(() => new ReusableBox<T>(), 1000, (box) => box.Reset());

        /// <summary>
        /// The value boxed into this reusable box.
        /// </summary>
        public T value;

        private ReusableBox()
        {
            this.value = default(T);
        }

        /// <summary>
        /// Returns an empty box instance drawn from a pool.
        /// The drawn box object must be disposed after usage, disposing it will return it into the object pool.
        /// </summary>
        /// <returns>An empty box with type T</returns>
        public static ReusableBox<T> GetBox()
        {
            return pool.Get();
        }

        /// <summary>
        /// Calls <see cref="GetBox"/> followed by assigning <see cref="value"/>.
        /// </summary>
        /// <param name="value">The value that needs to be boxed.</param>
        /// <returns>The boxed value</returns>
        public static ReusableBox<T> Box(T value)
        {
            var box = GetBox();
            box.value = value;
            return box;
        }

        /// <summary>
        /// Returns this box back to the <see cref="pool"/>
        /// </summary>
        public void Dispose()
        {
            pool.Return(this);
        }

        /// <summary>
        /// Internal reset method call that resets the boxed value.
        /// </summary>
        private void Reset()
        {
            this.value = default(T);
        }

        /// <summary>
        /// Implicit cast operator for casting the box back to <see cref="T"/>
        /// </summary>
        /// <param name="box">The box to unbox</param>
        public static implicit operator T(ReusableBox<T> box)
        {
            return box.value;
        }
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;

            // Depending on type we change how equality is determined
            if (obj is ReusableBox<T>)
                return this.Equals((obj as ReusableBox<T>).value);
            else if (obj is T)
                return this.Equals((T)obj);

            return false;
        }

        public bool Equals(T other)
        {
            return this.value.Equals(other);
        }

        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }

        public override string ToString()
        {
            return this.value.ToString();
        }
    }
}