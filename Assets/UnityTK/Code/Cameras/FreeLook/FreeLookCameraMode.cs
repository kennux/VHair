using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityTK.Cameras
{
    /// <summary>
    /// Camera mode implementation for a free look camera.
    /// </summary>
    public class FreeLookCameraMode : CameraModeBase<FreeLookCameraModeInputData>
    {
        /// <summary>
        /// The minimum rotation of the camera (euler x-angle).
        /// This controls the limits of looking up/down.
        /// </summary>
        [Header("Settings")]
        public float minRotation = -45;

        /// <summary>
        /// The maximum rotation of the camera (euler x-angle).
        /// This controls the limits of looking up/down.
        /// </summary>
        public float maxRotation = 45;

        /// <summary>
        /// The camera look sensitivity
        /// </summary>
        public float sensitivity = 10;

        /// <summary>
        /// Internal camera pitch value (euler x-angle).
        /// </summary>
        private float pitch;

        protected override FreeLookCameraModeInputData MergeInputData(Dictionary<CameraModeInput<FreeLookCameraModeInputData>, FreeLookCameraModeInputData> data)
        {
            FreeLookCameraModeInputData fli = new FreeLookCameraModeInputData();
            foreach (var val in data.Values)
            {
                fli.lookAxis += val.lookAxis;
            }

            return fli;
        }

        protected override void _UpdateMode(ref CameraState cameraState)
        {
            var euler = cameraState.transform.localEulerAngles;
            pitch = euler.x = Mathf.Clamp((pitch - this.inputData.lookAxis.y * this.sensitivity * Time.deltaTime), this.minRotation, this.maxRotation);
            cameraState.transform.localEulerAngles = euler;

            // Rotation
            euler = cameraState.transform.localEulerAngles;
            euler.y += this.inputData.lookAxis.x * this.sensitivity * Time.deltaTime;
            euler.y = euler.y < -360 ? euler.y + 360 : euler.y;
            euler.y = euler.y > 360 ? euler.y - 360 : euler.y;
            cameraState.transform.localEulerAngles = euler;
        }
    }
}
