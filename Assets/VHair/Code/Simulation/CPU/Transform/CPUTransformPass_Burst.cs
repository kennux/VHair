using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace VHair
{
	/// <summary>
	/// CPU-based vertex transformation pass.
	/// Will initially grab the vertices of the hair asset in object space and every frame transform them into worldspace.
	/// 
	/// This pass completely wipes and overrides the vertices! Thus it should be used only when it is the only pass.
	/// </summary>
	public class CPUTransformPass_Burst : HairSimulationPass<CPUTransformSimulation_Burst>
	{
		[BurstCompile]
		struct TransformJob : IJobParallelFor
		{
			[ReadOnly] public NativeArray<float3> initialVertices;
			[WriteOnly] public NativeArray<float3> vertices;

			public float4x4 matrix;

			public void Execute(int index)
			{
				vertices[index] = math.transform(matrix, initialVertices[index]);
			}
		}

		public override void InitializeSimulation()
		{

		}

		protected override void _SimulationStep(float timestep)
		{
			TransformJob job = new TransformJob()
			{
				initialVertices = simulation.initialVertices,
				vertices = simulation.vertices,
				matrix = Matrix4x4.TRS(this.transform.position, this.transform.rotation, this.transform.lossyScale)
			};

			job.Schedule(simulation.vertices.Length, 128).Complete();
			this.instance.vertices.CpuReference.CopyFrom(simulation.vertices);
			this.instance.vertices.SetGPUDirty();
		}
	}
}