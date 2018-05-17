using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VHair
{
    /// <summary>
    /// Abstract implementation of a hair simulation pass.
    /// The hair simulation is divided into passes where each pass simulates a specific thing.
    /// 
    /// The passes are run in the order they are in the unity inspector.
    /// </summary>
    /// <typeparam name="T">The hair simulation base class.</typeparam>
    public abstract class HairSimulationPass<T> : HairInstanceComponent<HairInstance>, IHairSimulationPass where T : HairSimulation
    {
        protected T simulation;

        /// <summary>
        /// Reads <see cref="simulation"/>
        /// </summary>
        protected virtual void Awake()
        {
            this.simulation = GetComponent<T>();
            if (ReferenceEquals(this.simulation, null))
            {
                Debug.LogError("Simulation pass on incompatible simulation type! Destroying myself!");
                Destroy(this);
            }
        }

        public abstract void InitializeSimulation();
        public abstract void SimulationStep(float timestep);
    }
}