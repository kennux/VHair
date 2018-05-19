using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VHair
{
    /// <summary>
    /// CPU implementation of a length constraint algorithm.
    /// 
    /// </summary>
    public class CPULengthConstraints : HairSimulationPass<CPUPhysicsSimulation>
    {
        public float stiffness = 1;

        private float[] lengths;

        public override void InitializeSimulation()
        {
            this.lengths = new float[this.instance.vertexCount];
            HairStrand[] strands = this.instance.strands.cpuReference;
            Vector3[] vertices = this.instance.vertices.cpuReference;

            for (int s = 0; s < strands.Length; s++)
            {
                HairStrand strand = strands[s];
                for (int j = strand.firstVertex+1; j < strand.lastVertex; j++)
                {
                    this.lengths[j] = Vector3.Distance(vertices[j], vertices[j - 1]);
                }
            }
        }

        protected override void _SimulationStep(float timestep)
        {
            HairStrand[] strands = this.instance.strands.cpuReference;
            Vector3[] vertices = this.instance.vertices.cpuReference;
            uint[] movability = this.instance.movability.cpuReference;
            
            for (int s = 0; s < strands.Length; s++)
            {
                HairStrand strand = strands[s];
                for (int j = strand.firstVertex + 1; j <= strand.lastVertex; j++)
                {
                    if (!HairMovability.IsMovable(j, movability))
                        continue;

                    float nDist = this.lengths[j];
                    Vector3 p = vertices[j], pPrev = vertices[j - 1], pDir = (p - pPrev);
                    float dist = pDir.magnitude;
                    float distDiff = (nDist - dist);

                    vertices[j] = p + ((pDir / dist) * (distDiff * this.stiffness * timestep));
                }
            }

            this.instance.vertices.SetGPUDirty();
        }
    }
}