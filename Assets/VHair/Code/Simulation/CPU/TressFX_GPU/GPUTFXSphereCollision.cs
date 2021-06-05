using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace VHair
{
    public class GPUTFXSphereCollision : HairSimulationPass<GPUTFXPhysicsSimulation>
	{

		public SphereCollider[] colliders;

		private struct CollisionSphere
		{
			public float3 center;
			public float radius;
			public float radiusSq;
		}

		private NativeArray<CollisionSphere> _colliders;
		private GraphicsBuffer _collidersGpu;

		public void UpdateColliders(bool dispose = true)
		{
			if (dispose)
				_colliders.Dispose();

			_colliders = new NativeArray<CollisionSphere>(this.colliders.Length, Allocator.Persistent);
			_collidersGpu = new GraphicsBuffer(GraphicsBuffer.Target.Structured, this.colliders.Length, 20);
		}

		public override void InitializeSimulation()
		{
			UpdateColliders(false);
		}

		protected override void _SimulationStep(float timestep)
		{
			for (int i = 0; i < this.colliders.Length; i++)
			{
				float radius = this.colliders[i].radius * Mathf.Max(Mathf.Max(this.colliders[i].transform.lossyScale.x, this.colliders[i].transform.lossyScale.y), this.colliders[i].transform.lossyScale.z);
				_colliders[i] = new CollisionSphere()
				{
					center = this.colliders[i].transform.TransformPoint(this.colliders[i].center),
					radius = radius,
					radiusSq = radius * radius
				};
			}

			_collidersGpu.SetData(_colliders);

			int kernelGroup = simulation.simulationShader.FindKernel("SphereCollisionGroup");
			int kernelSingle = simulation.simulationShader.FindKernel("SphereCollisionSingle");
			simulation.simulationShader.SetBuffer(kernelGroup, "sCollisionSpheres", _collidersGpu);
			simulation.simulationShader.SetBuffer(kernelSingle, "sCollisionSpheres", _collidersGpu);
			simulation.simulationShader.SetInt("pColliderCount", _colliders.Length);

			simulation.DispatchShader(kernelGroup, kernelSingle, this.simulation.instance.strandCount);
		}

		public void OnDestroy()
		{
			this._collidersGpu.Dispose();
			this._colliders.Dispose();
		}
	}
}