using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;

namespace UnityTK.Cameras
{
    /// <summary>
    /// <see cref="FreeLookCameraModeInput"/> implementation for desktop devices (mouse input).
    /// </summary>
    public class FreeLookCameraModeDesktopInput : FreeLookCameraModeInput
    {
        public override FreeLookCameraModeInputData GetData()
        {
            return new FreeLookCameraModeInputData()
            {
                lookAxis = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"))
            };
        }
    }
}
