using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;

namespace UnityTK.Cameras
{
    /// <summary>
    /// <see cref="TopDownCameraModeInput"/> implementation for desktop devices (mouse input).
    /// </summary>
    public class TopDownCameraModeDesktopInput : TopDownCameraModeInput
    {
        public override TopDownCameraModeInputData GetData()
        {
            return new TopDownCameraModeInputData()
            {
                movementDelta = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")),
                zoomDelta = -Input.GetAxisRaw("Mouse ScrollWheel")
            };
        }
    }
}
