using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VHair
{
    /// <summary>
    /// Interface for implementing hair simulation passes.
    /// The hair simulation is divided into passes where each pass simulates a specific thing (like a gravity simulation, followed by an external force like wind simulation, finished by a shape / length constraint pass).
    /// </summary>
    public interface IHairSimulationPass
    {
        /// <summary>
        /// Called when the simulation is being initialized from <see cref="HairSimulation"/>.
        /// </summary>
        void InitializeSimulation();

        /// <summary>
        /// Called every time the simulation updates.
        /// </summary>
        /// <param name="timestep">The amount of time that passed since the last simulation step.</param>
        void SimulationStep(float timestep);
    }
}
