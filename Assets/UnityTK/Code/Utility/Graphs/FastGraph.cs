using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK
{
    /// <summary>
    /// An optimized / fast implementation of <see cref="IGraph{TIndex, TNode, TConnectionData}"/>.
    /// This implementation is unlike <see cref="Graph{TIndex, TNode, TConnectionData}"/> not creating much objects on the heap and aims for linear memory layout.
    /// 
    /// Nodes and connections are stored in arrays with pre-defined sizes.
    /// This implies these limitations:
    /// - Graph has pre-defined amount of nodes (no dynamic resize :( )
    /// - Nodes can only have a pre-defined amount of connections
    /// 
    /// Use this graph basically for whenever you need maximized performance and dont care about memory consumption.
    /// There are a few caches kept internally to speed up look ups which consume more memory in order to tradeoff for performance.
    /// 
    /// For a better solution memory-wise, use <see cref="Graph{TIndex, TNode, TConnectionData}"/>
    /// </summary>
    public class FastGraph<TIndex, TNode, TConnectionData> : IGraph<TIndex, TNode, TConnectionData>, IEnumerable<TNode>
    {
        /// <summary>
        /// All nodes currently in the graph.
        /// </summary>
        private TNode[] nodes;

        /// <summary>
        /// Analogous to <see cref="nodes"/>.
        /// </summary>
        private TIndex[] indices;

        /// <summary>
        /// Connection counters for <see cref="nodes"/>
        /// </summary>
        private int[] nodeConnectionCounter;

        /// <summary>
        /// Connections from node to nodes.
        /// </summary>
        private ConnectedGraphNode<TNode, TConnectionData>[] connections;

        /// <summary>
        /// Maximum amount of connections per node.
        /// </summary>
        private int maxConnections;

        /// <summary>
        /// Lookup for looking up node index to index in <see cref="nodes"/>
        /// </summary>
        private Dictionary<TIndex, int> indexMap = new Dictionary<TIndex, int>();

        /// <summary>
        /// Lookup for looking up nodes to index in <see cref="nodes"/>
        /// </summary>
        private Dictionary<TNode, int> nodeMap = new Dictionary<TNode, int>();

        /// <summary>
        /// Insertion pointer for <see cref="nodes"/>.
        /// 
        /// If <see cref="nodes"/>.Length, the arrays are full and <see cref="freeIndices"/> will be used.
        /// </summary>
        private int nodePtr = 0;
        private Queue<int> freeIndices = new Queue<int>();

        #region Memory management

        /// <summary>
        /// Returns the index base for connections of nodes.
        /// This returns nodeIndex * <see cref="maxConnections"/>
        /// </summary>
        /// <param name="nodeIndex">The node index to retrieve connection index base for.</param>
        /// <returns>The connections index base for <see cref="connections"/> and <see cref="connectionDatas"/></returns>
        private int GetConnectionIndexBase(int nodeIndex)
        {
            return nodeIndex * this.maxConnections;
        }

        /// <summary>
        /// Resizes this graph.
        /// 
        /// This will resize all internal arrays.
        /// </summary>
        /// <param name="nodeCount">Max amount of nodes to be stored in this graph.</param>
        /// <param name="maxConnectionsPerNode">The maximum amount of connections a node can have.</param>
        /// <param name="cleanReallocation">A "clean" reallocation will just reallocate the internal data arrays, dropping all data currently in the graph.</param>
        public void Resize(int nodeCount, int maxConnectionsPerNode, bool cleanReallocation = false)
        {
            this.maxConnections = maxConnectionsPerNode;

            if (cleanReallocation)
            {
                // Realloc
                this.nodes = new TNode[nodeCount];
                this.indices = new TIndex[nodeCount];
                this.nodeConnectionCounter = new int[nodeCount];
                this.connections = new ConnectedGraphNode<TNode, TConnectionData>[nodeCount * maxConnectionsPerNode];

                this.indexMap.Clear();
                this.nodeMap.Clear();
                this.nodePtr = 0;
            }
            else
            {
                // Resize
                System.Array.Resize(ref this.nodes, nodeCount);
                System.Array.Resize(ref this.indices, nodeCount);
                System.Array.Resize(ref this.nodeConnectionCounter, nodeCount);
                System.Array.Resize(ref this.connections, nodeCount * maxConnectionsPerNode);
            }
        }

        /// <summary>
        /// Returns a free index for a new element.
        /// </summary>
        /// <returns>Index in <see cref="nodes"/></returns>
        private int GetFreeIndex()
        {
            if (this.nodePtr == this.nodes.Length)
            {
                if (this.freeIndices.Count == 0)
                    throw new System.OutOfMemoryException("Graph ran out of storage space!");

                return this.freeIndices.Dequeue();
            }

            return this.nodePtr++;
        }

        /// <summary>
        /// Finds connection from -> to.
        /// </summary>
        /// <param name="from">The node from which to find a connection.</param>
        /// <param name="to">The node for which to find a connection to.</param>
        /// <returns>The index in <see cref="connections"/>, -1 if not found.</returns>
        private int FindConnection(TNode from, TNode to, out int indexBase)
        {
            // Lookup node
            int idxFrom;
            if (!this.nodeMap.TryGetValue(from, out idxFrom))
            {
                indexBase = -1;
                return -1;
            }

            // Look for connection
            indexBase = GetConnectionIndexBase(idxFrom);
            int cIdxEnd = indexBase + this.maxConnections;
            var cmp = EqualityComparer<TNode>.Default;

            for (int i = indexBase; i < cIdxEnd; i++)
            {
                if (cmp.Equals(this.connections[i].node, to))
                {
                    return i;
                }
            }

            return -1;
        }

        #endregion

        /// <summary>
        /// <see cref="Resize(int, int)"/>
        /// </summary>
        /// <param name="nodeCount">Max amount of nodes to be stored in this graph.</param>
        /// <param name="maxConnectionsPerNode">The maximum amount of connections a node can have.</param>
        public FastGraph(int nodeCount, int maxConnectionsPerNode)
        {
            Resize(nodeCount, maxConnectionsPerNode, true);
        }

        public void Add(TIndex index, TNode node)
        {
            int idx = GetFreeIndex();

            // Insert
            this.nodes[idx] = node;
            this.indices[idx] = index;
            this.nodeConnectionCounter[idx] = 0;
            System.Array.Clear(this.connections, this.GetConnectionIndexBase(idx), this.maxConnections);
            this.indexMap.Add(index, idx);
            this.nodeMap.Add(node, idx);
        }

        public TNode Get(TIndex index)
        {
            int idx;
            if (!this.indexMap.TryGetValue(index, out idx))
                return default(TNode);

            return this.nodes[idx];
        }

        public void Remove(TIndex index)
        {
            int idx;
            if (this.indexMap.TryGetValue(index, out idx))
            {
                this.nodeMap.Remove(this.nodes[idx]);
                this.indexMap.Remove(index);
                this.nodes[idx] = default(TNode);
                this.indices[idx] = default(TIndex);
                this.nodeConnectionCounter[idx] = 0;
                System.Array.Clear(this.connections, this.GetConnectionIndexBase(idx), this.maxConnections);

                this.freeIndices.Enqueue(idx);
            }
        }
        

        public void Connect(TNode from, TNode to, TConnectionData connectionData)
        {
            // Lookup nodes
            int idxFrom, idxTo;
            if (!this.nodeMap.TryGetValue(from, out idxFrom))
                throw new KeyNotFoundException("Could not find node from");
            if (!this.nodeMap.TryGetValue(to, out idxTo))
                throw new KeyNotFoundException("Could not find node to");

            int idxBase;
            int c = FindConnection(from, to, out idxBase);
            if (c != -1)
            {
                // Update existing
                this.connections[c] = new ConnectedGraphNode<TNode, TConnectionData>(to, connectionData);
                return;
            }

            // Get connection pointer
            int cPtr = this.nodeConnectionCounter[idxFrom];
            if (cPtr >= this.maxConnections)
                throw new System.OutOfMemoryException("Cannot add any more connections to " + from + " - already has max connections!");

            // Calculate connection index and write connection data
            int cIdx = idxBase + cPtr;
            this.connections[cIdx] = new ConnectedGraphNode<TNode, TConnectionData>(this.nodes[idxTo], connectionData);
            this.nodeConnectionCounter[idxFrom] = cPtr + 1;
        }

        public void Disconnect(TNode from, TNode to)
        {
            // Lookup node
            int idxFrom;
            if (!this.nodeMap.TryGetValue(from, out idxFrom))
                throw new KeyNotFoundException("Could not find node from");

            // Lookup connection
            int indexBase;
            int cIdx = FindConnection(from, to, out indexBase);
            if (cIdx == -1)
                return;

            int idxEnd = indexBase + this.nodeConnectionCounter[idxFrom] - 1;
            for (int i = cIdx; i < idxEnd; i++)
            {
                this.connections[i] = this.connections[i + 1];
            }

            this.nodeConnectionCounter[idxFrom]--;
        }

        public bool IsConnected(TNode from, TNode to)
        {
            TConnectionData data;
            return TryGetConnection(from, to, out data);
        }

        public bool TryGetConnection(TNode from, TNode to, out TConnectionData connectionData)
        {
            int indexBase;
            int cIdx = FindConnection(from, to, out indexBase);

            if (cIdx == -1)
            {
                connectionData = default(TConnectionData);
                return false;
            }
            else
            {
                connectionData = this.connections[cIdx].data;
                return true;
            }
        }

        public void Clear()
        {
            Resize(this.nodes.Length, this.maxConnections, true);
        }

        #region Enumerations

        public ArrayEnumerator<TNode> GetEnumerator()
        {
            return new ArrayEnumerator<TNode>(this.nodes);
        }

        IEnumerator<TNode> IEnumerable<TNode>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Enumerator structure for <see cref="GetConnectedNodes(TIndex)"/>.
        /// </summary>
        public struct ConnectedNodesEnumerator : IEnumerator<ConnectedGraphNode<TNode, TConnectionData>>
        {

            public ConnectedGraphNode<TNode, TConnectionData> Current
            {
                get
                {
                    return this.connections[this.ptr];
                }
            }

            object IEnumerator.Current => Current;

            private int ptr;
            private int start;
            private int end;
            private ConnectedGraphNode<TNode, TConnectionData>[] connections;

            public ConnectedNodesEnumerator(int ptr, int start, int end, ConnectedGraphNode<TNode, TConnectionData>[] connections)
            {
                this.ptr = ptr;
                this.start = start;
                this.end = end;
                this.connections = connections;
            }

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
                this.ptr = start;
            }
        }

        public ConnectedNodesEnumerator GetConnectedNodes(TIndex nodeIndex)
        {
            int idx = this.indexMap[nodeIndex];
            int start = GetConnectionIndexBase(idx);
            int end = start + this.nodeConnectionCounter[idx];

            return new ConnectedNodesEnumerator(start, start, end, this.connections);
        }

        IEnumerator<ConnectedGraphNode<TNode, TConnectionData>> IGraph<TIndex, TNode, TConnectionData>.GetConnectedNodes(TIndex nodeIndex)
        {
            return GetConnectedNodes(nodeIndex);
        }

        #endregion
    }
}