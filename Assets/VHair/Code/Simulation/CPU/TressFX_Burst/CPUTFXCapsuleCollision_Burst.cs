using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace VHair
{
	public class CPUTFXCapsuleCollision_Burst : HairSimulationPass<CPUTFXPhysicsSimulation_Burst>
	{
		struct Job : IJobParallelFor
		{
			public int colliderCount;
			
			[ReadOnly]
			public NativeArray<CollisionCasule> colliders;
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

						// Test sphere 1
						float3 dir = (v - c.sphere1Pos);
						float sqMag = math.lengthsq(dir);
						if (sqMag <= c.radiusSq)
						{
							// Intersection! push the vertex out
							float d = math.sqrt(sqMag);
							v += (dir / d) * (c.radius - d);
							wasModified = true;
						}
						else
						{
							// Test sphere 2
							dir = (v - c.sphere2Pos);
							sqMag = math.lengthsq(dir);
							if (sqMag <= c.radiusSq)
							{
								// Intersection! push the vertex out
								float d = math.sqrt(sqMag);
								v += (dir / d) * (c.radius - d);
								wasModified = true;
							}
							else
							{
								// Test cylinder
								float3 d = c.sphere2Pos - c.sphere1Pos;
								float3 pd = v - c.sphere1Pos;

								float dot = math.dot(d, pd);
								if (dot >= 0 && dot <= c.lengthSq)
								{
									float dist = math.lengthsq(pd) - (dot * dot / c.lengthSq);
									if (dist <= c.radiusSq)
									{
										// Get closest point to vertex on cylinder axis
										float3 dNorm = d / c.length;
										float dotP = math.dot(v - c.sphere1Pos, dNorm);
										float3 axisPoint = c.sphere1Pos + dNorm * dotP;

										// Construct ray through vertex from axis point and get cylinder body exit point
										v =  axisPoint + (math.normalizesafe(v - axisPoint) * c.radius);
										wasModified = true;
									}
								}
							}
						}
					}

					if (wasModified)
						vertices[i] = v; // Sync
				}
			}
		}

		public CapsuleCollider[] colliders;

		private struct CollisionCasule
		{
			public float3 sphere1Pos;
			public float3 sphere2Pos;
			public float3 cylinderCenter;
			public float radius;
			public float radiusSq;
			public float length;
			public float lengthSq;
		}

		private NativeArray<CollisionCasule> _colliders;

		public void UpdateColliders(bool dispose = true)
		{
			if (dispose)
				_colliders.Dispose();

			_colliders = new NativeArray<CollisionCasule>(this.colliders.Length, Allocator.Persistent);
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
				float3 center = this.colliders[i].transform.TransformPoint(this.colliders[i].center);
				float height = this.colliders[i].height * Mathf.Max(Mathf.Max(this.colliders[i].transform.lossyScale.x, this.colliders[i].transform.lossyScale.y), this.colliders[i].transform.lossyScale.z);
				float heightHalf = height * 0.5f;
				float3 sphere1Pos = center + (float3)(this.colliders[i].transform.up * (heightHalf - radius));
				float3 sphere2Pos = center - (float3)(this.colliders[i].transform.up * (heightHalf - radius));

				_colliders[i] = new CollisionCasule()
				{
					cylinderCenter = center,
					sphere1Pos = sphere1Pos,
					sphere2Pos = sphere2Pos,
					radius = radius,
					radiusSq = radius * radius,
					length = math.length(sphere1Pos - sphere2Pos),
					lengthSq = math.lengthsq(sphere1Pos - sphere2Pos)
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