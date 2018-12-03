using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections.Generic;

namespace UnityTK.Test.Utility
{
    public class MeshBuilderTest
    {
		[Test]
		public void TestChannelEnumerator()
		{
			MeshBuilderChannels channels = MeshBuilderChannels.NORMALS | MeshBuilderChannels.TANGENTS | MeshBuilderChannels.COLORS;
			MeshBuilderChannel[] channelsArr = new MeshBuilderChannel[] { MeshBuilderChannel.NORMALS, MeshBuilderChannel.TANGENTS, MeshBuilderChannel.COLORS };

			CollectionAssert.AreEquivalent(channelsArr, channels.AsEnumerable());

			channels = MeshBuilderChannels.NORMALS | MeshBuilderChannels.TANGENTS | MeshBuilderChannels.UV | MeshBuilderChannels.UV8;
			channelsArr = new MeshBuilderChannel[] { MeshBuilderChannel.NORMALS, MeshBuilderChannel.TANGENTS, MeshBuilderChannel.UV, MeshBuilderChannel.UV8 };

			CollectionAssert.AreEquivalent(channelsArr, channels.AsEnumerable());
        }

		[Test]
		public void TestBuilding()
		{
			MeshBuilder builder = new MeshBuilder();
			Mesh m = new Mesh();

			int[] indices = new int[] { 0, 1, 2 };
			Vector3[] positions = new Vector3[] { Vector3.zero, Vector3.one, Vector3.one * 2f };
			Color[] colors = new Color[] { Color.red, Color.green, Color.yellow };
			Vector3[] normals = new Vector3[] { Vector3.down, Vector3.up, Vector3.left };
			Vector4[] tangents = new Vector4[] { Vector3.down, Vector3.up, Vector3.left };
			Vector4[] uvs = new Vector4[] { new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(1, .5f) };

			builder.Start(MeshBuilderChannels.COLORS | MeshBuilderChannels.NORMALS | MeshBuilderChannels.TANGENTS | MeshBuilderChannels.ALLUV);
			for (int i = 0; i < positions.Length; i++)
			{
				MeshBuilder.VertexData vertexData = new MeshBuilder.VertexData()
				{
					position = positions[i],
					color = colors[i],
					normal = normals[i],
					tangent = tangents[i],
					uv = uvs[i],
					uv2 = uvs[i],
					uv3 = uvs[i],
					uv4 = uvs[i],
					uv5 = uvs[i],
					uv6 = uvs[i],
					uv7 = uvs[i],
					uv8 = uvs[i]
				};
				builder.AddVertex(ref vertexData);
			}

			// Flush
			builder.WriteMeshData(m);
			builder.Stop();

			// Check mesh data
			CollectionAssert.AreEqual(indices, m.GetIndices(0));
			CollectionAssert.AreEqual(positions, m.vertices);
			CollectionAssert.AreEqual(colors, m.colors);
			CollectionAssert.AreEqual(normals, m.normals);
			CollectionAssert.AreEqual(tangents, m.tangents);

			List<Vector4> meshUvs = new List<Vector4>();
			for (int i = 0; i < 8; i++)
			{
				m.GetUVs(i, meshUvs);
				CollectionAssert.AreEqual(uvs, meshUvs);
				meshUvs.Clear();
			}

			// Test exception when already flushed
			try
			{
				MeshBuilder.VertexData vertexData = new MeshBuilder.VertexData()
				{
					position = positions[0],
					color = colors[0],
					normal = normals[0],
					tangent = tangents[0],
					uv4 = uvs[0]
				};
				builder.AddVertex(ref vertexData);
				Assert.IsTrue(false);
			}
			catch (System.InvalidOperationException ex)
			{

			}

			// Test invalid vertex data
			builder.Start(MeshBuilderChannels.COLORS);
			try
			{
				var vd = new MeshBuilder.VertexData();
				builder.AddVertex(ref vd);
				Assert.IsTrue(false);
			}
			catch (System.InvalidOperationException ex)
			{

			}
			builder.Stop();
        }
    }
}