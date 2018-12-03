using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK
{
    /// <summary>
    /// Interface defining basic functionality for graphs.
    /// </summary>
    /// <typeparam name="TNode">The type of the graph nodes.</typeparam>
    /// <typeparam name="TIndex">Indexer for the nodes.</typeparam>
    /// <typeparam name="TConnectionData">Data for connections</typeparam>
    public interface IGraph<TIndex, TNode, TConnectionData>
    {
        /// <summary>
        /// Adds a new node to this graph.
        /// </summary>
        /// <param name="index">The index of the node.</param>
        /// <param name="node">The node</param>
        void Add(TIndex index, TNode node);

        /// <summary>
        /// Looks up the node for the specified index.
        /// </summary>
        /// <param name="index">The index of the node to lookup.</param>
        /// <returns>The node</returns>
        TNode Get(TIndex index);

        /// <summary>
        /// Creates a directional connection from one node to another.
        /// </summary>
        /// <param name="from">The node where this connection originates at.</param>
        /// <param name="to">The node where this connection leads to.</param>
        void Connect(TNode from, TNode to, TConnectionData connectionData);

        /// <summary>
        /// Oppoiste of <see cref="Connect(TNode, TNode)"/>.
        /// </summary>
        /// <param name="from">The node the connection is originating on.</param>
        /// <param name="to">The node the connection is targeting.</param>
        void Disconnect(TNode from, TNode to);

        /// <summary>
        /// Determines whether or not the from node is connected to.
        /// </summary>
        /// <param name="from">The node the connection is originating on.</param>
        /// <param name="to">The node the connection is targeting.</param>
        /// <returns>Whether the connection is exsting.</returns>
        bool IsConnected(TNode from, TNode to);

        /// <summary>
        /// Tries to get connection data for connection from -> to.
        /// </summary>
        /// <param name="from">The origin node for the connection.</param>
        /// <param name="to">Where the connection is pointing to.</param>
        /// <param name="connectionData">The data of the connection</param>
        /// <returns>Whether or not the connection was found.</returns>
        bool TryGetConnection(TNode from, TNode to, out TConnectionData connectionData);

        /// <summary>
        /// Retrieves all connected nodes for the specified node.
        /// </summary>
        /// <param name="nodeIndex">The node index to look connections up for</param>
        /// <returns>An enumerable for iterating over all connected nodes.</returns>
        IEnumerator<ConnectedGraphNode<TNode, TConnectionData>> GetConnectedNodes(TIndex nodeIndex);

        /// <summary>
        /// Removes the node for the specified index from this graph.
        /// </summary>
        /// <param name="index">The index of the node to be removed.</param>
        void Remove(TIndex index);

        /// <summary>
        /// Clears the graph by destroying all data stored in it.
        /// </summary>
        void Clear();
    }
}