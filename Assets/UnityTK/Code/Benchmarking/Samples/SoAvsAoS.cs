using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Benchmarking.Samples
{
    /// <summary>
    /// Sample benchmark comparing the structure of arrays memory layout model vs array of structures memory model.
    /// </summary>
    public class SoAvsAoS : Benchmark
    {
        private const int elementCount = 100000;
        private Vector3[] aos;
        private SoAStruct soa;

        private struct SoAStruct
        {
            public float[] x;
            public float[] y;
            public float[] z;

            public SoAStruct(int elementCount)
            {
                this.x = new float[elementCount];
                this.y = new float[elementCount];
                this.z = new float[elementCount];
            }
        }

        private Vector3 accum = new Vector3();
        private float accumX = 0;
        private float accumY = 0;
        private float accumZ = 0;

        protected override void Prepare()
        {
            this.aos = new Vector3[elementCount];
            this.soa = new SoAStruct(elementCount);

            // Randomize data
            for (int i = 0; i < elementCount; i++)
            {
                this.aos[i] = new Vector3(Random.value, Random.value, Random.value) * .1f;
                this.soa.x[i] = Random.value * .1f;
                this.soa.y[i] = Random.value * .1f;
                this.soa.z[i] = Random.value * .1f;
            }
        }

        protected override void RunBenchmark(BenchmarkResult bRes)
        {
            bRes.BeginLabel("AoS Accumulation");

            Vector3 v;
            for (int i = 0; i < elementCount; i++)
            {
                v = this.aos[i];
                this.accum.x += v.x;
                this.accum.y += v.y;
                this.accum.z += v.z;
            }

            bRes.EndLabel();
            bRes.BeginLabel("SoA Accumulation");

            for (int i = 0; i < elementCount; i++)
            {
                this.accumX += this.soa.x[i];
                this.accumY += this.soa.y[i];
                this.accumZ += this.soa.z[i];
            }

            bRes.EndLabel();
        }
    }
}