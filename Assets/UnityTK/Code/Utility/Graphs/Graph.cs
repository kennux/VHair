using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK
{
    /// <summary>
    /// Basic generic graph implementation.
    /// Nothing fancy here, just a simple and naive implementation of <see cref="IGraph"/>.
    /// 
    /// Underlying implementation just uses 2 <see cref="Dictionary{TKey, TValue}"/> for nodes.
    /// It uses a dictionary with a list of tuples (index, connection data) for connections.
    /// </summary>
    public class Graph<TIndex, TNode, TConnectionData> : IGraph<TIndex, TNode, TConnectionData>, IEnumerable<TNode>
    {
        /// <summary>
        /// All nodes of this graph.
        /// </summary>
        private Dictionary<TIndex, TNode> nodes = new Dictionary<TIndex, TNode>();

        /// <summary>
        /// Node reverse mappings.
        /// </summary>
        private Dictionary<TNode, TIndex> nodesReverse = new Dictionary<TNode, TIndex>();

        /// <summary>
        /// Connections dictionary
        /// </summary>
        private Dictionary<TIndex, List<ConnectedGraphNode<TNode, TConnectionData>>> connections = new Dictionary<TIndex, List<ConnectedGraphNode<TNode, TConnectionData>>>();

		private readonly List<ConnectedGraphNode<TNode, TConnectionData>> emptyConnectionsList = new List<ConnectedGraphNode<TNode, TConnectionData>>();

        public void Add(TIndex index, TNode node)
        {
            this.nodes.Add(index, node);
            this.nodesReverse.Add(node, index);
        }

        public TNode Get(TIndex index)
        {
            return this.nodes[index];
        }

        public void Remove(TIndex index)
        {
            TNode node;

            if (this.nodes.TryGetValue(index, out node))
            {
                this.nodes.Remove(index);
                this.nodesReverse.Remove(node);

                List<ConnectedGraphNode<TNode, TConnectionData>> lst;
                if (this.connections.TryGetValue(index, out lst))
                {
                    this.connections.Remove(index);
                    ListPool<ConnectedGraphNode<TNode, TConnectionData>>.Return(lst);
                }
            }
        }

        /// <summary>
        /// Finds a connection index in the specified list of connection datas.
        /// </summary>
        /// <returns>Index in connection data, -1 if not found</returns>
        private int FindConnectionIndex(List<ConnectedGraphNode<TNode, TConnectionData>> connectionData, TNode node, out ConnectedGraphNode<TNode, TConnectionData> data)
        {
            var cmp = EqualityComparer<TNode>.Default;

            for (int i = 0; i < connectionData.Count; i++)
            {
                var cData = connectionData[i];
                if (cmp.Equals(cData.node, node))
                {
                    data = cData;
                    return i;
                }
            }

            data = default(ConnectedGraphNode<TNode, TConnectionData>);
            return -1;
        }

        /// <summary>
        /// Returns the connections from node to any other nodes.
        /// </summary>
        private List<ConnectedGraphNode<TNode, TConnectionData>> GetConnections(TNode node, bool createIfNotExisting = true)
        {
            // Look up index
            TIndex index;
            if (!this.nodesReverse.TryGetValue(node, out index))
                return null;

            List<ConnectedGraphNode<TNode, TConnectionData>> cons;
            if (!this.connections.TryGetValue(index, out cons) && createIfNotExisting)
            {
                cons = ListPool<ConnectedGraphNode<TNode, TConnectionData>>.Get();
                this.connections.Add(index, cons);
            }

            return cons;
        }

        public void Connect(TNode from, TNode to, TConnectionData connectionData)
        {
            // Look up
            var cons = GetConnections(from);

            // Check for connection
            ConnectedGraphNode<TNode, TConnectionData> cData;
            var conId = FindConnectionIndex(cons, to, out cData);
            
            if (conId == -1)
                cons.Add(new ConnectedGraphNode<TNode, TConnectionData>()
                {
                    data = connectionData,
                    node = to
                });
            else
            {
                var con = cons[conId];
                con.data = connectionData;
                cons[conId] = con;
            }
        }

        public void Disconnect(TNode from, TNode to)
        {
            // Look up
            var cons = GetConnections(from);

            // Check for connection
            ConnectedGraphNode<TNode, TConnectionData> cData;
            var conId = FindConnectionIndex(cons, to, out cData);

            if (conId != -1)
            {
                cons.RemoveAt(conId);
            }
        }

        public bool IsConnected(TNode from, TNode to)
        {
            TConnectionData data;
            return TryGetConnection(from, to, out data);
        }

        public bool TryGetConnection(TNode from, TNode to, out TConnectionData connectionData)
        {
            // Look up
            List<ConnectedGraphNode<TNode, TConnectionData>> cons = GetConnections(from, false);
            if (ReferenceEquals(cons, null))
            {
                connectionData = default(TConnectionData);
                return false;
            }

            // Check for connection
            ConnectedGraphNode<TNode, TConnectionData> cData;
            var conId = FindConnectionIndex(cons, to, out cData);

            if (conId == -1)
            {
                connectionData = default(TConnectionData);
                return false;
            }
            else
            {
                connectionData = cons[conId].data;
                return true;
            }
        }

        public void Clear()
        {
            this.nodes.Clear();
            this.nodesReverse.Clear();
            this.connections.Clear();
        }

        public Dictionary<TIndex, TNode>.ValueCollection.Enumerator GetEnumerator()
        {
            return this.nodes.Values.GetEnumerator();
        }

        IEnumerator<TNode> IEnumerable<TNode>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public List<ConnectedGraphNode<TNode, TConnectionData>>.Enumerator GetConnectedNodes(TIndex nodeIndex)
        {
            // Get connections data list
            List<ConnectedGraphNode<TNode, TConnectionData>> lst;
            if (this.connections.TryGetValue(nodeIndex, out lst))
                return lst.GetEnumerator();

            // :(
            return emptyConnectionsList.GetEnumerator();
        }
        
        IEnumerator<ConnectedGraphNode<TNode, TConnectionData>> IGraph<TIndex, TNode, TConnectionData>.GetConnectedNodes(TIndex nodeIndex)
        {
            return GetConnectedNodes(nodeIndex);
        }
    }
}