using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace UnityTK
{    /// <summary>
     /// Unity Parallelism Helper
     /// </summary>
    public static class Parallel
    {
        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <param name="iterations"></param>
        /// <param name="function"></param>
        public static void For(int iterations, Action<int> function)
        {
            int iterationsPassed = 0;
            ManualResetEvent resetEvent = new ManualResetEvent(false);

            for (int i = 0; i < iterations; i++)
            {
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    int currentIteration = (int)state;

                    try
                    {
                        function(currentIteration);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }

                    if (Interlocked.Increment(ref iterationsPassed) == iterations)
                        resetEvent.Set();
                }, i);
            }

            resetEvent.WaitOne();
        }

        /// <summary>
        /// Executes a foreach loop in which iterations may run in parallel.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="function"></param>
        public static void ForEach<T>(IEnumerable<T> collection, Action<T> function)
        {
            int iterations = 0;
            int iterationsPassed = 0;
            ManualResetEvent resetEvent = new ManualResetEvent(false);

            foreach (var item in collection)
            {
                Interlocked.Increment(ref iterations);
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    T subject = (T)state;

                    try
                    {
                        function(subject);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }

                    if (Interlocked.Increment(ref iterationsPassed) == iterations)
                        resetEvent.Set();

                }, item);
            }

            resetEvent.WaitOne();
        }
    }
}
