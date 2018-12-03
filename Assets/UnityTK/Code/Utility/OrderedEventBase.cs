using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK
{
    /// <summary>
    /// "Ordered" event implementation base class.
    /// 
    /// This class differs from regular events in the handler execution order.
    /// Instead of just registering handlers a handler can be registered with handler and order.
    /// 
    /// The order is represented as 32 bit signed integer.
    /// Whenever a handler is registered its inserted in sorted fashion into the internal handler queue.
    /// 
    /// When the event is invoked handlers are invoked in ascending sorted order.
    /// </summary>
    public class OrderedEventBase<T>
    {
        /// <summary>
        /// The internal handler dictionary.
        /// </summary>
        protected Dictionary<int, List<T>> handlers = new Dictionary<int, List<T>>();

        /// <summary>
        /// Returns the handler list for the specified order.
        /// </summary>
        private List<T> GetHandlers(int order)
        {
            List<T> lst = null;
            if (!handlers.TryGetValue(order, out lst))
            {
                lst = new List<T>();
                handlers.Add(order, lst);
            }

            return lst;
        }

        /// <summary>
        /// Binds the specified handler with the specified order on this event.
        /// </summary>
        public void Bind(T handler, int order)
        {
            GetHandlers(order).Add(handler);
        }

        /// <summary>
        /// Unbinds the specified handler from this event.
        /// </summary>
        /// <param name="handler">The handler to unbind.</param>
        public void Unbind(T handler)
        {
            foreach (var lst in this.handlers.Values)
                lst.Remove(handler);
        }

        /// <summary>
        /// Returns all the <see cref="handlers"/> order queus in ascendingly sorted order.
        /// </summary>
        /// <param name="preAlloc">Pre-allocated list for storing the result.</param>
        /// <returns>A sorted list with all distinct order in this event.</returns>
        protected virtual List<int> GetSortedOrders(List<int> preAlloc = null)
        {
            ListPool<int>.GetIfNull(ref preAlloc);

            // Accumulate
            foreach (var order in handlers.Keys)
                preAlloc.Add(order);

            // Sort
            Essentials.InsertionSort(preAlloc);
            return preAlloc;
        }
    }
}