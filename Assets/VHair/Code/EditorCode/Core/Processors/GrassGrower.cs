using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using System.Collections;
using Unity.Mathematics;

namespace VHair.Editor
{
	public class GrassGrower : IHairAssetProcessor
	{
		public string Name => "Grass grower";

		public string Description => "Grows grass procedurally on an object.";

		private GameObject targetObject;
		private Vector3 growingDirection = Vector3.up;
		private float growingDirMinAngle = .75f;
		private float stalksPerSquareUnit = 500;
		private int verticesPerStalk = 4;
		private float heightMin = 0.15f;
		private float heightMax = 0.25f;

		public void OnGUI()
		{
			targetObject = (GameObject)EditorGUILayout.ObjectField("Grow on", targetObject, typeof(GameObject), true);
			growingDirection = EditorGUILayout.Vector3Field("Growing direction", growingDirection);
			growingDirMinAngle = EditorGUILayout.FloatField("Growing dir min angle", growingDirMinAngle);
			stalksPerSquareUnit = EditorGUILayout.FloatField("Stalks per square unit", stalksPerSquareUnit);
			verticesPerStalk = EditorGUILayout.IntField("Vertices per stalk", verticesPerStalk);
			heightMin = EditorGUILayout.FloatField("Height min", heightMin);
			heightMax = EditorGUILayout.FloatField("Height max", heightMax);
		}

		public void Run(HairAsset asset)
		{
			List<bool> movability = new List<bool>();
			List<float3> vertices = new List<float3>();
			List<HairStrand> strands = new List<HairStrand>();

			foreach (var mf in targetObject.GetComponentsInChildren<MeshFilter>())
			{
				var t = mf.transform;
				var mesh = mf.sharedMesh;

				if (mesh == null)
					continue;

				var meshTriangles = mesh.triangles;
				var meshVertices = mesh.vertices;
				var meshNormals = mesh.normals;
				for (int i = 0; i < meshTriangles.Length; i+=3)
				{
					int i1 = meshTriangles[i], i2 = meshTriangles[i + 1], i3 = meshTriangles[i + 2];
					Vector3 p1 = meshVertices[i1], p2 = meshVertices[i2], p3 = meshVertices[i3];
					Vector3 n1 = t.TransformDirection(meshNormals[i1]), n2 = t.TransformDirection(meshNormals[i2]), n3 = t.TransformDirection(meshNormals[i3]);

					var faceNormal = (n1 + n2 + n3) / 3f;

					if (Vector3.Dot(faceNormal, growingDirection) >= growingDirMinAngle)
					{
						float triangleSurfaceArea = Vector3.Cross(p2 - p1, p3 - p1).magnitude;
						int stalks = Mathf.FloorToInt(triangleSurfaceArea * stalksPerSquareUnit);

						for (int k = 0; k < stalks; k++)
						{
							float r1 = UnityEngine.Random.value, r2 = UnityEngine.Random.value;
							var p = (1f - Mathf.Sqrt(r1)) * p1 + (Mathf.Sqrt(r1) * (1f - r2)) * p2 + (r2 * Mathf.Sqrt(r1)) * p3;

							int startIdx = vertices.Count;
							movability.Add(false);
							vertices.Add(p);

							float height = UnityEngine.Random.Range(heightMin, heightMax);
							float heightPerVertex = height / (verticesPerStalk - 1);
							for (int j = 1; j < verticesPerStalk; j++)
							{
								movability.Add(true);
								vertices.Add(p + (faceNormal * (heightPerVertex * (float)j)));
							}

							strands.Add(new HairStrand()
							{
								firstVertex = startIdx,
								lastVertex = vertices.Count - 1
							});
						}
					}
				}
			}

			asset.InitializeStrands(strands.ToArray());
			asset.InitializeVertices(vertices.ToArray());

			uint[] u_movability = new uint[Mathf.CeilToInt(movability.Count / 32f)];
			for(int i = 0; i < movability.Count; i++)
			{
				HairMovability.SetMovable(i, movability[i], u_movability);
			}
			asset.InitializeMovability(u_movability);
		}
	}
}
