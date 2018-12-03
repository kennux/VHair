using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Cameras
{
    /// <summary>
    /// UnityTK camera class.
    /// This is the base component for any cameras using the UnityTK camera module.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class UTKCamera : MonoBehaviour
    {
        /// <summary>
        /// The current camera mode that is used / will be used on awake.
        /// </summary>
        [Header("Start Parameters")]
        public CameraMode currentMode;

        /// <summary>
        /// The current state of the camera.
        /// </summary>
        protected CameraState cameraState;

        /// <summary>
        /// The underlying camera wrapped by this UTKCamera.
        /// </summary>
        public new Camera camera
        {
            get { return this._camera.Get(this); }
        }
        private LazyLoadedComponentRef<Camera> _camera;

        /// <summary>
        /// All modes this camera has avilable.
        /// </summary>
        public ReadOnlyList<CameraMode> modes
        {
            get { return this._modes; }
        }
        private List<CameraMode> _modes = new List<CameraMode>();

        #region Unity Messages

        /// <summary>
        /// Sets up the camera modes and inputs.
        /// <see cref="modes"/>
        /// <see cref="inputs"/>
        /// </summary>
        private void Awake()
        {
            // Read components
            this.GetComponentsInChildren<CameraMode>(this._modes);
            this.cameraState = new CameraState(this.camera);

            // Validity
            if (this._modes.Count == 0)
            {
                Debug.LogError("UnityTK Camera without modes! Disabling camera!", this);
                this.enabled = false;
                return;
            }

            // Set up mode if it isnt set at beginning
            if (Essentials.UnityIsNull(this.currentMode))
                this.currentMode = this.modes[0];

            SetCameraMode(this.currentMode);
        }

        /// <summary>
        /// Updates the camera :>
        /// </summary>
        private void Update()
        {
            // Update mode
            this.currentMode.UpdateMode(ref this.cameraState);

            // PP & Apply
            this.currentMode.PostProcessState(this.cameraState).Apply(this.camera);
        }

        #endregion

        #region Features

        /// <summary>
        /// <see cref="CameraMode.TryGetCameraModeFeature{T}"/> for the <see cref="currentMode"/>.
        /// </summary>
        public T TryGetCameraModeFeature<T>() where T : class, ICameraModeFeature
        {
            return this.currentMode.TryGetCameraModeFeature<T>();
        }

        #endregion

        #region Camera modes

        /// <summary>
        /// Sets the specified camera mode by disabling the current mode and replacing it with the specified mode.
        /// Only nodes known by this camera can be used!
        /// </summary>
        /// <param name="mode">The camera mode to be set.</param>
        public void SetCameraMode(CameraMode mode)
        {
            if (!this._modes.Contains(mode))
                throw new System.ArgumentException(string.Format("Tried to set camera mode which isnt available to the camera: {0}", mode));

            if (!Essentials.UnityIsNull(this.currentMode))
                this.currentMode.gameObject.SetActive(false);

            this.currentMode = mode;
            this.currentMode.gameObject.SetActive(true);
            this.currentMode.OnPrepare(this.cameraState);
        }

        #endregion
    }
}