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
		public HairRenderer hairRenderer;

		private new MeshRenderer renderer;
		private MeshFilter filter;

		private MeshFilter rendererFilter;

		private void Awake()
		{
			renderer = GetComponent<MeshRenderer>();
			filter = GetComponent<MeshFilter>();

			rendererFilter = hairRenderer.GetComponent<MeshFilter>();
		}

		public void LateUpdate()
		{
			filter.sharedMesh = rendererFilter.sharedMesh;
		}
	}
}
