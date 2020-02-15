using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace VHair
{
	/// <summary>
	/// The data saved in the asset should _NEVER_ be changed at runtime, but instead be used as a source from where to create a copy (instance).
	/// </summary>
	[CreateAssetMenu(fileName = "HairAsset", menuName = "VHair/Hair Asset")]
	public class HairAsset : ScriptableObject
	{
		/// <summary>
		/// The vertices used for the hair.
		/// </summary>
		[SerializeField]
		private float3[] vertices;

		/// <summary>
		/// The strands of this hair asset.
		/// </summary>
		[SerializeField]
		private HairStrand[] strands;

		/// <summary>
		/// The movability bits of the hair.
		/// Bit index maps to <see cref="VertexCount"/>, <see cref="HairMovability"/> for the utilities to access this data.
		/// </summary>
		[SerializeField]
		private uint[] movability;

		/// <summary>
		/// Whether or not this asset already has data imported.
		/// </summary>
		public bool wasImported = false;

		/// <summary>
		/// The amount of strands in this hair asset.
		/// </summary>
		public int StrandCount
		{
			get { return this.strands.Length; }
		}

		/// <summary>
		/// The amount of vertices in this hair asset.
		/// </summary>
		public int VertexCount
		{
			get { return this.vertices.Length; }
		}

		/// <summary>
		/// Allocates and returns a copy of <see cref="vertices"/>
		/// </summary>
		public float3[] CreateVertexDataCopy()
		{
			float3[] vertices = new float3[this.vertices.Length];
			System.Array.Copy(this.vertices, vertices, vertices.Length);
			return vertices;
		}

		/// <summary>
		/// Allocates and returns a copy of <see cref="vertices"/>, as <see cref="Unity.Collections.NativeArray{T}"/>
		/// </summary>
		public NativeArray<float3> CreateVertexDataCopy(Allocator allocator)
		{
			return new NativeArray<float3>(this.vertices, allocator);
		}

		/// <summary>
		/// Allocates and returns a copy of <see cref="strands"/>
		/// </summary>
		public HairStrand[] CreateStrandDataCopy()
		{
			HairStrand[] strands = new HairStrand[this.strands.Length];
			System.Array.Copy(this.strands, strands, strands.Length);
			return strands;
		}

		/// <summary>
		/// Allocates and returns a copy of <see cref="strands"/>
		/// </summary>
		public NativeArray<HairStrand> CreateStrandDataCopy(Allocator allocator)
		{
			return new NativeArray<HairStrand>(this.strands, allocator);
		}

		/// <summary>
		/// Creates a copy of <see cref="movability"/>
		/// </summary>
		/// <param name="movability"></param>
		public uint[] CreateMovabilityDataCopy()
		{
			uint[] movability = new uint[Mathf.CeilToInt(this.vertices.Length / 32f)];
			System.Array.Copy(this.movability, movability, movability.Length);
			return movability;
		}

		public NativeArray<uint> CreateMovabilityDataCopy(Allocator allocator)
		{
			return new NativeArray<uint>(this.movability, allocator);
		}

		/// <summary>
		/// AiO version of:
		/// <see cref="CreateVertexDataCopy"/>
		/// <see cref="CreateStrandDataCopy"/>
		/// <see cref="CreateMovabilityDataCopy()"/>
		/// </summary>
		public void GetRawDataCopy(out float3[] vertices, out HairStrand[] strands, out uint[] movability)
		{
			vertices = CreateVertexDataCopy();
			strands = CreateStrandDataCopy();
			movability = CreateMovabilityDataCopy();
		}

		/// <summary>
		/// Initializes (reallocates and overwrites internal-) vertex data.
		/// </summary>
		/// <param name="vertices">The vertex data to write to the asset.</param>
		public void InitializeVertices(float3[] vertices)
		{
			this.vertices = new float3[vertices.Length];
			vertices.CopyTo(this.vertices, 0);
		}

		/// <summary>
		/// Initializes (reallocates and overwrites internal- ) strand data.
		/// </summary>
		/// <param name="strands">The strand data to write to the asset.</param>
		public void InitializeStrands(HairStrand[] strands)
		{
			this.strands = new HairStrand[strands.Length];
			strands.CopyTo(this.strands, 0);
		}

		/// <summary>
		/// Initializes (reallocates and overwrites internal- ) movability data.
		/// </summary>
		/// <param name="movability">The mobility data to write, if null every vertex is marked as immovable.</param>
		public void InitializeMovability(uint[] movability = null)
		{
			this.movability = HairMovability.CreateData(vertices.Length);
			
			if (movability != null)
			{
				if (movability.Length != this.movability.Length)
					throw new System.ArgumentException("Incorrect movability array size, length should be ceil(vertexCount / 32f)");
				movability.CopyTo(this.movability, 0);
			}
		}
	}
}