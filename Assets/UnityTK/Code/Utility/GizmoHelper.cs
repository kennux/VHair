using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK
{
    /// <summary>
    /// Static gizmos helper class that contains some useful functionality for drawing editor gizmos.
    /// </summary>
    public static class GizmoHelper
    {
        /// <summary>
        /// Draws an array visualizing a direction on a specific position.
        /// Source: http://wiki.unity3d.com/index.php/DrawArrow
        /// </summary>
        /// <param name="pos">The origin position the direction is relative to.</param>
        /// <param name="direction">The direction the arrow is pointing in.</param>
        /// <param name="arrowHeadLength">The arrow head length</param>
        /// <param name="arrowHeadAngle">The arrow head angle.</param>
        public static void DrawArrow(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }
    }
}