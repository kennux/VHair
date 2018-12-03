using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace VHair
{
    public class CPUTFXWind_Burst : HairSimulationPass<CPUTFXPhysicsSimulation_Burst>
    {
		[BurstCompile]
		struct Job : IJobParallelFor
		{
			public float3 force;
			
			[ReadOnly]
			public NativeArray<uint> movability;
			[NativeDisableParallelForRestriction]
			public NativeArray<float3> vertices;
			public void Execute(int i)
			{
                if (!HairMovability.IsMovable(i, movability))
                    return;

                vertices[i] = vertices[i] + force;
			}
		}

		public WindZone windZone;

        public override void InitializeSimulation()
        {

        }

        protected override void _SimulationStep(float timestep)
        {
            Vector3 f = this.windZone.transform.forward * Mathf.Abs(Mathf.Sin(Time.time) * this.windZone.windMain) * timestep;
			Job job = new Job()
			{
				force = f,
				movability = this.simulation.movability,
				vertices = this.simulation.vertices
			};

			this.simulation.jobHandle = job.Schedule(this.simulation.vertices.Length, 128, this.simulation.jobHandle);
        }
    }
}