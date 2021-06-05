using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Jobs;

namespace VHair
{
	/// <summary>
	/// Base class for cpu based physics simulation.
	/// </summary>
	public class CPUTFXPhysicsSimulation_Burst : HairSimulation
	{
		// Simulation properties
		[HideInInspector]
		public NativeArray<float3> prevFrameVertices;

		[HideInInspector]
		public NativeArray<float3> initialVertices;

		[HideInInspector]
		public Matrix4x4 prevFrameMatrix;
		
		public NativeArray<HairStrand> strands;
		public NativeArray<float3> vertices;
		public NativeArray<uint> movability;

		public float timestepScale = 1;

		protected override void Start()
		{
			base.Start();
			
			this.prevFrameVertices = this.instance.asset.CreateVertexDataCopy(Allocator.Persistent);
			this.initialVertices = this.instance.asset.CreateVertexDataCopy(Allocator.Persistent);

			// Copy asset data
			this.vertices = this.instance.asset.CreateVertexDataCopy(Allocator.Persistent);
			this.strands = this.instance.asset.CreateStrandDataCopy(Allocator.Persistent);
			this.movability = this.instance.asset.CreateMovabilityDataCopy(Allocator.Persistent);
			this.prevFrameMatrix = this.transform.localToWorldMatrix;

			// Initial vertex transform
			var vertices = this.instance.vertices.CpuReference;
			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i] = this.prevFrameMatrix.MultiplyPoint3x4(vertices[i]);
			}
			this.instance.vertices.SetGPUDirty();
		}

		public JobHandle jobHandle;
		protected override void _Update(float timestep)
		{
			this.jobHandle = default(JobHandle);

			base._Update(timestep * this.timestepScale);

			JobHandle.ScheduleBatchedJobs();
			this.jobHandle.Complete();
			this.vertices.CopyTo(this.instance.vertices.CpuReference);
			this.instance.vertices.SetGPUDirty();
			this.vertices.CopyTo(this.prevFrameVertices);
			this.prevFrameMatrix = this.transform.localToWorldMatrix;
		}

		public void OnDestroy()
		{
			this.vertices.Dispose();
			this.strands.Dispose();
			this.movability.Dispose();
			this.prevFrameVertices.Dispose();
			this.initialVertices.Dispose();
		}
	}
}