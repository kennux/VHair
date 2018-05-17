using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VHair
{
    /// <summary>
    /// Base class for implementing hair simulations.
    /// 
    /// Runs simulation stepping of all components (<see cref="IHairSimulationPass"/>, <see cref="HairSimulationPass{T}"/>).
    /// Essentially a master class for any hair simulation.
    /// </summary>
    public class HairSimulation : HairInstanceComponent<HairInstance>
    {
        /// <summary>
        /// Updating mode.
        /// Values map to the respective unity engine update message.
        /// 
        /// <see cref="SimulationUpdating.SCRIPTED"/> can be set to dont update the hair automatically but instead require manual <see cref="ScriptedUpdate(float)"/> calls.
        /// </summary>
        public enum SimulationUpdating
        {
            UPDATE,
            FIXED_UPDATE,
            LATE_UPDATE,
            SCRIPTED
        }

        public SimulationUpdating update;
        private List<IHairSimulationPass> passes = new List<IHairSimulationPass>();

        /// <summary>
        /// Updates <see cref="passes"/>
        /// </summary>
        private void UpdatePasses()
        {
            passes.Clear();
            GetComponents(passes);
        }

        /// <summary>
        /// Runs hair simulation initialization.
        /// <see cref="HairSimulationPass{T}.InitializeSimulation"/>
        /// </summary>
        protected virtual void Start()
        {
            UpdatePasses();
            foreach (var pass in this.passes)
            {
                pass.InitializeSimulation();
            }
        }

        /// <summary>
        /// Can be called to manually run the hair simulation with the specified time step once.
        /// </summary>
        /// <param name="timestep">The amount that passed since the last call, aka the amount of time passed you want to simulate (the larger the period is, the less accurate the simulation usually is).</param>
        public void ScriptedUpdate(float timestep)
        {
            _Update(timestep);
        }

        #region Stepping

        protected virtual void _Update(float timestep)
        {
            UpdatePasses();
            foreach (var pass in this.passes)
            {
                pass.SimulationStep(timestep);
            }
        }

        public void Update()
        {
            if (this.update == SimulationUpdating.UPDATE)
                _Update(Time.deltaTime);
        }

        public void LateUpdate()
        {
            if (this.update == SimulationUpdating.LATE_UPDATE)
                _Update(Time.deltaTime);
        }

        public void FixedUpdate()
        {
            if (this.update == SimulationUpdating.FIXED_UPDATE)
                _Update(Time.fixedDeltaTime);
        }

        #endregion
    }
}