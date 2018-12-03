using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

namespace UnityTK.Cameras
{
    /// <summary>
    /// Base class for implementing <see cref="ICameraModeFeature"/> as monobehaviours.
    /// Features can also be implemented as POCOs by just implementing the interface(s) directly.
    /// </summary>
    public abstract class CameraModeFeatureBase : MonoBehaviour, ICameraModeFeature
    {
        public abstract void PostProcessState(ref CameraState cameraState);
    }
}
