using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityTK.Benchmarking
{
    /// <summary>
    /// Datastructure used to read data from <see cref="BenchmarkResult"/>
    /// </summary>
    public struct BenchmarkResultLabel
    {
        /// <summary>
        /// The label / display name of this label.
        /// </summary>
        public string label;

        /// <summary>
        /// The amount of time spent on this label in milliseconds.
        /// </summary>
        public double time;

        /// <summary>
        /// All children of this label, this will be null if there are no children.
        /// </summary>
        public List<BenchmarkResultLabel> children;
    }
}
