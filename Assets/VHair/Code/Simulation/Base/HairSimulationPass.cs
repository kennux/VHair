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
        /// <summary>
        /// Amount of iterations per step.
        /// When this is > 1, there are multiple iterations of this pass executed.
        /// 
        /// When multiple passes are executed the timestep is divded by the amount of iterations and then the simulation is executed with multiple iterations.
        /// </summary>
        public int iterationsPerStep = 1;

        protected T simulation;

		public bool Enabled => enabled;

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

        /// <summary>
        /// Empty, only here to make scripts toggleable in the editor >_>
        /// </summary>
        protected virtual void Update()
        {

        }

        public virtual void SimulationStep(float timestep)
        {
            float t = timestep / (float)this.iterationsPerStep;
            for (int i = 0; i < this.iterationsPerStep; i++)
            {
                this._SimulationStep(t);
            }
        }

        public abstract void InitializeSimulation();
        protected abstract void _SimulationStep(float timestep);
    }
}