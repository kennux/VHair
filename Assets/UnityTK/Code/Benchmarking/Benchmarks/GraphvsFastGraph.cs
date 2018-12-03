using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Benchmarking.Samples
{
    /// <summary>
    /// Sample benchmark benchmarking the 2 <see cref="IGraph{TIndex, TNode, TConnectionData}"/> implementations:
    /// - <see cref="Graph{TIndex, TNode, TConnectionData}"/>
    /// - <see cref="FastGraph{TIndex, TNode, TConnectionData}"/>
    /// </summary>
    public class GraphvsFastGraph : Benchmark
    {
        const int NODECOUNT = 500000;
        const int CONNECTIONSMAX = 10;
        private Graph<int, float, float> graph;
        private FastGraph<int, float, float> fastGraph;

        protected override void Prepare()
        {
            // Prep graphs
            this.graph = new Graph<int, float, float>();
            this.fastGraph = new FastGraph<int, float, float>(NODECOUNT, CONNECTIONSMAX);

            // Generate random test nodes
            List<float> nodes = new List<float>();
            for (int i = 0; i < NODECOUNT; i++)
            {
                var v = 1f / (float)i;
                nodes.Add(v);
                graph.Add(i, v);
                fastGraph.Add(i, v);
            }

            // Connect them at random
            int ctr = 0;
            foreach (var node in nodes)
            {
                for (int i = 0; i < NODECOUNT; i++)
                {
                    if (Random.value > 0.25f)
                    {
                        // Connect
                        graph.Connect(node, graph.Get(i), Random.value);
                        fastGraph.Connect(node, graph.Get(i), Random.value);

                        ctr++;
                        if (ctr >= CONNECTIONSMAX)
                            break;
                    }
                }
            }
        }

        protected override void RunBenchmark(BenchmarkResult bRes)
        {
            bRes.BeginLabel("Graph");
            
            // Benchmark node iteration
            bRes.BeginLabel("Node Iteration");

            float v = 0;
            foreach (var node in graph)
            {
                v += node;
            }

            bRes.EndLabel();

            // Benchmark node connection iteration
            bRes.BeginLabel("Node Connection Iteration");

            v = 0;
            for (int i = 0; i < NODECOUNT; i++)
            {
                var enumerator = graph.GetConnectedNodes(i);
                while (enumerator.MoveNext())
                {
                    v += enumerator.Current.data;
                }
            }

            bRes.EndLabel();

            bRes.EndLabel();
            
            bRes.BeginLabel("FastGraph");

            // Benchmark node iteration
            bRes.BeginLabel("Node Iteration");

            v = 0;
            foreach (var node in fastGraph)
            {
                v += node;
            }

            bRes.EndLabel();

            // Benchmark node connection iteration
            bRes.BeginLabel("Node Connection Iteration");

            v = 0;
            for (int i = 0; i < NODECOUNT; i++)
            {
                var enumerator = fastGraph.GetConnectedNodes(i);
                while (enumerator.MoveNext())
                {
                    v += enumerator.Current.data;
                }
            }

            bRes.EndLabel();

            bRes.EndLabel();
        }
    }
}