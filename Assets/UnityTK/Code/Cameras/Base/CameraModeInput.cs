using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

namespace UnityTK.Cameras
{
    /// <summary>
    /// Abstract base class for implementing camera input components.
    /// <seealso cref="CameraMode"/>
    /// </summary>
    [RequireComponent(typeof(CameraMode))]
    public abstract class CameraModeInput<T> : UTKCameraComponent
    {
        public abstract T GetData();
    }
}
