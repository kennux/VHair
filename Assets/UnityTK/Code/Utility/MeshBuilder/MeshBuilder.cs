using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK
{
	/// <summary>
	/// Mesh builder class to be used for procedural mesh generation.
	/// 
	/// <see cref="Start(MeshBuilderChannels, MeshTopology)"/>
	/// <see cref="WriteMeshData(Mesh)"/>
	/// <see cref="Stop"/>
	/// </summary>
	public class MeshBuilder
	{
		public struct VertexData
		{
			public Vector3 position;
			public Vector3? normal;
			public Vector4? tangent;
			public Color? color;
			public Vector4? uv;
			public Vector4? uv2;
			public Vector4? uv3;
			public Vector4? uv4;
			public Vector4? uv5;
			public Vector4? uv6;
			public Vector4? uv7;
			public Vector4? uv8;

			public bool HasDataForChannel(MeshBuilderChannel channel)
			{
				switch (channel)
				{
					case MeshBuilderChannel.COLORS: return this.color.HasValue;
					case MeshBuilderChannel.NORMALS: return this.normal.HasValue;
					case MeshBuilderChannel.TANGENTS: return this.tangent.HasValue;
					case MeshBuilderChannel.UV: return this.uv.HasValue;
					case MeshBuilderChannel.UV2: return this.uv2.HasValue;
					case MeshBuilderChannel.UV3: return this.uv3.HasValue;
					case MeshBuilderChannel.UV4: return this.uv4.HasValue;
					case MeshBuilderChannel.UV5: return this.uv5.HasValue;
					case MeshBuilderChannel.UV6: return this.uv6.HasValue;
					case MeshBuilderChannel.UV7: return this.uv7.HasValue;
					case MeshBuilderChannel.UV8: return this.uv8.HasValue;
				}
				return false;
			}
		}

		/// <summary>
		/// The positions / vertices of this mesh.
		/// </summary>
		private List<Vector3> positions = new List<Vector3>();
		private List<int> indices = new List<int>();
		
		// Channels
		private List<Vector3> normals;
		private List<Vector4> tangents;
		private List<Color> colors;
		private Dictionary<int, List<Vector4>> uvs;

		// Mesh properties
		private Bounds? bounds;
		public MeshTopology currentTopology { get { return this.topology; } }
		private MeshTopology topology = MeshTopology.Triangles;

		// Builder channels
		public MeshBuilderChannels currentChannels { get { return this.channels; } }
		private MeshBuilderChannels channels = MeshBuilderChannels.INVALID;

		private void Setup()
		{
			this.positions.Clear();
			if ((this.channels & MeshBuilderChannels.TANGENTS) > 0)
			{
				if (ReferenceEquals(this.tangents, null))
					this.tangents = new List<Vector4>();
				else
					this.tangents.Clear();
			}

			if ((this.channels & MeshBuilderChannels.NORMALS) > 0)
			{
				if (ReferenceEquals(this.normals, null))
					this.normals = new List<Vector3>();
				else
					this.normals.Clear();
			}

			if ((this.channels & MeshBuilderChannels.COLORS) > 0)
			{
				if (ReferenceEquals(this.colors, null))
					this.colors = new List<Color>();
				else
					this.colors.Clear();
			}

			if (this.channels.HasAnyUV())
			{
				if (ReferenceEquals(this.uvs, null))
					this.uvs = new Dictionary<int, List<Vector4>>();
				else
				{
					foreach (var lst in uvs)
						lst.Value.Clear();
				}

				int idx = 0;
				foreach (var channel in this.channels.AsEnumerable())
				{
					if (channel.TryMapToUVIndex(out idx))
						this.uvs.GetOrCreate(idx);
				}
			}
		}

		public void AddVertex(ref VertexData vertexData)
		{
			if (this.channels == MeshBuilderChannels.INVALID)
				throw new System.InvalidOperationException("Cannot add vertex data to meshbuilder without calling StartBuild first!");

			// Validate layout
			foreach (var channel in this.channels.AsEnumerable())
			{
				if (!vertexData.HasDataForChannel(channel))
					throw new System.InvalidOperationException("Vertex data must contain the data required by the mesh builder! Missing channel: " + channel);
			}

			// Write data
			this.positions.Add(vertexData.position);
			foreach (var channel in this.channels.AsEnumerable())
			{
				switch (channel)
				{
					case MeshBuilderChannel.NORMALS: this.normals.Add(vertexData.normal.Value); break;
					case MeshBuilderChannel.TANGENTS: this.tangents.Add(vertexData.tangent.Value); break;
					case MeshBuilderChannel.COLORS: this.colors.Add(vertexData.color.Value); break;
					case MeshBuilderChannel.UV: this.uvs[0].Add(vertexData.uv.Value); break;
					case MeshBuilderChannel.UV2: this.uvs[1].Add(vertexData.uv2.Value); break;
					case MeshBuilderChannel.UV3: this.uvs[2].Add(vertexData.uv3.Value); break;
					case MeshBuilderChannel.UV4: this.uvs[3].Add(vertexData.uv4.Value); break;
					case MeshBuilderChannel.UV5: this.uvs[4].Add(vertexData.uv5.Value); break;
					case MeshBuilderChannel.UV6: this.uvs[5].Add(vertexData.uv6.Value); break;
					case MeshBuilderChannel.UV7: this.uvs[6].Add(vertexData.uv7.Value); break;
					case MeshBuilderChannel.UV8: this.uvs[7].Add(vertexData.uv8.Value); break;
				}
			}

			// Write index
			this.indices.Add(this.indices.Count);

			if (this.bounds.HasValue)
			{
				var b = this.bounds.Value;
				b.Encapsulate(vertexData.position);
				this.bounds = b;
			}
			else
				this.bounds = new Bounds(vertexData.position, Vector3.zero);
		}

		public void AddGeometry<T>(T geometry) where T : IMeshBuilderGeometry
		{
			if (this.channels == MeshBuilderChannels.INVALID)
				throw new System.InvalidOperationException("Cannot add geometry to meshbuilder without calling StartBuild first!");

			geometry.Build(this);
		}

		/// <summary>
		/// Starts the mesh building process.
		/// This method must be called before adding vertex data or geometry.
		/// 
		/// <see cref="AddVertex(ref VertexData)"/>
		/// <see cref="AddGeometry{T}(T)"/>
		/// 
		/// When done building the mesh, you can write it via <see cref="WriteMeshData(Mesh)"/>.
		/// To reuse the mesh builder, call <see cref="Stop"/>.
		/// </summary>
		/// <param name="channels"></param>
		/// <param name="topology"></param>
		public void Start(MeshBuilderChannels channels, MeshTopology topology = MeshTopology.Triangles)
		{
			if (this.channels != MeshBuilderChannels.INVALID)
				return;

			this.channels = channels;
			this.topology = topology;
			Setup();
		}

		/// <summary>
		/// Writes the data currently inside the builder into the specified mesh.
		/// </summary>
		public void WriteMeshData(Mesh m)
		{
			if (this.positions.Count == 0)
				return;

			// Write data
			m.SetVertices(this.positions);
			m.bounds = this.bounds.Value;
			foreach (var channel in this.channels.AsEnumerable())
			{
				switch (channel)
				{
					case MeshBuilderChannel.NORMALS: m.SetNormals(this.normals); break;
					case MeshBuilderChannel.TANGENTS: m.SetTangents(this.tangents); break;
					case MeshBuilderChannel.COLORS: m.SetColors(this.colors); break;
					case MeshBuilderChannel.UV: m.SetUVs(0, this.uvs[0]); break;
					case MeshBuilderChannel.UV2: m.SetUVs(1, this.uvs[1]); break;
					case MeshBuilderChannel.UV3: m.SetUVs(2, this.uvs[2]); break;
					case MeshBuilderChannel.UV4: m.SetUVs(3, this.uvs[3]); break;
					case MeshBuilderChannel.UV5: m.SetUVs(4, this.uvs[4]); break;
					case MeshBuilderChannel.UV6: m.SetUVs(5, this.uvs[5]); break;
					case MeshBuilderChannel.UV7: m.SetUVs(6, this.uvs[6]); break;
					case MeshBuilderChannel.UV8: m.SetUVs(7, this.uvs[7]); break;
				}
			}

			if (this.topology == MeshTopology.Triangles)
				m.SetTriangles(this.indices, 0); // Optimized call
			else
				m.SetIndices(this.indices.ToArray(), this.topology, 0);
		}

		/// <summary>
		/// Immediately stops a previously started mesh build process.
		/// <see cref="Start(MeshBuilderChannels, MeshTopology)"/>
		/// 
		/// This will drop all data currently in this mesh builder.
		/// After calling this, you can again call <see cref="Start(MeshBuilderChannels, MeshTopology)"/>
		/// </summary>
		public void Stop()
		{
			this.channels = MeshBuilderChannels.INVALID;
			this.bounds = null;
		}
	}
}