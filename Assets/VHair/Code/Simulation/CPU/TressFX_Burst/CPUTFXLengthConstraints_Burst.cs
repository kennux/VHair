using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace VHair
{
    /// <summary>
    /// CPU implementation of a length constraint algorithm.
    /// 
    /// </summary>
    public class CPUTFXLengthConstraints_Burst : HairSimulationPass<CPUTFXPhysicsSimulation_Burst>
    {
		[BurstCompile]
		struct Job : IJobParallelFor
		{
			public float stiffness;
			public float timestep;
			
			[ReadOnly]
			public NativeArray<float> lengths;
			[ReadOnly]
			public NativeArray<HairStrand> strands;
			[NativeDisableParallelForRestriction]
			public NativeArray<float3> vertices;
			[ReadOnly]
			public NativeArray<uint> movability;

			public void Execute(int index)
			{
				HairStrand strand = this.strands[index];
                for (int j = strand.firstVertex + 1; j <= strand.lastVertex; j++)
                {
                    if (!HairMovability.IsMovable(j, movability))
                        continue;

                    float nDist = this.lengths[j];
                    float3 p = vertices[j], pPrev = vertices[j - 1], pDir = (p - pPrev);
                    float dist = math.length(pDir);
                    float distDiff = (nDist - dist);

                    vertices[j] = p + ((pDir / dist) * (distDiff * this.stiffness * timestep));
                }
			}
		}

        public float stiffness = 1;

        private NativeArray<float> lengths;

        public override void InitializeSimulation()
        {
            this.lengths = new NativeArray<float>(this.instance.vertexCount, Allocator.Persistent);
            NativeArray<HairStrand> strands = this.instance.strands.CpuReference;
			NativeArray<float3> vertices = this.instance.vertices.CpuReference;

            for (int s = 0; s < strands.Length; s++)
            {
                HairStrand strand = strands[s];
                for (int j = strand.firstVertex+1; j < strand.lastVertex; j++)
                {
                    this.lengths[j] = math.distance(vertices[j], vertices[j - 1]);
                }
            }
        }

        protected override void _SimulationStep(float timestep)
        {
			Job job = new Job()
			{
				lengths = this.lengths,
				movability = this.simulation.movability,
				strands = this.simulation.strands,
				vertices = this.simulation.vertices,
				stiffness = this.stiffness,
				timestep = timestep
			};

			this.simulation.jobHandle = job.Schedule(this.simulation.strands.Length, 32, this.simulation.jobHandle);
        }

		public void OnDestroy()
		{
			this.lengths.Dispose();
		}
	}
}