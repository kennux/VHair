using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace VHair
{
    public class CPUTFXLocalShapeConstraints_Burst : HairSimulationPass<CPUTFXPhysicsSimulation_Burst>
    {
		[BurstCompile]
		struct Job : IJobParallelFor
		{
			public float stiffness;
			public float timestep;
			public quaternion rotation;
			
			[ReadOnly]
			public NativeArray<quaternion> globalTransform;
			[ReadOnly]
			public NativeArray<float3> referenceVectors;
			[ReadOnly]
			public NativeArray<HairStrand> strands;
			[NativeDisableParallelForRestriction]
			public NativeArray<float3> vertices;
			[ReadOnly]
			public NativeArray<uint> movability;

			public void Execute(int index)
			{
				HairStrand strand = this.strands[index];
				quaternion rotGlobal = this.globalTransform[strand.firstVertex];
				
                for (int j = strand.firstVertex+1; j < strand.lastVertex-1; j++)
                {
					quaternion rotGlobalWorld = math.mul(rotation,rotGlobal);

					float3 p1 = vertices[j], p2 = vertices[j + 1];
					float3 orgP2 = math.mul(rotGlobalWorld, this.referenceVectors[j+1]) + p1;
					float3 delta = stiffness * (orgP2 - p2);

					if (HairMovability.IsMovable(j, movability))
						p1 -= delta;
					if (HairMovability.IsMovable(j + 1, movability))
						p2 += delta;

					float3 vec = (p2 - p1);
					
					rotGlobal = math.mul(rotGlobal, quaternion.LookRotation(math.normalize(math.mul(math.inverse(rotGlobalWorld), vec)), new float3(0,1,0)));
					vertices[j] = p1;
					vertices[j+1] = p2;
                }
			}
		}

		// Settings
        public float stiffness;
		
		// Local
		[NonSerialized]
		[HideInInspector]
        public NativeArray<quaternion> localTransform;

		// Global
		[NonSerialized]
		[HideInInspector]
        public NativeArray<quaternion> globalTransform;

		// Other
        private NativeArray<float3> referenceVectors;

		public void OnDestroy()
		{
			this.localTransform.Dispose();
			this.globalTransform.Dispose();
			this.referenceVectors.Dispose();
		}

		public override void InitializeSimulation()
        {
            this.localTransform = new NativeArray<quaternion>(this.instance.vertexCount, Allocator.Persistent);
            this.globalTransform = new NativeArray<quaternion>(this.instance.vertexCount, Allocator.Persistent);
            this.referenceVectors = new NativeArray<float3>(this.instance.vertexCount, Allocator.Persistent);

			for (int i = 0; i < this.instance.vertexCount; i++)
			{
				this.localTransform[i] = Quaternion.identity;
				this.globalTransform[i] = Quaternion.identity;
				this.referenceVectors[i] = Vector3.zero;
			}

            HairStrand[] strands = this.instance.asset.CreateStrandDataCopy();
            float3[] vertices = this.instance.asset.CreateVertexDataCopy();
            this.CalculateTransforms(strands, vertices, this.localTransform, this.globalTransform, this.referenceVectors);
        }

        private void CalculateTransforms(HairStrand[] strands, float3[] vertices, NativeArray<quaternion> localTransforms, NativeArray<quaternion> globalTransforms, NativeArray<float3> referenceVectors)
        {
            for (int i = 0; i < strands.Length; i++)
            {
                HairStrand strand = strands[i];
                int j;

				// First vertex
				Quaternion local;
				local = globalTransforms[strand.firstVertex] = localTransforms[strand.firstVertex] = Quaternion.LookRotation(math.normalize(vertices[strand.firstVertex + 1] - vertices[strand.firstVertex]));

                for (j = strand.firstVertex+1; j < strand.lastVertex; j++)
                {
                    Vector3 p1 = vertices[j-1], p2 = vertices[j], d = (p2 - p1);
					Vector3 vec = Quaternion.Inverse(globalTransforms[j - 1]) * d;
					if (vec.magnitude < 0.001f)
						local = Quaternion.identity;
					else
						local = Quaternion.LookRotation(vec.normalized);

					referenceVectors[j] = vec;
					globalTransforms[j] = globalTransforms[j-1] * local;
                    localTransforms[j] = local;
                }
            }
        }

        protected override void _SimulationStep(float timestep)
        {
			float stiffness = .5f * Mathf.Min(this.stiffness * timestep, .95f);
			Job job = new Job()
			{
				globalTransform = this.globalTransform,
				referenceVectors = this.referenceVectors,
				movability = this.simulation.movability,
				strands = this.simulation.strands,
				vertices = this.simulation.vertices,
				stiffness = stiffness,
				timestep = timestep,
				rotation = this.transform.rotation
			};
			
			this.simulation.jobHandle = job.Schedule(this.simulation.strands.Length, 32, this.simulation.jobHandle);
        }
    }
}