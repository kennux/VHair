using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK
{
    /// <summary>
    /// Implementation of <see cref="OrderedEventBase"/>
    /// </summary>
    public class OrderedEvent : OrderedEventBase<System.Action>
    {
        public void Invoke()
        {
            var sorted = GetSortedOrders();

            foreach (var order in sorted)
                foreach (var handler in this.handlers[order])
                    handler();

            ListPool<int>.Return(sorted);
        }
    }

    /// <summary>
    /// Implementation of <see cref="OrderedEventBase"/>
    /// </summary>
    public class OrderedEvent<T> : OrderedEventBase<System.Action<T>>
    {
        public void Invoke(T obj)
        {
            var sorted = GetSortedOrders();

            foreach (var order in sorted)
                foreach (var handler in this.handlers[order])
                    handler(obj);

            ListPool<int>.Return(sorted);
        }
    }

    /// <summary>
    /// Implementation of <see cref="OrderedEventBase"/>
    /// </summary>
    public class OrderedEvent<T, T2> : OrderedEventBase<System.Action<T, T2>>
    {
        public void Invoke(T obj, T2 obj2)
        {
            var sorted = GetSortedOrders();

            foreach (var order in sorted)
                foreach (var handler in this.handlers[order])
                    handler(obj, obj2);

            ListPool<int>.Return(sorted);
        }
    }

    /// <summary>
    /// Implementation of <see cref="OrderedEventBase"/>
    /// </summary>
    public class OrderedEvent<T, T2, T3> : OrderedEventBase<System.Action<T, T2, T3>>
    {
        public void Invoke(T obj, T2 obj2, T3 obj3)
        {
            var sorted = GetSortedOrders();

            foreach (var order in sorted)
                foreach (var handler in this.handlers[order])
                    handler(obj, obj2, obj3);

            ListPool<int>.Return(sorted);
        }
    }

    /// <summary>
    /// Implementation of <see cref="OrderedEventBase"/>
    /// </summary>
    public class OrderedEvent<T, T2, T3, T4> : OrderedEventBase<System.Action<T, T2, T3, T4>>
    {
        public void Invoke(T obj, T2 obj2, T3 obj3, T4 obj4)
        {
            var sorted = GetSortedOrders();

            foreach (var order in sorted)
                foreach (var handler in this.handlers[order])
                    handler(obj, obj2, obj3, obj4);

            ListPool<int>.Return(sorted);
        }
    }
}