using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using Unity.Mathematics;

namespace VHair
{
	/// <summary>
	/// A "standard" renderer implementation that does nothing else except synchronizing the simulation data to a mesh generated at runtime.
	/// It will only provide a mesh to a mesh renderer and not do any rendering itself at all.
	/// 
	/// This implementation uses the GPU to calculate the mesh from the spline data.
	/// Use it when the simulation also runs on the GPU.
	/// </summary>
	[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
	public class VHairGPURenderer : HairRenderer
	{
		public enum UVDistributionStrategy : int
		{
			ALONG_STRAND_Y = 0
		}

		[Header("Config")]
		public Transform facing;
		public float hairWidth = .1f;
		public UVDistributionStrategy uvDistStrat;
		public Vector3 fixedNormal;

		[Header("Auto-Referenced")]
		public ComputeShader triangulationShader;

		// Working vars
		private GraphicsBuffer workingVertexBuffer;
		private GraphicsBuffer workingIndexBuffer;
		private GraphicsBuffer vertexBuffer;
		private GraphicsBuffer indexBuffer;

		private new MeshRenderer renderer;
		private MeshFilter filter;
		private Mesh mesh;

		// Constants
		const string KernelTriangulateGroup = "TriangulateGroup";
		const string KernelTriangulateSingle = "TriangulateSingle";
		const int GroupSize = 32;

		struct Vertex
		{
			public float3 position;
			public float3 normal;
			public float3 tangent;
			public float2 uv;
		}


		public override void OnValidate()
        {
			base.OnValidate();
			triangulationShader = Resources.Load<ComputeShader>("Triangulation");
        }

        public void Start()
        {
			int vertexCount = 0;
			int indexCount = 0;

			for (int i = 0; i < instance.strandCount; i++)
            {
				var strand = instance.strands.CpuReference[i];
				int count = (strand.lastVertex - strand.firstVertex) + 1;

				vertexCount += ((count - 1) * 4);
				indexCount += ((count - 1) * 6);
			}

			mesh = new Mesh();

			mesh.vertexBufferTarget |= GraphicsBuffer.Target.CopyDestination;
			mesh.indexBufferTarget |= GraphicsBuffer.Target.CopyDestination;

			mesh.vertices = new Vector3[vertexCount];
			mesh.SetIndexBufferParams(indexCount, IndexFormat.UInt32);
			mesh.SetVertexBufferParams(vertexCount, new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
				new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
				new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 3), 
				new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2));
			mesh.SetSubMesh(0, new SubMeshDescriptor(0, indexCount, MeshTopology.Triangles), MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontResetBoneBounds);

			vertexBuffer = mesh.GetVertexBuffer(0);
			indexBuffer = mesh.GetIndexBuffer();

			workingVertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured | GraphicsBuffer.Target.CopySource, vertexBuffer.count, vertexBuffer.stride);
			workingIndexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured | GraphicsBuffer.Target.CopySource, indexBuffer.count, indexBuffer.stride);

			Update();
			LateUpdate();

			Bounds b = default;
			Vertex[] vertices = new Vertex[vertexBuffer.count];
			workingVertexBuffer.GetData(vertices);
			bool first = true;
			foreach (var vertex in vertices)
            {
				if (first)
				{
					b = new Bounds(vertex.position, Vector3.zero);
					first = false;
				}
				else
					b.Encapsulate(vertex.position);
            }

			mesh.bounds = b;
			this.renderer = GetComponent<MeshRenderer>();
			this.filter = GetComponent<MeshFilter>();
			this.filter.sharedMesh = mesh;
		}

        public void Update()
		{
			int groupKernel = triangulationShader.FindKernel(KernelTriangulateGroup);
			int singleKernel = triangulationShader.FindKernel(KernelTriangulateSingle);

			void SetBuffers(int kernel)
			{
				triangulationShader.SetBuffer(kernel, "rVertexBuffer", workingVertexBuffer);
				triangulationShader.SetBuffer(kernel, "rIndexBuffer", workingIndexBuffer);
				triangulationShader.SetBuffer(kernel, "pStrands", instance.strands.GpuReference);
				triangulationShader.SetBuffer(kernel, "pVertices", instance.vertices.GpuReference);
			}

			SetBuffers(groupKernel);
			SetBuffers(singleKernel);
			triangulationShader.SetVector("pFacingPos", facing.position);
			triangulationShader.SetMatrix("pWorldToLocal", transform.worldToLocalMatrix);
			triangulationShader.SetVector("pFixedNormal", fixedNormal);
			triangulationShader.SetFloat("pHairWidth", hairWidth);
			triangulationShader.SetInt("pUvDistStrategy", (int)uvDistStrat);

			int groupCalls = instance.strandCount / GroupSize;
			int singleCalls = instance.strandCount % GroupSize;

			triangulationShader.Dispatch(groupKernel, groupCalls, 1, 1);
			triangulationShader.Dispatch(singleKernel, singleCalls, 1, 1);
		}

        public void LateUpdate()
		{
			Graphics.CopyBuffer(workingIndexBuffer, indexBuffer);
			Graphics.CopyBuffer(workingVertexBuffer, vertexBuffer);
		}

        public void OnDestroy()
        {
			workingIndexBuffer.Dispose();
			workingVertexBuffer.Dispose();
			vertexBuffer.Dispose();
			indexBuffer.Dispose();
        }
    }
}