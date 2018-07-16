using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VHair
{
    public class CPUTFXVerlet : HairSimulationPass<CPUTFXPhysicsSimulation>
    {
        public float gravityStrength = 1;

        public override void InitializeSimulation()
        {

        }

        protected override void _SimulationStep(float timestep)
        {
            // TODO: Damping
            Matrix4x4 matrix = this.transform.localToWorldMatrix;
            Matrix4x4 invPrevMatrix = this.simulation.prevFrameMatrix.inverse;

            // Read vertices and strands
            HairStrand[] strands = this.instance.strands.cpuReference;
            Vector3[] vertices = this.instance.vertices.cpuReference;
            uint[] movability = this.instance.movability.cpuReference;

            Vector3 gravity = Physics.gravity * this.gravityStrength;
            HairStrand strand;
            Vector3 lastFramePosWS, lastFramePosOS, posWS, newPos;
            lastFramePosOS = lastFramePosWS = posWS = newPos = new Vector3();
            float timestepSqr = timestep * timestep;
            for (int j = 0; j < strands.Length; j++)
            {
                strand = strands[j];

                // First vertex is immovable
                vertices[strand.firstVertex] = matrix.MultiplyPoint3x4(this.simulation.initialVertices[strand.firstVertex]);

                for (int i = strand.firstVertex; i <= strand.lastVertex; i++)
                {
                    if (!HairMovability.IsMovable(i, movability))
                        continue;

                    lastFramePosWS = vertices[i];
                    lastFramePosOS = invPrevMatrix.MultiplyPoint3x4(lastFramePosWS);

                    posWS = matrix.MultiplyPoint3x4(lastFramePosOS);

                    // Unoptimized: 
                    // newPos = posWS + (posWS - lastFramePosWS) + (gravity * (timestep * timestep));

                    // Optimized version:
                    newPos.x = posWS.x + (posWS.x - lastFramePosWS.x) + (gravity.x * timestepSqr);
                    newPos.y = posWS.y + (posWS.y - lastFramePosWS.y) + (gravity.y * timestepSqr);
                    newPos.z = posWS.z + (posWS.z - lastFramePosWS.z) + (gravity.z * timestepSqr);

                    vertices[i] = newPos;
                }
            }

            // Set dirty
            this.instance.vertices.SetGPUDirty();
        }
    }
}