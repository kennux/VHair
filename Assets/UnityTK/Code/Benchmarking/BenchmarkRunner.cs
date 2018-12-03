using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityTK.Benchmarking
{
    /// <summary>
    /// <see cref="Benchmark"/> runner implementation.
    /// </summary>
    public static class BenchmarkRunner
    {
        private const string gameObjectName = "_UnityTK_Benchmark_";

        /// <summary>
        /// Runs the specified benchmarks.
        /// </summary>
        /// <param name="benchmarks">The <see cref="Benchmark"/> implementations to be benchmarked.</param>
        /// <param name="preAlloc">Pre-allocated list for resoruce set. Optional, will be drawn from pool if not specified.</param>
        public static List<BenchmarkResult> RunBenchmark(List<Type> benchmarks, List<BenchmarkResult> preAlloc = null)
        {
            ListPool<BenchmarkResult>.GetIfNull(ref preAlloc);

            // Load temporary scene
            // TODO

            // Create GO for all benchmarks
            GameObject benchmarksGo = new GameObject(gameObjectName);

            try
            {
                foreach (var type in benchmarks)
                    benchmarksGo.AddComponent(type);

                RunBenchmark(benchmarksGo, preAlloc);
            }
            finally
            {
                GameObject.DestroyImmediate(benchmarksGo);
            }

            // Reload old scene setup
            // TODO

            return preAlloc;
        }

        /// <summary>
        /// Runs all <see cref="Benchmark"/> on the benchmarksGo.
        /// Stores the results in preAlloc.
        /// </summary>
        /// <param name="benchmarks">The benchmarks gameobject.</param>
        /// <param name="preAlloc">preAllocated result list. Will be drawn from pool if null.</param>
        /// <returns>preAlloc</returns>
        public static List<BenchmarkResult> RunBenchmark(GameObject benchmarks, List<BenchmarkResult> preAlloc = null)
        {
            ListPool<BenchmarkResult>.GetIfNull(ref preAlloc);
            List<Benchmark> lst = ListPool<Benchmark>.Get();

            var test = benchmarks.GetComponents<MonoBehaviour>();
            benchmarks.GetComponents<Benchmark>(lst);
            foreach (var benchmark in lst)
            {
                preAlloc.Add(benchmark.Run());
            }

            ListPool<Benchmark>.Return(lst);
            return preAlloc;
        }
    }
}
