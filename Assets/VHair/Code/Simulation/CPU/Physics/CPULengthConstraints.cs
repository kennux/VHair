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

        public override void InitializeSimulation()
        {

        }

        public override void SimulationStep(float timestep)
        {

        }
    }
}