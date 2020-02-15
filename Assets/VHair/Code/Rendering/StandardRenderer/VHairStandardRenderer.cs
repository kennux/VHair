using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Burst;
using System.Threading;

namespace VHair
{
	/// <summary>
	/// A "standard" renderer implementation that does nothing else except synchronizing the simulation data to a mesh generated at runtime.
	/// It will only provide a mesh to a mesh renderer and not do any rendering itself at all.
	/// </summary>
	[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
	public class VHairStandardRenderer : HairRenderer
	{
		public enum UVDistributionStrategy
		{
			ALONG_STRAND_Y
		}

		struct TriangulatedSegment
		{
			public int hairVertexIndex1;
			public int hairVertexIndex2;
			
			public int strandVertexIndex1;
			public int strandVertexIndex2;
			public int strandIndex;
			
			// v1-------v2
			// |		|
			// |		|
			// |		|
			// v3-------v4
			public int v1;
			public int v2;
			public int v3;
			public int v4;
		}

		[BurstCompile]
		unsafe struct PositionUpdateJob : IJobParallelFor
		{
			public float3 facing;
			public float4x4 worldToLocal;
			public float hairWidth;

			[ReadOnly]
			public NativeArray<float3> hairVertices;
			[ReadOnly]
			public NativeArray<TriangulatedSegment> segments;
			
			[NativeDisableParallelForRestriction]
			[WriteOnly]
			public NativeArray<float3> vertices;

			[NativeDisableParallelForRestriction]
			[WriteOnly]
			public NativeArray<float3> normals;

			public void Execute(int index)
			{
				TriangulatedSegment segment = this.segments[index];
				float3 v1, v2, v3, v4, n1, n2, n3, n4, p1, p2;
				
				p1 = hairVertices[segment.hairVertexIndex1];
				p2 = hairVertices[segment.hairVertexIndex2];

				float3 p2To1 = (p2 - p1);
				float3 facingDir = ((p2 - facing) + (p1 - facing)) / 2f;

				float3 left = math.normalize(math.cross(p2To1, facingDir));
				v1 = p1 + hairWidth * left;
				v2 = p1 + hairWidth * -left;
				v3 = p2 + hairWidth * left;
				v4 = p2 + hairWidth * -left;
				
				v1 = math.mul(worldToLocal, new float4(v1.x, v1.y, v1.z, 1)).xyz;
				v2 = math.mul(worldToLocal, new float4(v2.x, v2.y, v2.z, 1)).xyz;
				v3 = math.mul(worldToLocal, new float4(v3.x, v3.y, v3.z, 1)).xyz;
				v4 = math.mul(worldToLocal, new float4(v4.x, v4.y, v4.z, 1)).xyz;

				vertices[segment.v1] = v1;
				vertices[segment.v2] = v2;
				vertices[segment.v3] = v3;
				vertices[segment.v4] = v4;

				n1 = n2 = n3 = n4 = math.mul(quaternion.LookRotation(math.normalize(p2 - p1), new float3(0,1,0)), new float3(0,1,0));
				normals[segment.v1] = n1;
				normals[segment.v2] = n2;
				normals[segment.v3] = n3;
				normals[segment.v4] = n4;

			}
		}

		public Transform facing;
		public float hairWidth = .1f;
		public UVDistributionStrategy uvDistStrat;
		
		private new MeshRenderer renderer;
		private MeshFilter filter;
		private Mesh mesh;

		public Mesh Mesh => mesh;
		
		private NativeArray<float3> splineVertices;
		private NativeArray<float3> triVertices;
		private NativeArray<float2> uvs;
		private NativeArray<float3> normals;
		private NativeArray<int> indices;
		private NativeArray<TriangulatedSegment> segments;

		public void Awake()
		{
			this.renderer = GetComponent<MeshRenderer>();
			this.filter = GetComponent<MeshFilter>();
			this.mesh = new Mesh();
			this.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			this.mesh.MarkDynamic();
			this.filter.sharedMesh = this.mesh;
		}

		public void Start()
		{
			List<TriangulatedSegment> segments = new List<TriangulatedSegment>();

			// Triangulate
			var strands = this.instance.strands.CpuReference;
			{
				int vCtr = 0;
				for (int sIndex = 0; sIndex < strands.Length; sIndex++)
				{
					HairStrand strand = strands[sIndex];
					for (int vIndex = strand.firstVertex; vIndex < strand.lastVertex - 1; vIndex++)
					{
						// Build segment
						TriangulatedSegment segment = new TriangulatedSegment()
						{
							hairVertexIndex1 = vIndex,
							hairVertexIndex2 = vIndex + 1,
							v1 = vCtr++,
							v2 = vCtr++,
							v3 = vCtr++,
							v4 = vCtr++,
							strandIndex = sIndex,
							strandVertexIndex1 = vIndex-strand.firstVertex,
							strandVertexIndex2 = (vIndex+1)-strand.firstVertex
						};

						segments.Add(segment);
					}
				}
			}
			this.segments = new NativeArray<TriangulatedSegment>(segments.Count, Allocator.Persistent);
			this.segments.CopyFrom(segments.ToArray());

			// Distribute uv and indices
			this.uvs = new NativeArray<float2>(segments.Count * 4, Allocator.Persistent);
			this.normals = new NativeArray<float3>(segments.Count * 4, Allocator.Persistent);
			this.triVertices = new NativeArray<float3>(segments.Count * 4, Allocator.Persistent);
			this.indices = new NativeArray<int>(segments.Count * 6, Allocator.Persistent);
			
			for (int i = 0; i < segments.Count; i++)
			{
				var segment = segments[i];

				// Calc uv
				float2 uv1, uv2, uv3, uv4;
				uv1 = uv2 = uv3 = uv4 = float2.zero;
				switch (this.uvDistStrat)
				{
					case UVDistributionStrategy.ALONG_STRAND_Y:
						{
							HairStrand hs = strands[segment.strandIndex];
							float y = segment.strandVertexIndex1 / (hs.lastVertex - hs.firstVertex);
							float y2 = segment.strandVertexIndex2 / (hs.lastVertex - hs.firstVertex);
							
							uv1 = new float2(0, y);
							uv2 = new float2(1, y);
							uv3 = new float2(0, y2);
							uv4 = new float2(1, y2);
						}
						break;
				}
				
				this.uvs[segment.v1] = uv1;
				this.uvs[segment.v2] = uv2;
				this.uvs[segment.v3] = uv3;
				this.uvs[segment.v4] = uv4;

				// Write indices
				this.indices[(i * 6)] = segment.v1;
				this.indices[(i * 6)+1] = segment.v2;
				this.indices[(i * 6)+2] = segment.v4;
				this.indices[(i * 6)+3] = segment.v4;
				this.indices[(i * 6)+4] = segment.v3;
				this.indices[(i * 6)+5] = segment.v1;
			}
			
			this.uvs.CopyFrom(uvs.ToArray());
			this.indices.CopyFrom(indices.ToArray());

			this.splineVertices = new NativeArray<float3>(this.instance.asset.VertexCount, Allocator.Persistent);

			// Init mesh
			this.mesh.SetVertices<float3>(triVertices);
			this.mesh.SetUVs<float2>(0, uvs);
			this.mesh.SetNormals<float3>(normals);
			this.mesh.SetIndices<int>(indices, MeshTopology.Triangles, 0);

			LateUpdate();
			this.mesh.RecalculateBounds();
			this.mesh.bounds = new Bounds(this.mesh.bounds.center, this.mesh.bounds.size * 3f); // Approximation, real bounding box calculation is difficult to do fast.
		}

		private JobHandle handle;
		public unsafe void LateUpdate()
		{
			this.splineVertices.CopyFrom(this.instance.vertices.CpuReference);

			handle = new PositionUpdateJob()
			{
				normals = this.normals,
				segments = this.segments,
				hairVertices = this.splineVertices,
				vertices = this.triVertices,
				facing = this.facing.position,
				hairWidth = this.hairWidth,
				worldToLocal = this.transform.worldToLocalMatrix
			}.Schedule(this.segments.Length, 16);

			JobHandle.ScheduleBatchedJobs();
			handle.Complete();

			mesh.SetVertices<float3>(this.triVertices);
			mesh.SetNormals<float3>(this.normals);
			this.mesh.UploadMeshData(false);
		}

		public void OnDestroy()
		{
			this.triVertices.Dispose();
			this.uvs.Dispose();
			this.normals.Dispose();
			this.indices.Dispose();
			this.splineVertices.Dispose();
			this.segments.Dispose();
		}
	}
}
