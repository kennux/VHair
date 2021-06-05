using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace VHair
{
    public class GPUTFXVerlet : HairSimulationPass<GPUTFXPhysicsSimulation>
    {
        public float gravityStrength = 1;

        public override void InitializeSimulation()
        {
        }

        protected override void _SimulationStep(float timestep)
        {
            Matrix4x4 localToWorld = this.transform.localToWorldMatrix;
            Matrix4x4 invPrevLocalToWorld = this.simulation.prevFrameMatrix.inverse;

            simulation.simulationShader.SetMatrix("pLocalToWorld", localToWorld);
            simulation.simulationShader.SetMatrix("pInvPrevLocalToWorld", invPrevLocalToWorld);
            simulation.simulationShader.SetVector("pGravity", Physics.gravity * this.gravityStrength);

            simulation.DispatchShader(simulation.simulationShader.FindKernel("VerletGroup"), simulation.simulationShader.FindKernel("VerletSingle"), this.simulation.instance.strandCount);
        }
    }
}