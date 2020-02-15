using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace VHair
{
	public class CPUTFXGlobalShapeConstraint : HairSimulationPass<CPUTFXPhysicsSimulation>
	{
		public float stiffness = 0.3f;
		public int vertexRange = 3;

		public override void InitializeSimulation()
		{
		}

		protected override void _SimulationStep(float timestep)
		{
			NativeArray<float3> vertices = this.instance.vertices.CpuReference;
			NativeArray<uint> movability = this.instance.movability.CpuReference;
			NativeArray<HairStrand> strands = this.instance.strands.CpuReference;

			Matrix4x4 matrix = this.transform.localToWorldMatrix;
			for (int j = 0; j < strands.Length; j++)
			{
				HairStrand strand = strands[j];
				int target = Mathf.Min(strand.firstVertex + this.vertexRange, strand.lastVertex);
				for (int i = strand.firstVertex; i <= target; i++)
				{
					if (!HairMovability.IsMovable(i, movability))
						continue;

					Vector3 iV = this.simulation.initialVertices[i], v = vertices[i];
					iV = matrix.MultiplyPoint3x4(iV);

					Vector3 delta = (iV - v);
					vertices[i] = v + (delta * this.stiffness * timestep);
				}
			}

			this.instance.vertices.SetGPUDirty();
		}
	}
}