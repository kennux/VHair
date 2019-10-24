using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace VHair.Editor
{
	public class IncreaseHairDensityProcessor : IHairAssetProcessor
	{
		public string Name => "Increase density";

		public string Description => "Makes the hair more dense by spawning new hair with an offset to the existing ones";

		//Params
		public int newHairPerHair = 2;
		public float maxOffsetRadius = 0.01f;

		public void OnGUI()
		{
			newHairPerHair = EditorGUILayout.IntField("New hair per hair", newHairPerHair);
			maxOffsetRadius = EditorGUILayout.FloatField("Max offset radius", maxOffsetRadius);
		}

		public void Run(HairAsset asset)
		{
			asset.GetRawDataCopy(out Vector3[] originalVertices, out HairStrand[] originalStrands, out uint[] originalMovability);
			List<Vector3> newVertices = new List<Vector3>();
			List<HairStrand> newStrands = new List<HairStrand>();
			BitArray movability = new BitArray(originalVertices.Length * (newHairPerHair + 1));
			for (int i = 0; i < originalMovability.Length; i++)
			{
				for (int j = 0; j < 32; j++)
					movability[(i * 32) + j] = HairMovability.IsMovable((i * 32) + j, originalMovability);
			}

			for (int i = 0; i < originalStrands.Length; i++)
			{
				float rLeft = UnityEngine.Random.value * maxOffsetRadius, rUp = UnityEngine.Random.value * maxOffsetRadius;
				var origStrand = originalStrands[i];
				if (origStrand.lastVertex - origStrand.firstVertex < 2)
					continue;
				for (int j = 0; j < newHairPerHair; j++)
				{
					int firstVertex = newVertices.Count + originalVertices.Length;
					for (int k = origStrand.firstVertex; k <= origStrand.lastVertex; k++)
					{
						Quaternion orientation = k == origStrand.lastVertex ? Quaternion.LookRotation(originalVertices[k] - originalVertices[k-1]) : Quaternion.LookRotation(originalVertices[k+1] - originalVertices[k]);
						Vector3 left = orientation * Vector3.left;
						Vector3 up = orientation * Vector3.up;
						Vector3 offset = (rLeft * left) + (rUp * up);
						movability[originalVertices.Length + newVertices.Count] = movability[k];
						newVertices.Add(originalVertices[k] + offset);
					}
					newStrands.Add(new HairStrand() { firstVertex = firstVertex, lastVertex = newVertices.Count + originalVertices.Length-1 });
				}
			}

			uint[] _movability = new uint[Mathf.CeilToInt(movability.Length / 32f)];
			for (int i = 0; i < movability.Length; i++)
			{
				HairMovability.SetMovable(i, movability[i], _movability);
			}

			asset.InitializeVertices(originalVertices.Concat(newVertices).ToArray());
			asset.InitializeMovability(_movability);
			asset.InitializeStrands(originalStrands.Concat(newStrands).ToArray());
		}
	}
}
