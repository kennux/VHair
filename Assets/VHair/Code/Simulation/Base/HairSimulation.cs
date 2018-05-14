using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VHair
{
    public class HairSimulation : HairInstanceComponent<HairInstance>
    {
        public enum SimulationUpdating
        {
            UPDATE,
            FIXED_UPDATE,
            LATE_UPDATE,
            SCRIPTED
        }

        public SimulationUpdating update;
        private List<IHairSimulationPass> passes = new List<IHairSimulationPass>();
        
        protected virtual void _Update(float timestep)
        {
            passes.Clear();
            GetComponents(passes);
            foreach (var pass in this.passes)
            {
                pass.SimulationStep(timestep);
            }
        }

        public void ScriptedUpdate(float timestep)
        {
            _Update(timestep);
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
    }
}