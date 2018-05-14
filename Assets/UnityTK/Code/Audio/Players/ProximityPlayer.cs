using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Audio
{
    /// <summary>
    /// Proximity player / manager behaviour.
    /// This manager must be in the scene if you want to use <see cref="ProximityBasedAudio"/>
    /// </summary>
    public class ProximityPlayer : MonoBehaviour
    {
        public static ProximityPlayer instance { get { return UnitySingleton<ProximityPlayer>.Get(); } }

        /// <summary>
        /// The camera used for proximity determination.
        /// </summary>
        [Header("Configuration")]
        public Camera proximityCamera;

        public void Awake()
        {
            UnitySingleton<ProximityPlayer>.Register(this);
        }

        /// <summary>
        /// Calculates the proximity for the specified transform.
        /// Calculates the viewport position for the specified transform and <see cref="proximityCamera"/>.
        /// 
        /// The viewport position is in range -1 to 1 (if in bounds, may be larger if out of bounds) - The z-axis coordinate is dropped.
        /// The resulting position vector's length is the proximity returned by this method.
        /// 
        /// Supports <see cref="Transform"/> and <see cref="RectTransform"/>.
        /// </summary>
        /// <param name="transform">The transformation object to use</param>
        /// <param name="panStereo">Stereo panning (viewport x-axis clamped to -1 to 1).</param>
        /// <returns>The proximity as float.</returns>
        public float GetProximity(Transform transform, out float panStereo)
        {
            RectTransform rTransform = (transform as RectTransform);
            if (ReferenceEquals(rTransform, null))
                rTransform = (transform.parent as RectTransform); // For some (scripting) cases, the transforms are being assigned a rect transform child even tho they are not rect transforms themselves! Here we catch this

            Vector3 v = default(Vector3);
            if (ReferenceEquals(rTransform, null))
            {
                // regular transform
                v = this.proximityCamera.WorldToViewportPoint(transform.position);
            }
            else
            {
                // Rect transform
                v = this.proximityCamera.ScreenToViewportPoint(rTransform.position);
            }

            // Move to -1 to 1 range
            v = (v*2f) - Vector3.one;

            panStereo = Mathf.Clamp(v.x, -1, 1);
            v.z = 0;
            return v.magnitude;
        }
    }
}