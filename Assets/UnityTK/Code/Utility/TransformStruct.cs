using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

namespace UnityTK
{
    /// <summary>
    /// A data structure resembling <see cref="UnityEngine.Transform"/>.
    /// </summary>
    public struct TransformStruct
    {
        public Transform parent;

        public Vector3 position
        {
            get { return Essentials.UnityIsNull(this.parent) ? this.localPosition : this.parent.TransformPoint(this.localPosition); }
            set { this.localPosition = Essentials.UnityIsNull(this.parent) ? value : this.parent.InverseTransformPoint(value); }
        }

        public Quaternion rotation
        {
            get { return Essentials.UnityIsNull(this.parent) ? this.localRotation : this.parent.rotation * this.localRotation; }
            set { this.localRotation = Essentials.UnityIsNull(this.parent) ? value : this.parent.rotation * this.localRotation; }
        }

        public Vector3 localPosition;
        public Quaternion localRotation;

        public Vector3 localEulerAngles
        {
            get { return this.localRotation.eulerAngles; }
            set { this.localRotation.eulerAngles = value; }
        }

        public TransformStruct(Transform t)
        {
            this.parent = t.parent;
            this.localPosition = t.localPosition;
            this.localRotation = t.localRotation;
        }

        public void Apply(Transform t)
        {
            t.parent = this.parent;
            t.localPosition = this.localPosition;
            t.localRotation = this.localRotation;
        }
    }
}
