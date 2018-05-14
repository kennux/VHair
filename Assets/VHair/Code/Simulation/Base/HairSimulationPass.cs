using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VHair
{
    public abstract class HairSimulationPass<T> : HairInstanceComponent<HairInstance>, IHairSimulationPass where T : HairSimulation
    {
        protected T simulation;

        protected virtual void Awake()
        {
            this.simulation = GetComponent<T>();
            if (ReferenceEquals(this.simulation, null))
            {
                Debug.LogError("Simulation pass on incompatible simulation type! Destroying myself!");
                Destroy(this);
            }
        }
        public abstract void SimulationStep(float timestep);
    }
}