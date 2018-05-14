using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VHair
{
    public interface IHairSimulationPass
    {
        void SimulationStep(float timestep);
    }
}
