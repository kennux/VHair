using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK.Benchmarking;

namespace UnityTK.Editor.Benchmarking
{
    /// <summary>
    /// Example benchmark for UnityTK benchmarking
    /// </summary>
    public class UTKLinkedListBenchmark : Benchmark
    {
        private UTKLinkedList<int> linkedList;

        protected override void Prepare()
        {
            this.linkedList = new UTKLinkedList<int>();
        }

        protected override void RunBenchmark(BenchmarkResult bRes)
        {
            bRes.BeginLabel("10k adds");

            for (int i = 0; i < 10000; i++)
            {
                this.linkedList.Add(i);
            }

            bRes.EndLabel();

            bRes.BeginLabel("10k first removals");

            for (int i = 0; i < 10000; i++)
            {
                this.linkedList.RemoveElement(this.linkedList.first);
            }

            bRes.EndLabel();

            bRes.BeginLabel("10k adds");

            for (int i = 0; i < 10000; i++)
            {
                this.linkedList.Add(i);
            }

            bRes.EndLabel();

            bRes.BeginLabel("10k last removals");

            for (int i = 0; i < 10000; i++)
            {
                this.linkedList.RemoveElement(this.linkedList.last);
            }

            bRes.EndLabel();
        }
    }
}