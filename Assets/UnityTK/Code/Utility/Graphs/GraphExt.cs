using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK
{
    /// <summary>
    /// Static graph extensions.
    /// </summary>
    public static class GraphExt
    {
        /// <summary>
        /// <see cref="IGraph{TIndex, TNode, TConnectionData}.Connect(TNode, TNode, TConnectionData)"/>
        /// </summary>
        public static void Connect<TIndex, TNode, TConnectionData>(this IGraph<TIndex, TNode, TConnectionData> graph, TNode from, TIndex to, TConnectionData data)
        {
            graph.Connect(from, graph.Get(to), data);
        }

        /// <summary>
        /// <see cref="IGraph{TIndex, TNode, TConnectionData}.Connect(TNode, TNode, TConnectionData)"/>
        /// </summary>
        public static void Connect<TIndex, TNode, TConnectionData>(this IGraph<TIndex, TNode, TConnectionData> graph, TIndex from, TIndex to, TConnectionData data)
        {
            graph.Connect(graph.Get(from), graph.Get(to), data);
        }

        /// <summary>
        /// <see cref="IGraph{TIndex, TNode, TConnectionData}.Connect(TNode, TNode, TConnectionData)"/>
        /// </summary>
        public static void Connect<TIndex, TNode, TConnectionData>(this IGraph<TIndex, TNode, TConnectionData> graph, TIndex from, TNode to, TConnectionData data)
        {
            graph.Connect(graph.Get(from), to, data);
        }
    }
}