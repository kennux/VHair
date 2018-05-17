using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VHair
{
    public class CPUPhysicsSimulation : HairSimulation
    {
        // Simulation properties
        [HideInInspector]
        public Vector3[] prevFrameVertices;
        [HideInInspector]
        public Vector3[] initialVertices;

        [HideInInspector]
        public Matrix4x4 prevFrameMatrix;

        protected void Start()
        {
            this.instance.asset.GetVertexData(out this.prevFrameVertices);
            this.instance.asset.GetVertexData(out this.initialVertices);
            this.prevFrameMatrix = this.transform.localToWorldMatrix;

            // Initial vertex transform
            var vertices = this.instance.vertices.cpuReference;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = this.prevFrameMatrix.MultiplyPoint3x4(vertices[i]);
            }
        }

        protected override void _Update(float timestep)
        {
            base._Update(timestep);

            System.Array.Copy(this.instance.vertices.cpuReference, this.prevFrameVertices, this.instance.asset.vertexCount);
            this.prevFrameMatrix = this.transform.localToWorldMatrix;
        }
    }
}