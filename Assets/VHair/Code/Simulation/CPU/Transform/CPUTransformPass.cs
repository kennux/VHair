using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VHair
{
    /// <summary>
    /// CPU-based vertex transformation pass.
    /// Will initially grab the vertices of the hair asset in object space and every frame transform them into worldspace.
    /// 
    /// This pass completely wipes and overrides the vertices! Thus it should be used only when it is the only pass.
    /// </summary>
    public class CPUTransformPass : HairSimulationPass<CPUTransformSimulation>
    {

        public override void SimulationStep(float timestep)
        {
            var vertexArray = this.instance.vertices.cpuReference;
            Matrix4x4 matrix = Matrix4x4.TRS(this.transform.position, this.transform.rotation, this.transform.lossyScale);
            var vertices = this.simulation.vertices;
            for (int i = 0; i < vertexArray.Length; i++)
            {
                vertexArray[i] = matrix.MultiplyPoint3x4(vertices[i]);
            }
            
            this.instance.vertices.SetGPUDirty();
        }
    }
}