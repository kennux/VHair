using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Jobs;

namespace VHair
{
    /// <summary>
    /// Base class for cpu based physics simulation.
    /// </summary>
    public class CPUTFXPhysicsSimulation_Burst : HairSimulation
    {
        // Simulation properties
        [HideInInspector]
        public NativeArray<float3> prevFrameVertices;
		private Vector3[] _prevFrameVertices;

        [HideInInspector]
        public NativeArray<float3> initialVertices;
		private Vector3[] _initialVertices;

        [HideInInspector]
        public Matrix4x4 prevFrameMatrix;
		
		public NativeArray<HairStrand> strands;
		public NativeArray<float3> vertices;
		public NativeArray<uint> movability;

		public float timestepScale = 1;

		public static unsafe void CopyVectorArray(Vector3[] from, NativeArray<float3> to)
		{
			fixed (Vector3* fromPtr = from)
			{
				UnsafeUtility.MemCpy(NativeArrayUnsafeUtility.GetUnsafePtr(to), (void*)fromPtr, sizeof(Vector3) * from.Length);
			}
		}

		public static unsafe void CopyVectorArray(NativeArray<float3> from, Vector3[] to)
		{
			fixed (Vector3* toPtr = to)
			{
				UnsafeUtility.MemCpy((void*)toPtr, NativeArrayUnsafeUtility.GetUnsafePtr(from), sizeof(Vector3) * from.Length);
			}
		}

        protected override void Start()
        {
            base.Start();
			
			this._prevFrameVertices = this.instance.asset.GetVertexData();
			this._initialVertices = this.instance.asset.GetVertexData();
            this.prevFrameVertices = new NativeArray<float3>(this._prevFrameVertices.Length, Allocator.Persistent);
            this.initialVertices = new NativeArray<float3>(this._initialVertices.Length, Allocator.Persistent);

			// Copy asset data
            this.vertices = new NativeArray<float3>(this._initialVertices.Length, Allocator.Persistent);
            this.strands = new NativeArray<HairStrand>(this.instance.asset.GetStrandData(), Allocator.Persistent);
            this.movability = new NativeArray<uint>(this.instance.asset.GetMovabilityData(), Allocator.Persistent);
            this.prevFrameMatrix = this.transform.localToWorldMatrix;

			// Setup native arrays
			CopyVectorArray(this._prevFrameVertices, this.prevFrameVertices);
			CopyVectorArray(this._initialVertices, this.initialVertices);
			CopyVectorArray(this._initialVertices, this.vertices);

            // Initial vertex transform
            var vertices = this.instance.vertices.cpuReference;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = this.prevFrameMatrix.MultiplyPoint3x4(vertices[i]);
            }
        }

		public JobHandle jobHandle;
        protected override void _Update(float timestep)
        {
			this.jobHandle = default(JobHandle);

            base._Update(timestep * this.timestepScale);

			JobHandle.ScheduleBatchedJobs();
			this.jobHandle.Complete();
			CopyVectorArray(this.vertices, this.instance.vertices.cpuReference);
			this.instance.vertices.SetGPUDirty();
			this.vertices.CopyTo(this.prevFrameVertices);
            this.prevFrameMatrix = this.transform.localToWorldMatrix;
        }

		public void OnDestroy()
		{
			this.vertices.Dispose();
			this.strands.Dispose();
			this.movability.Dispose();
			this.prevFrameVertices.Dispose();
			this.initialVertices.Dispose();
		}
	}
}