using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace VHair
{
    public class CPUTFXGlobalShapeConstraint_Burst : HairSimulationPass<CPUTFXPhysicsSimulation_Burst>
    {
		[BurstCompile]
		struct Job : IJobParallelFor
		{
			public float4x4 matrix;
			public float stiffness;
			public float timestep;
			public int vertexRange;
			
			[ReadOnly]
			public NativeArray<float3> initialVertices;
			[ReadOnly]
			public NativeArray<HairStrand> strands;
			[ReadOnly]
			public NativeArray<uint> movability;
			[NativeDisableParallelForRestriction]
			public NativeArray<float3> vertices;
			public void Execute(int j)
			{
                HairStrand strand = strands[j];
                int target = Mathf.Min(strand.firstVertex + this.vertexRange, strand.lastVertex);
                for (int i = strand.firstVertex; i <= target; i++)
                {
                    if (!HairMovability.IsMovable(i, movability))
                        continue;

                    float3 iV = initialVertices[i], v = vertices[i];
					float4 iV4 = new float4(iV.x, iV.y, iV.z, 1);
                    iV = math.mul(matrix, iV4).xyz;

                    float3 delta = (iV - v);
                    vertices[i] = v + (delta * this.stiffness * timestep);
                }
			}
		}

        public float stiffness = 0.3f;
        public int vertexRange = 3;

        public override void InitializeSimulation()
        {
        }

        protected override void _SimulationStep(float timestep)
        {
			Job job = new Job()
			{
				initialVertices = this.simulation.initialVertices,
				movability = this.simulation.movability,
				strands = this.simulation.strands,
				vertices = this.simulation.vertices,
				matrix = this.transform.localToWorldMatrix,
				stiffness = this.stiffness,
				vertexRange = this.vertexRange,
				timestep = timestep
			};

			this.simulation.jobHandle = job.Schedule(this.simulation.strands.Length, 16, this.simulation.jobHandle);
        }
    }
}