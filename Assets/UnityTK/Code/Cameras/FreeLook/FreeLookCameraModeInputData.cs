using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

namespace UnityTK.Cameras
{
    /// <summary>
    /// Input datastructure for <see cref="FreeLookCameraMode"/>.
    /// </summary>
    public struct FreeLookCameraModeInputData
    {
        /// <summary>
        /// The look axis in screenspace.
        /// </summary>
        public Vector2 lookAxis;
    }
}
