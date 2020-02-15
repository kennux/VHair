using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace VHair
{
	public class CPUTFXSphereCollision_Burst : HairSimulationPass<CPUTFXPhysicsSimulation_Burst>
	{
		[BurstCompile]
		struct Job : IJobParallelFor
		{
			public int colliderCount;
			
			[ReadOnly]
			public NativeArray<CollisionSphere> colliders;
			[ReadOnly]
			public NativeArray<uint> movability;
			[NativeDisableParallelForRestriction]
			public NativeArray<float3> vertices;
			public void Execute(int i)
			{
				// Is hair movable?
				if (HairMovability.IsMovable(i, movability))
				{
					bool wasModified = false;
					float3 v = vertices[i];
					for (int j = 0; j < colliderCount; j++)
					{
						// Read collider
						var c = colliders[j];

						// Intersection?
						float3 dir = (v - c.center);
						float sqMag = math.lengthsq(dir);
						if (sqMag <= c.radiusSq) // if (Vector3.Distance(v, c.center) <= c.radius)
						{
							// Intersection! push the vertex out
							float d = math.sqrt(sqMag); // dir.magnitude;
							v += (dir / d) * (c.radius - d);
							wasModified = true;
						}
					}

					if (wasModified)
						vertices[i] = v; // Sync
				}
			}
		}

		public SphereCollider[] colliders;

		private struct CollisionSphere
		{
			public float3 center;
			public float radius;
			public float radiusSq;
		}

		private NativeArray<CollisionSphere> _colliders;

		public void UpdateColliders(bool dispose = true)
		{
			if (dispose)
				_colliders.Dispose();

			_colliders = new NativeArray<CollisionSphere>(this.colliders.Length, Allocator.Persistent);
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

			Job job = new Job()
			{
				colliders = _colliders,
				vertices = this.simulation.vertices,
				movability = this.simulation.movability,
				colliderCount = _colliders.Length
			};
			
			this.simulation.jobHandle = job.Schedule(this.simulation.vertices.Length, 128, this.simulation.jobHandle);
		}

		public void OnDestroy()
		{
			this._colliders.Dispose();
		}
	}
}