using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VHair
{
    public class CPUTFXWind : HairSimulationPass<CPUTFXPhysicsSimulation>
    {
        public WindZone windZone;

        public override void InitializeSimulation()
        {

        }

        protected override void _SimulationStep(float timestep)
        {
            // TODO: Spherical wind zones
            Vector3[] vertices = this.instance.vertices.cpuReference;
            uint[] movability = this.instance.movability.cpuReference;
            Vector3 f = this.windZone.transform.forward * Mathf.Abs(Mathf.Sin(Time.time) * this.windZone.windMain);

            for (int i = 0; i < vertices.Length; i++)
            {
                if (!HairMovability.IsMovable(i, movability))
                    continue;

                vertices[i] = vertices[i] + (f * timestep);
            }
        }
    }
}