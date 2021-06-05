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
	public class GPUTFXPhysicsSimulation : HairSimulation
	{
		// Simulation properties
		[HideInInspector]
		public GraphicsBuffer prevFrameVertices;

		[HideInInspector]
		public GraphicsBuffer initialVertices;

		[HideInInspector]
		public Matrix4x4 prevFrameMatrix;

		public GraphicsBuffer strands;
		public GraphicsBuffer vertices;
		public GraphicsBuffer movability;

		public float timestepScale = 1;

		public ComputeShader simulationShader;

		const int GroupSize = 32;

        private void OnValidate()
        {
			simulationShader = Resources.Load<ComputeShader>("TFX_Simulation");
        }

		public void SetBuffers(int kernelId)
		{
			simulationShader.SetBuffer(kernelId, "sPrevFrameVertices", prevFrameVertices);
			simulationShader.SetBuffer(kernelId, "sInitialVertices", initialVertices);
			simulationShader.SetBuffer(kernelId, "sVertices", vertices);
			simulationShader.SetBuffer(kernelId, "sStrands", strands);
			simulationShader.SetBuffer(kernelId, "sMovability", movability);
		}

		public void DispatchShader(int groupKernelId, int singleKernelId, int count)
        {
			int groups = count / GroupSize;
			int singles = count % GroupSize;

			SetBuffers(groupKernelId);
			SetBuffers(singleKernelId);

			simulationShader.Dispatch(groupKernelId, groups, 1, 1);
			simulationShader.Dispatch(singleKernelId, singles, 1, 1);
		}

        protected override void Start()
		{
			base.Start();

			this.prevFrameVertices = new GraphicsBuffer(GraphicsBuffer.Target.Structured | GraphicsBuffer.Target.CopyDestination | GraphicsBuffer.Target.CopySource, instance.vertexCount, 12);
			this.initialVertices = new GraphicsBuffer(GraphicsBuffer.Target.Structured | GraphicsBuffer.Target.CopyDestination | GraphicsBuffer.Target.CopySource, instance.vertexCount, 12);
			this.initialVertices.SetData(instance.asset.CreateVertexDataCopy());
			this.vertices = new GraphicsBuffer(GraphicsBuffer.Target.Structured | GraphicsBuffer.Target.CopyDestination | GraphicsBuffer.Target.CopySource, instance.vertexCount, 12);

			var movability = this.instance.asset.CreateMovabilityDataCopy(Allocator.Temp);
			try
			{
				this.movability = new GraphicsBuffer(GraphicsBuffer.Target.Structured | GraphicsBuffer.Target.CopyDestination | GraphicsBuffer.Target.CopySource, movability.Length, 4);
				this.movability.SetData(movability);
			}
			finally { movability.Dispose(); }

			var strands = this.instance.asset.CreateStrandDataCopy(Allocator.Temp);
			try
			{
				this.strands = new GraphicsBuffer(GraphicsBuffer.Target.Structured | GraphicsBuffer.Target.CopyDestination | GraphicsBuffer.Target.CopySource, instance.strandCount, 8);
				this.strands.SetData(strands);
			}
			finally { strands.Dispose(); }

			// Copy asset data
			this.prevFrameMatrix = this.transform.localToWorldMatrix;

			// Initial vertex transform
			var vertices = this.instance.vertices.CpuReference;
			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i] = this.prevFrameMatrix.MultiplyPoint3x4(vertices[i]);
			}
			this.instance.vertices.SetGPUDirty();

			this.prevFrameVertices.SetData(vertices);
			this.vertices.SetData(vertices);
		}

		protected override void _Update(float timestep)
		{
			simulationShader.SetFloat("pTimestep", timestep * this.timestepScale);

			base._Update(timestep * this.timestepScale);

			Graphics.CopyBuffer(this.vertices, this.instance.vertices.GpuReference);
			this.instance.vertices.SetCPUDirty();
			Graphics.CopyBuffer(this.vertices, this.prevFrameVertices);

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