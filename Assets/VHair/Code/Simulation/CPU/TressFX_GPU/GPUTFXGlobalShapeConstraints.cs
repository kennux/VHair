using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace VHair
{
    public class GPUTFXGlobalShapeConstraints : HairSimulationPass<GPUTFXPhysicsSimulation>
    {
        public float stiffness = 0.3f;
        public int vertexRange = 3;

        public override void InitializeSimulation()
        {
        }

        protected override void _SimulationStep(float timestep)
        {
            Matrix4x4 localToWorld = this.transform.localToWorldMatrix;

            simulation.simulationShader.SetMatrix("pLocalToWorld", localToWorld);
            simulation.simulationShader.SetFloat("pStiffness", stiffness);
            simulation.simulationShader.SetInt("pVertexRange", vertexRange);

            simulation.DispatchShader(simulation.simulationShader.FindKernel("GlobalShapeConstraintsGroup"), simulation.simulationShader.FindKernel("GlobalShapeConstraintsSingle"), this.simulation.instance.strandCount);
        }
    }
}