using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Cameras
{
    /// <summary>
    /// Camera mode implementation for top down cameras.
    /// This includes traditional top down cameras aswell as isometric camera perspectives (TODO).
    /// 
    /// It implements camera movement along a 2d plane in 3d space, which can be moved on the y-axis withing certain limits to create the zoom behaviour.
    /// </summary>
    public class TopDownCameraMode : CameraModeBase<TopDownCameraModeInputData>
    {
#if UNITY_EDITOR
        private void OnValidate()
        {
            this.minYZoomLevel = Mathf.Clamp(this.minYZoomLevel, this.bounds.min.y, this.bounds.max.y);
            this.maxYZoomLevel = Mathf.Clamp(this.maxYZoomLevel, this.bounds.min.y, this.bounds.max.y);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(this.bounds.center, this.bounds.size);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(new Vector3(this.bounds.center.x, this.minYZoomLevel, this.bounds.center.z), new Vector3(this.bounds.size.x, 0.05f, this.bounds.size.z));
            Gizmos.DrawWireCube(new Vector3(this.bounds.center.x, this.maxYZoomLevel, this.bounds.center.z), new Vector3(this.bounds.size.x, 0.05f, this.bounds.size.z));
        }
#endif
        /// <summary>
        /// The bounding box of the world to be viewed with this camera.
        /// This controls the camera plane used for movement.
        /// After changing this at runtime, call <see cref="UpdatePlane"/> explicitly.
        /// </summary>
        [Header("Config")]
        public Bounds bounds;

        /// <summary>
        /// Y-axis min for camera when fully zoomed in.
        /// </summary>
        public float minYZoomLevel;

        /// <summary>
        /// Y-axis max for camera when fully zoomed out.
        /// </summary>
        public float maxYZoomLevel;

        /// <summary>
        /// Euler angles to be always set to the camera.
        /// </summary>
        public Vector3 eulerAngles = new Vector3(90, 0, 0);

        /// <summary>
        /// The movement sensitivity (multiplicator)
        /// </summary>
        public float movementSensitivity = 100f;

        /// <summary>
        /// The zoom sensitivity (multiplicator)
        /// </summary>
        public float zoomSensitivity = 25f;

        /// <summary>
        /// The zoom level in worldspace (y-axis).
        /// </summary>
        protected float zoomLevel
        {
            get { return this.minYZoomLevel + (this.maxYZoomLevel * this.zoomLevelNormalized); }
        }
        
        [Header("Debug")]
        [SerializeField]
        /// <summary>
        /// The normalized zoomlevel, normalized between <see cref="minYZoomLevel"/>, <see cref="maxYZoomLevel"/>
        /// </summary>
        protected float zoomLevelNormalized;
        
        [SerializeField]
        /// <summary>
        /// The min vector of the 2d plane used for movement.
        /// </summary>
        protected Vector2 planeMin;
        
        [SerializeField]
        /// <summary>
        /// The mmax vector of the 2d plane used for movement.
        /// </summary>
        protected Vector2 planeMax;
        
        [SerializeField]
        /// <summary>
        /// The camera position on a plane defined by <see cref="planeMin"/> and <see cref="planeMax"/>
        /// </summary>
        protected Vector2 planeCoords;

        protected override TopDownCameraModeInputData MergeInputData(Dictionary<CameraModeInput<TopDownCameraModeInputData>, TopDownCameraModeInputData> data)
        {
            TopDownCameraModeInputData id = new TopDownCameraModeInputData();
            foreach (var d in data.Values)
            {
                id.movementDelta += d.movementDelta;
                id.zoomDelta += d.zoomDelta;
            }

            return id;
        }

        /// <summary>
        /// Updates the internal movement plane.
        /// <seealso cref="bounds"/>
        /// </summary>
        public void UpdatePlane()
        {
            this.planeMin = new Vector2(bounds.min.x, bounds.min.z);
            this.planeMax = new Vector2(bounds.max.x, bounds.max.z);
        }

        private void OnEnable()
        {
            UpdatePlane();
        }

        public override void OnPrepare(CameraState camera)
        {
            base.OnPrepare(camera);

            // Try to get plane coords and clamp them
            this.planeCoords = new Vector2(camera.transform.position.x, camera.transform.position.z);
            this.zoomLevelNormalized = camera.transform.position.y.Remap(this.minYZoomLevel, this.maxYZoomLevel, 0, 1);
            ClampState();
        }

        /// <summary>
        /// Clamps:
        /// - <see cref="planeCoords"/> to <see cref="planeMin"/> and <see cref="planeMax"/>
        /// - <see cref="zoomLevelNormalized"/> to 0-1
        /// </summary>
        private void ClampState()
        {
            // TODO: Plane coords camer AABB check for bounds
            this.planeCoords.x = Mathf.Clamp(this.planeCoords.x, planeMin.x, planeMax.x);
            this.planeCoords.y = Mathf.Clamp(this.planeCoords.y, planeMin.y, planeMax.y);
            this.zoomLevelNormalized = Mathf.Clamp01(this.zoomLevelNormalized);
        }

        protected override void _UpdateMode(ref CameraState cameraState)
        {
            // Logical update
            this.planeCoords += this.inputData.movementDelta * movementSensitivity * Time.deltaTime;
            this.zoomLevelNormalized += this.inputData.zoomDelta * zoomSensitivity * Time.deltaTime;
            ClampState();

            // Visual update
            // Calculate position determination ray
            Vector3 rayOrigin = new Vector3(this.planeCoords.x, this.bounds.min.y, this.planeCoords.y);
            Quaternion rotation = Quaternion.Euler(this.eulerAngles);
            Ray ray = new Ray(rayOrigin, rotation * -Vector3.forward); // Create ray from ground towards camera position

            // Cast position determination ray against target plane
            float d;
            if (new Plane(Vector3.up, new Vector3(this.planeCoords.x, this.zoomLevel, this.planeCoords.y)).Raycast(ray, out d))
            {
                var p = ray.GetPoint(d);
                cameraState.transform.position = p;
            }

            cameraState.transform.rotation = rotation;
        }
    }
}