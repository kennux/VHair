using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Cameras
{
    /// <summary>
    /// The input data for <see cref="TopDownCameraMode"/>
    /// </summary>
    public struct TopDownCameraModeInputData
    {
        /// <summary>
        /// The movement delta on the camera 2d plane.
        /// </summary>
        public Vector2 movementDelta;

        /// <summary>
        /// The camera zoom delta, postive = up, negative = down.
        /// </summary>
        public float zoomDelta;
    }
}
