using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

namespace UnityTK.Cameras
{
    /// <summary>
    /// Camera mode feature for shaking the camera.
    /// </summary>
    public interface ICameraModeShakeFeature : ICameraModeFeature
    {
        /// <summary>
        /// Shakes the camera with the specified force.
        /// Force is in screenspace.
        /// </summary>
        /// <param name="force">The force to shake the camera with.</param>
        void Shake(Vector2 force);
    }
}
