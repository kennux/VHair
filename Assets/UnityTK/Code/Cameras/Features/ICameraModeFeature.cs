using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityTK.Cameras
{
    /// <summary>
    /// Base interface to be inherited from by every camera mode feature interface.
    /// These interfaces are used to encapsule specific features away from the actual camera mode implementation.
    /// 
    /// Every camera mode can have multiple features available, the camera will be able to provide access to these based on the currently active camera mode.
    /// <seealso cref="CameraMode"/>
    /// <seealso cref="UTKCamera"/>
    /// </summary>
    public interface ICameraModeFeature
    {
        /// <summary>
        /// Post processes the camera state, <see cref="CameraMode.PostProcessState(CameraState)"/>
        /// </summary>
        /// <param name="cameraState">The camera state to post process.</param>
        void PostProcessState(ref CameraState cameraState);
    }
}
