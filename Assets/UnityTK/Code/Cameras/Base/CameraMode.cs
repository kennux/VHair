using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityTK.Cameras
{
    /// <summary>
    /// Abstract base class for implementing a camera mode.
    /// Camera modes contain the logic for camera behaviour of <see cref="UTKCamera"/>.
    /// 
    /// This is only a base class, for implementing new camera modes <see cref="CameraModeBase{TInputData}"/>
    /// </summary>
    public abstract class CameraMode : UTKCameraComponent
    {
        /// <summary>
        /// List of feature interface implementations this camera mode has available.
        /// </summary>
        protected List<ICameraModeFeature> features = new List<ICameraModeFeature>();

        /// <summary>
        /// Sets up <see cref="features"/> by filling the list with references to features.
        /// </summary>
        protected virtual void Awake()
        {
            this.GetComponents<ICameraModeFeature>(this.features);
        }

        /// <summary>
        /// Returns the camera mode feature of specified type T.
        /// </summary>
        /// <typeparam name="T">The interface type of the feature to retrieve the implementation for.</typeparam>
        /// <returns>The feature instance if it was found, null otherwise.</returns>
        public T TryGetCameraModeFeature<T>() where T : class, ICameraModeFeature
        {
            foreach (var f in this.features)
            {
                T c = f as T;
                if (!ReferenceEquals(c, null))
                    return c;
            }

            return null;
        }

        /// <summary>
        /// Called when this camera mode is being activated.
        /// This is called after OnEnable.
        /// </summary>
        /// <param name="cameraState">The current camera state.</param>
        public virtual void OnPrepare(CameraState cameraState)
        {

        }

        /// <summary>
        /// Called in order to updaate the camera mode.
        /// Should be called every frame.
        /// </summary>
        /// <param name="camera">The camera state to update.</param>
        public abstract void UpdateMode(ref CameraState cameraState);

        /// <summary>
        /// Post processes the current camera state for this camera mode.
        /// </summary>
        /// <param name="cameraState">The camera state to post process.</param>
        /// <returns>The post processed camera state.</returns>
        public virtual CameraState PostProcessState(CameraState cameraState)
        {
            // Run feature PP
            foreach (var feature in this.features)
                feature.PostProcessState(ref cameraState);

            return cameraState;
        }
    }
}
