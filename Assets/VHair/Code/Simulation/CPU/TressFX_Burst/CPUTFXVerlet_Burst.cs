using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace VHair
{
	public class CPUTFXVerlet_Burst : HairSimulationPass<CPUTFXPhysicsSimulation_Burst>
	{
		[BurstCompile]
		struct Job : IJobParallelFor
		{
			public float4x4 matrix;
			public float4x4 invPrevMatrix;
			public float3 gravity;
			public float timestepSqr;

			[ReadOnly]
			public NativeArray<HairStrand> strands;
			[ReadOnly]
			public NativeArray<float3> initialVertices;
			[NativeDisableParallelForRestriction]
			public NativeArray<float3> vertices;
			[ReadOnly]
			public NativeArray<uint> movability;

			public void Execute(int index)
			{
				HairStrand strand = this.strands[index];
				float3 lastFramePosWS, lastFramePosOS, posWS, newPos;
				lastFramePosOS = lastFramePosWS = posWS = newPos = new float3();

				// First vertex is immovable
				float3 initialPos = initialVertices[strand.firstVertex];
				vertices[strand.firstVertex] = math.mul(matrix, new float4(initialPos.x, initialPos.y, initialPos.z,1)).xyz;

				for (int i = strand.firstVertex; i <= strand.lastVertex; i++)
				{
					if (!HairMovability.IsMovable(i, movability))
						continue;

					lastFramePosWS = vertices[i];
					float4 lastFramePosWS4 = new float4(lastFramePosWS.x, lastFramePosWS.y, lastFramePosWS.z, 1);
					lastFramePosOS = math.mul(invPrevMatrix, lastFramePosWS4).xyz;
					float4 lastFramePosOS4 = new float4(lastFramePosOS.x, lastFramePosOS.y, lastFramePosOS.z, 1);

					posWS = math.mul(matrix, lastFramePosOS4).xyz;

					// Unoptimized: 
					// newPos = posWS + (lastFramePosWS - posWS) + (gravity * (timestep * timestep));

					// Optimized version:
					newPos.x = posWS.x + (lastFramePosWS.x - posWS.x) + (gravity.x * timestepSqr);
					newPos.y = posWS.y + (lastFramePosWS.y - posWS.y) + (gravity.y * timestepSqr);
					newPos.z = posWS.z + (lastFramePosWS.z - posWS.z) + (gravity.z * timestepSqr);

					vertices[i] = newPos;
				}
			}
		}

		public float gravityStrength = 1;

		public override void InitializeSimulation()
		{

		}

		protected override void _SimulationStep(float timestep)
		{
			// TODO: Damping
			Matrix4x4 matrix = this.transform.localToWorldMatrix;
			Matrix4x4 invPrevMatrix = this.simulation.prevFrameMatrix.inverse;

			// Read vertices and strands
			NativeArray<HairStrand> strands = this.instance.strands.CpuReference;
			NativeArray<float3> vertices = this.instance.vertices.CpuReference;
			NativeArray<uint> movability = this.instance.movability.CpuReference;
			float3 gravity = Physics.gravity * this.gravityStrength;
			
			float timestepSqr = timestep * timestep;

			Job job = new Job()
			{
				invPrevMatrix = invPrevMatrix,
				matrix = matrix,
				movability = this.simulation.movability,
				strands = this.simulation.strands,
				vertices = this.simulation.vertices,
				initialVertices = this.simulation.initialVertices,
				gravity = gravity,
				timestepSqr = timestepSqr
			};

			this.simulation.jobHandle = job.Schedule(this.simulation.strands.Length, 32, this.simulation.jobHandle);
		}
	}
}