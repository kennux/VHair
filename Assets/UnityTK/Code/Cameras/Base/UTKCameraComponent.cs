using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

namespace UnityTK.Cameras
{
    /// <summary>
    /// UnityTK camera component to be used as base class for implementing <see cref="UTKCamera"/> (child-)behaviours.
    /// </summary>
    public abstract class UTKCameraComponent : MonoBehaviour
    {
        public UTKCamera utkCamera
        {
            get { return this._utkCamera.Get(this, true, false); }
        }
        private LazyLoadedComponentRef<UTKCamera> _utkCamera;

    }
}
