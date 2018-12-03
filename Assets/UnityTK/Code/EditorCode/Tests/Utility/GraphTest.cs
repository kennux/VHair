using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityTK;
using System;
using System.Linq;

namespace UnityTK.Test.Utility
{
    public class GraphTest
    {
        public class TestNode : IComparable<TestNode>, IComparable
        {
            public readonly int index;
            public readonly int value;

            public TestNode(int index, int value)
            {
                this.index = index;
                this.value = value;
            }

            public int CompareTo(TestNode other)
            {
                return this.value.CompareTo(other.value);
            }

            public int CompareTo(object obj)
            {
                return this.value.CompareTo(((TestNode)obj).value);
            }

            public override int GetHashCode()
            {
                return Essentials.CombineHashCodes(this.index.GetHashCode(), this.value.GetHashCode());
            }
        }

        public struct TestConnectionData : IComparable<TestConnectionData>, IComparable
        {
            public float cost;

            public int CompareTo(TestConnectionData other)
            {
                return cost.CompareTo(other);
            }

            public int CompareTo(object obj)
            {
                return cost.CompareTo(obj);
            }

            public override string ToString()
            {
                return this.cost.ToString();
            }

            public override int GetHashCode()
            {
                return this.cost.GetHashCode();
            }
        }

        public struct DebugConnectionData<TNode, TConnectionData>
        {
            public TNode from;
            public TNode to;
            public TConnectionData data;

            public override int GetHashCode()
            {
                return Essentials.CombineHashCodes(Essentials.CombineHashCodes(from.GetHashCode(), to.GetHashCode()), data.GetHashCode());
            }
        }

        public static void TestGraph<TIndex, TNode, TConnectionData, TGraph>(TGraph graph, Func<int, TNode> nodeConstructor, Func<int, TIndex> indexConstructor, Func<TConnectionData> connectionDataConstructor, Func<TNode, TIndex> indexGetter) where TGraph : IGraph<TIndex, TNode, TConnectionData>, IEnumerable<TNode> where TNode : TestNode
        {
            // Generate 100 random test nodes
            List<TNode> nodes = new List<TNode>();
            for (int i = 0; i < 100; i++)
            {
                var node = nodeConstructor(i);
                nodes.Add(node);
                graph.Add(indexConstructor(i), node);
            }

			// Previous bug: Test if node with no connections casues exception
			var enumerator = graph.GetConnectedNodes(indexConstructor(0));
			enumerator.MoveNext();

            // Connect them at random
            List<DebugConnectionData<TNode, TConnectionData>> connections = new List<DebugConnectionData<TNode, TConnectionData>>();
            foreach (var node in nodes)
            {
                foreach (var node2 in nodes)
                {
                    if (UnityEngine.Random.value > 0.25f)
                    {
                        // Connect
                        var cData = connectionDataConstructor();

                        connections.Add(new DebugConnectionData<TNode, TConnectionData>()
                        {
                            from = node,
                            to = node2,
                            data = cData
                        });
                        graph.Connect(node, node2, cData);
                    }
                }
            }

            // Test getter
            for (int i = 0; i < 100; i++)
            {
                Assert.AreSame(nodes[i], graph.Get(indexConstructor(i)));
            }

            // Test connections
            foreach (var connection in connections)
            {
                TConnectionData connectionData;
                Assert.IsTrue(graph.IsConnected(connection.from, connection.to));
                Assert.IsTrue(graph.TryGetConnection(connection.from, connection.to, out connectionData));
                Assert.AreEqual(connection.data, connectionData);
            }

            // Test clear
            graph.Clear();
            for (int i = 0; i < nodes.Count; i++)
            {
                graph.Add(indexConstructor(i), nodes[i]);
            }

            foreach (var connection in connections)
            {
                graph.Connect(connection.from, connection.to, connection.data);
            }

            // Test getter
            for (int i = 0; i < 100; i++)
            {
                Assert.AreSame(nodes[i], graph.Get(indexConstructor(i)));
            }

            // Test connections
            foreach (var connection in connections)
            {
                TConnectionData connectionData;
                Assert.IsTrue(graph.IsConnected(connection.from, connection.to));
                Assert.IsTrue(graph.TryGetConnection(connection.from, connection.to, out connectionData));
                Assert.AreEqual(connection.data, connectionData);
            }

            // Test connection enumerations
            List<DebugConnectionData<TNode, TConnectionData>> _connections = new List<DebugConnectionData<TNode, TConnectionData>>();
            int ctr = 0;
            foreach (var node in nodes)
            {
                var cons = graph.GetConnectedNodes(indexConstructor(node.index));
                while (cons.MoveNext())
                {
                    _connections.Add(new DebugConnectionData<TNode, TConnectionData>()
                    {
                        data = cons.Current.data,
                        from = node,
                        to = cons.Current.node
                    });
                }

                ctr++;
            }

            // Prehash
            List<int> hashes = new List<int>();
            for (int i = 0; i < connections.Count; i++)
                hashes.Add(connections[i].GetHashCode());

            // Compare connections created vs GetConnectedNodes enumerator
            foreach (var con in _connections)
            {
                int hash = con.GetHashCode();

                bool found = false;
                
                for (int i = 0; i < connections.Count; i++)
                {
                    if (hash == hashes[i])
                    {
                        // Fetch element
                        var c = connections[i];
                        if (Comparer<TConnectionData>.Equals(con.data, c.data) && Comparer<TNode>.Equals(con.from, c.from) && Comparer<TConnectionData>.Equals(con.to, c.to))
                        {
                            found = true;
                            break;
                        }
                    }
                }

                Assert.IsTrue(found);
            }

            // Test connection override
            TNode tNode1 = nodes[UnityEngine.Random.Range(0, 99)];
            TNode tNode2 = nodes[UnityEngine.Random.Range(0, 99)];

            var cd = connectionDataConstructor();
            var cd2 = connectionDataConstructor();
            graph.Connect(tNode1, tNode2, cd);
            graph.Connect(tNode1, tNode2, cd2);

            TConnectionData rCd;
            Assert.IsTrue(graph.TryGetConnection(tNode1, tNode2, out rCd));
            Assert.AreEqual(cd2, rCd);

            // Pick 10 random nodes and remove them
            HashSet<TNode> removedNodes = new HashSet<TNode>();
            List<int> removedIndices = new List<int>();

            while (removedNodes.Count < 10)
            {
                int idx = UnityEngine.Random.Range(0, 99);
                if (removedNodes.Add(graph.Get(indexConstructor(idx))))
                {
                    removedIndices.Add(idx);
                }
            }

            // Remove nodes
            foreach (var node in removedNodes)
            {
                graph.Remove(indexGetter(node));
            }

            // Test if all connections to these nodes were cleaned up (removed)
            foreach (var connection in connections)
            {
                foreach (var node in removedNodes)
                {
                    Assert.IsFalse(graph.IsConnected(node, connection.to));
                }
            }

            // Generate 10 random test nodes
            nodes.RemoveAll((n) => removedNodes.Contains(n));
            for (int i = 0; i < 10; i++)
            {
                int idx = removedIndices[i];

                var node = nodeConstructor(idx);
                nodes.Add(node);
                graph.Add(indexConstructor(idx), node);
            }

            CollectionAssert.AreEqual(nodes.OrderBy((n) => n.value) , graph.OrderBy((n) => n.value));
        }

        /// <summary>
        /// Tests the standard graph implementations.
        /// </summary>
        [Test]
        public void TestStandardGraph()
        {
            var graph = new Graph<int, TestNode, TestConnectionData>();
            TestGraph<int, TestNode, TestConnectionData, Graph<int, TestNode, TestConnectionData>>(graph, (idx) => new TestNode(idx, UnityEngine.Random.Range(0, 100000)), (idx) => idx, () => new TestConnectionData()
            {
                cost = UnityEngine.Random.value
            }, (node) => node.index);
        }

        /// <summary>
        /// Tests the standard graph implementations.
        /// </summary>
        [Test]
        public void TestFastGraph()
        {
            var graph = new FastGraph<int, TestNode, TestConnectionData>(100, 100);
            var graph2 = new FastGraph<int, TestNode, TestConnectionData>(3, 1);

            TestGraph<int, TestNode, TestConnectionData, FastGraph<int, TestNode, TestConnectionData>>(graph, (idx) => new TestNode(idx, UnityEngine.Random.Range(0, 100000)), (idx) => idx, () => new TestConnectionData()
            {
                cost = UnityEngine.Random.value
            }, (node) => node.index);

            // Test memory management
            TestNode node1 = new TestNode(0, 1);
            TestNode node2 = new TestNode(1, 2);
            TestNode node3 = new TestNode(2, 3);

            TestConnectionData cData1 = new TestConnectionData()
            {
                cost = 0.5f
            };
            TestConnectionData cData2 = new TestConnectionData()
            {
                cost = 1f
            };

            graph2.Add(0, node1);
            graph2.Add(1, node2);
            graph2.Add(2, node3);

            graph2.Connect(node1, node2, cData1);

            bool exceptionThrown = false;
            try
            {
                graph2.Connect(node1, node3, cData2);
            }
            catch (OutOfMemoryException ex)
            {
                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown);

            exceptionThrown = false;
            try
            {
                graph2.Add(3, node1);
            }
            catch (OutOfMemoryException ex)
            {
                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown);
        }
    }
}