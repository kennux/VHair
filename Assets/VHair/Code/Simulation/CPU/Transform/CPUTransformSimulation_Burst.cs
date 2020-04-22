using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Mathematics;
using Unity.Collections;

namespace VHair
{
	/// <summary>
	/// Simulation master object for only transforming the hair vertex buffer to follow the unity engine transform.
	/// This does only support the <see cref="CPUTransformPass_Burst"/> and does not do any real physics simulation!
	/// </summary>
	public class CPUTransformSimulation_Burst : HairSimulation
	{
		public NativeArray<float3> vertices
		{
			get { return this._vertices; }
		}
		private NativeArray<float3> _vertices;
		public NativeArray<float3> initialVertices
		{
			get { return this._initialVertices; }
		}
		private NativeArray<float3> _initialVertices;

		protected void Awake()
		{
			this._vertices = this.instance.asset.CreateVertexDataCopy(Allocator.Persistent);
			this._initialVertices = this.instance.asset.CreateVertexDataCopy(Allocator.Persistent);
		}

		protected void OnDestroy()
		{
			_vertices.Dispose();
			_initialVertices.Dispose();
		}
	}
}
