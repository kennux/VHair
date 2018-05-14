using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VHair
{
    public class CPULengthConstraints : HairSimulationPass<CPUPhysicsSimulation>
    {
        public float stiffness = 1;

        /// <summary>
        /// Stores length for every vertex to the next vertex
        /// </summary>
        private float[] lengths;

        protected void Start()
        {
            // Build initial length data
            this.lengths = new float[this.instance.vertexCount];
            HairStrand[] strands = this.instance.GetStrandsArray();
            Vector3[] vertices = this.instance.GetVertexArray();

            // Iterate over every strand
            for (int i = 0; i < strands.Length; i++)
            {
                HairStrand strand = strands[i];

                // Iterate over every segment and calculate lengths
                for (int j = strand.firstVertex; j < strand.lastVertex; j++)
                    this.lengths[j] = (vertices[j] - vertices[j+1]).magnitude;
            }
        }

        public override void SimulationStep(float timestep)
        {
            HairStrand[] strands = this.instance.GetStrandsArray();
            Vector3[] vertices = this.instance.GetVertexArray();

            // Iterate over every strand
            for (int i = 0; i < strands.Length; i++)
            {
                HairStrand strand = strands[i];

                // Iterate over every segment and calculate lengths
                for (int j = strand.firstVertex; j < strand.lastVertex; j++)
                {
                    // Calculate segment info
                    Vector3 p1 = vertices[j], p2 = vertices[j + 1], dir = (p2-p1);
                    float len = Mathf.Max(dir.magnitude, float.Epsilon), iLen = this.lengths[j];

                    // Calculate the amount the hair is being stretched or compressed
                    float stretchFactor = 1f - (float)((double)len / (double)iLen);

                    // Enforce length constraints
                    vertices[j + 1] = p2 + (dir * stretchFactor * (this.stiffness * timestep));
                }
            }

            this.instance.SetVerticesModified(true);
        }
    }
}