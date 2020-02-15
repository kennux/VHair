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
	/// <summary>
	/// Quick and dirty wavefront plaintext importer.
	/// TODO: Improve
	/// </summary>
	public class WavefrontImporter : IHairAssetImporter
	{
		public string displayName
		{
			get
			{
				return "Wavefront Plaintext (Blender)";
			}
		}

		public string filePath;

		public void Import(out float3[] vertices, out HairStrand[] strands)
		{
			// Prepare temporary lists
			List<float3> _vertices = new List<float3>();
			List<HairStrand> _strands = new List<HairStrand>();
			HashSet<int> _currentStrandIndices = new HashSet<int>();

			// Read OBJ file
			string[] lines = File.ReadAllLines(this.filePath);
			int prevI1 = -1, prevI2 = -1;
			Vector3 vertex = new Vector3();
			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i];
				if (line[0] == 'v' && line[1] == ' ')
				{
					// Split vertex
					string[] coordinates = line.Substring(2).Split(' ');

					// Parse vertex
					vertex.x = float.Parse(coordinates[0], System.Globalization.CultureInfo.InvariantCulture);
					vertex.y = float.Parse(coordinates[1], System.Globalization.CultureInfo.InvariantCulture);
					vertex.z = float.Parse(coordinates[2], System.Globalization.CultureInfo.InvariantCulture);

					// Write vertex
					_vertices.Add(vertex);
				}
				else if (line[0] == 'l' && line[1] == ' ')
				{
					// Split indices
					string[] indicesStr = line.Substring(2).Split(' ');
					int i1 = int.Parse(indicesStr[0])-1;
					int i2 = int.Parse(indicesStr[1])-1;

					// New strand?
					if (i1 != prevI2 && _currentStrandIndices.Count > 0)
					{
						// New strand!
						FlushStrand(_strands, _currentStrandIndices);
					}

					// Add indices to current strand
					_currentStrandIndices.Add(i1);
					_currentStrandIndices.Add(i2);

					// Set prev values
					prevI1 = i1;
					prevI2 = i2;
				}
			}

			if (_currentStrandIndices.Count > 0)
				FlushStrand(_strands, _currentStrandIndices); // Flush last!

			// Write back data
			vertices = _vertices.ToArray();
			strands = _strands.ToArray();

			// Run garbage collection to collect the large objects (and probably thousands of strings :3) we just created
			GC.Collect();
		}

		private void FlushStrand(List<HairStrand> strands, HashSet<int> indices)
		{
			// TODO: Reformatting if indices are fragmented, for now this is okay since we know blender never fragments them :>
			int min = int.MaxValue, max = int.MinValue;
			foreach (var index in indices)
			{
				if (min > index)
					min = index;
				if (max < index)
					max = index;
			}

			// Create strand
			strands.Add(new HairStrand()
			{
				firstVertex = min,
				lastVertex = max
			});

			// Start over
			indices.Clear();
		}
		

		public bool OnInspectorGUI(HairAsset hairAsset)
		{
			this.filePath = EditorGUILayout.TextField("File path", this.filePath);
			if (GUILayout.Button("Open File"))
			{
				this.filePath = EditorUtility.OpenFilePanel("Open file", "", "obj");
			}
			
			return !string.IsNullOrEmpty(this.filePath) && File.Exists(this.filePath);
		}
	}
}
