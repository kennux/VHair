using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK
{
    /// <summary>
    /// Datastructure tuple for <see cref="IGraph{TIndex, TNode, TConnectionData}"/>, TNode and ConnectionData.
    /// 
    /// Used on <see cref="IGraph{TIndex, TNode, TConnectionData}.GetConnectedNodes(TNode)"/>
    /// </summary>
    public struct ConnectedGraphNode<TNode, TConnectionData>
    {
        public TNode node;
        public TConnectionData data;

        public ConnectedGraphNode(TNode node, TConnectionData data)
        {
            this.node = node;
            this.data = data;
        }
    }
}
