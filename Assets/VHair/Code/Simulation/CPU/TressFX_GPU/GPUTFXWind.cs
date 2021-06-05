using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace VHair
{
    public class GPUTFXWind : HairSimulationPass<GPUTFXPhysicsSimulation>
    {
        public WindZone windZone;

        public override void InitializeSimulation()
        {
        }

        protected override void _SimulationStep(float timestep)
        {
            simulation.simulationShader.SetVector("pWindForce", WindUtility.EvaluateWindForce(this.windZone, this.transform.position) * timestep);

            simulation.DispatchShader(simulation.simulationShader.FindKernel("WindGroup"), simulation.simulationShader.FindKernel("WindSingle"), this.simulation.instance.strandCount);
        }
    }
}