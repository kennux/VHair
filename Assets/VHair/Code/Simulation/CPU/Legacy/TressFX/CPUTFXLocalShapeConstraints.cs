using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace VHair
{
    public class CPUTFXLocalShapeConstraints : HairSimulationPass<CPUTFXPhysicsSimulation>
    {
		// Settings
        public float stiffness;
		public bool debugGlobalTransform;
		public bool debugLocalTransform;
		public bool debugRealtime;
		public int debugSteps = 16;
		
		// Local
		[NonSerialized]
		[HideInInspector]
        public Quaternion[] localTransform;

		// Global
		[NonSerialized]
		[HideInInspector]
        public Quaternion[] globalTransform;
		[NonSerialized]
		[HideInInspector]
        public Quaternion[] realtimeDebug;

		// Other
        private Vector3[] referenceVectors;

        public override void InitializeSimulation()
        {
            this.localTransform = new Quaternion[this.instance.vertexCount];
            this.globalTransform = new Quaternion[this.instance.vertexCount];
            this.realtimeDebug = new Quaternion[this.instance.vertexCount];
            this.referenceVectors = new Vector3[this.instance.vertexCount];

			for (int i = 0; i < this.instance.vertexCount; i++)
			{
				this.localTransform[i] = Quaternion.identity;
				this.globalTransform[i] = Quaternion.identity;
				this.realtimeDebug[i] = Quaternion.identity;
				this.referenceVectors[i] = Vector3.zero;
			}

            HairStrand[] strands = this.instance.asset.CreateStrandDataCopy();
			float3[] vertices = this.instance.asset.CreateVertexDataCopy();
            this.CalculateTransforms(strands, vertices, this.localTransform, this.globalTransform, this.referenceVectors);
        }

        private void CalculateTransforms(HairStrand[] strands, float3[] vertices, Quaternion[] localTransforms, Quaternion[] globalTransforms, Vector3[] referenceVectors)
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
            NativeArray<HairStrand> strands = this.instance.strands.CpuReference;
			NativeArray<float3> vertices = this.instance.vertices.CpuReference;
			NativeArray<uint> movability = this.instance.movability.CpuReference;
			Quaternion rotation = this.transform.rotation;
			
			// Apply local shape constraint
            for (int i = 0; i < strands.Length; i++)
            {
                HairStrand strand = strands[i];
				Quaternion rotGlobal = this.globalTransform[strand.firstVertex];
				
				realtimeDebug[strand.firstVertex] = rotGlobal;
                for (int j = strand.firstVertex+1; j < strand.lastVertex-1; j++)
                {
					Quaternion rotGlobalWorld = rotation*rotGlobal;

					Vector3 p1 = vertices[j], p2 = vertices[j + 1];
					Vector3 orgP2 = rotGlobalWorld * this.referenceVectors[j+1] + p1;
					Vector3 delta = stiffness * (orgP2 - p2);

					if (HairMovability.IsMovable(j, movability))
						p1 -= delta;
					if (HairMovability.IsMovable(j + 1, movability))
						p2 += delta;

					Vector3 vec = (p2 - p1);
					
					rotGlobal = rotGlobal * Quaternion.LookRotation((Quaternion.Inverse(rotGlobalWorld) * vec).normalized);
					vertices[j] = p1;
					vertices[j+1] = p2;
					realtimeDebug[j] = rotGlobal;
                }
				realtimeDebug[strand.lastVertex] = rotGlobal;
            }

			this.instance.vertices.SetGPUDirty();
        }
    }
}