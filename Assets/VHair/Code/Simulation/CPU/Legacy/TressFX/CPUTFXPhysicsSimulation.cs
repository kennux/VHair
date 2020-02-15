using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace VHair
{
    /// <summary>
    /// Base class for cpu based physics simulation.
    /// </summary>
    public class CPUTFXPhysicsSimulation : HairSimulation
    {
        // Simulation properties
        [HideInInspector]
        public NativeArray<float3> prevFrameVertices;

        [HideInInspector]
        public NativeArray<float3> initialVertices;

        [HideInInspector]
        public Matrix4x4 prevFrameMatrix;

		public float timestepScale = 1;

        protected override void Start()
        {
            base.Start();

            this.prevFrameVertices = this.instance.asset.CreateVertexDataCopy(Allocator.Persistent);
            this.initialVertices = this.instance.asset.CreateVertexDataCopy(Allocator.Persistent);
            this.prevFrameMatrix = this.transform.localToWorldMatrix;

            // Initial vertex transform
            var vertices = this.instance.vertices.CpuReference;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = this.prevFrameMatrix.MultiplyPoint3x4(vertices[i]);
            }
        }

        protected override void _Update(float timestep)
        {
            base._Update(timestep * this.timestepScale);

			this.instance.vertices.CpuReference.CopyTo(this.prevFrameVertices);
            this.prevFrameMatrix = this.transform.localToWorldMatrix;
        }

		protected void OnDestroy()
		{
			if (prevFrameVertices.IsCreated)
				prevFrameVertices.Dispose();
			if (initialVertices.IsCreated)
				initialVertices.Dispose();
		}
    }
}