using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

namespace VHair
{
    public class CPUTransformSimulation : HairSimulation
    {
        public Vector3[] vertices
        {
            get { return this._vertices; }
        }
        private Vector3[] _vertices;

        protected void Awake()
        {
            this.instance.asset.GetVertexData(out this._vertices);
        }
    }
}
