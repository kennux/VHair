using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

namespace UnityTK.Cameras
{
    /// <summary>
    /// A datastructure for keeping the state of a camera.
    /// This currently only includes camera transformation.
    /// </summary>
    public struct CameraState
    {
        /// <summary>
        /// The camera world transformation.
        /// </summary>
        public TransformStruct transform;

        /// <summary>
        /// Extracts camera state information from the specified camera.
        /// </summary>
        /// <param name="camera">The camera to extract data from.</param>
        /// <returns>The camera state data structure.</returns>
        public CameraState(Camera camera)
        {
            this.transform = new TransformStruct(camera.transform);
        }

        /// <summary>
        /// Applies the state to the specified camera.
        /// </summary>
        /// <param name="camera">The camera to apply to state to.</param>
        public void Apply(Camera camera)
        {
            this.transform.Apply(camera.transform);
        }
    }
}
