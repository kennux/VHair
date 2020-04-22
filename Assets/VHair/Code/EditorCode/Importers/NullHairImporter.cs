using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityTK;
using Unity.Mathematics;

namespace VHair.Editor
{
	public class NullHairImporter : IHairAssetImporter
	{
		public string displayName => "Null importer";

		public void Import(out float3[] vertices, out HairStrand[] strands)
		{
			vertices = new float3[0];
			strands = new HairStrand[0];
		}

		public bool OnInspectorGUI(HairAsset hairAsset)
		{
			return true;
		}
	}
}
