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
	[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
	public class VHairProxyRenderer : MonoBehaviour
	{
		public VHairCPURenderer hairRenderer;

		private new MeshRenderer renderer;
		private MeshFilter filter;

		private void Awake()
		{
			renderer = GetComponent<MeshRenderer>();
			filter = GetComponent<MeshFilter>();
		}

		public void LateUpdate()
		{
			filter.sharedMesh = hairRenderer.Mesh;
		}
	}
}
