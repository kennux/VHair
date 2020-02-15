using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Mathematics;

namespace VHair
{
	/// <summary>
	/// Simulation master object for only transforming the hair vertex buffer to follow the unity engine transform.
	/// This does only support the <see cref="CPUTransformPass"/> and does not do any real physics simulation!
	/// </summary>
	public class CPUTransformSimulation : HairSimulation
	{
		public float3[] vertices
		{
			get { return this._vertices; }
		}
		private float3[] _vertices;

		protected void Awake()
		{
			this._vertices = this.instance.asset.CreateVertexDataCopy();
		}
	}
}
