using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;

namespace UnityTK.Test.Utility
{
    public class StaticBatchingTest
    {
        private static Mesh GenerateMesh(Vector3[] vertices, Vector3[] normals, Vector2[] uvs, int[] indices, MeshTopology topo = MeshTopology.Triangles)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            mesh.RecalculateTangents();

            return mesh;
        }

        private static StaticBatching CreateSB()
        {
            GameObject go = new GameObject("StaticBatching");
            return go.AddComponent<StaticBatching>();
        }

        /// <summary>
        /// Tests simply batching 2 meshes with same layer, material and worldspace chunk.
        /// </summary>
        [Test]
        public void TestSimpleBatching()
        {
            // Arrange
            var sb = CreateSB();
            StaticBatching.instance = sb;

            var m1 = GenerateMesh(new Vector3[] { Vector3.one, Vector3.one * 2f, Vector3.one * 3f }, new Vector3[] { Vector3.up, Vector3.up, Vector3.up }, new Vector2[] { Vector2.up, Vector2.up, Vector2.up }, new int[] { 0, 1, 2 }, MeshTopology.Triangles);
            var m2 = GenerateMesh(new Vector3[] { Vector3.one * 4f, Vector3.one * 5f, Vector3.one * 6f }, new Vector3[] { Vector3.down, Vector3.down, Vector3.down }, new Vector2[] { Vector2.down, Vector2.down, Vector2.down }, new int[] { 0, 1, 2 }, MeshTopology.Triangles);
            Material mat = new Material(Shader.Find("Standard"));
            object owner = new object();

            // Act
            sb.InsertMesh(m1, mat, 8, Matrix4x4.identity, owner);
            sb.InsertMesh(m2, mat, 8, Matrix4x4.TRS(Vector3.one, Quaternion.identity, Vector3.one), owner);
            sb.Update();
            
            // Assert
            int[] expectedIndices = new int[] { 0, 1, 2, 3, 4, 5 };
            Vector3[] expectedNormals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.down, Vector3.down, Vector3.down };
            Vector3[] expectedVertices = new Vector3[] { Vector3.one, Vector3.one * 2f, Vector3.one * 3f, Vector3.one * 5f, Vector3.one * 6f, Vector3.one * 7f };
            Vector2[] expectedUvs = new Vector2[] { Vector2.up, Vector2.up, Vector2.up, Vector2.down, Vector2.down, Vector2.down };

            var meshes = sb.GetBatchedMeshes(mat, 8);

            Assert.AreEqual(1, meshes.Count);
            CollectionAssert.AreEqual(expectedIndices, meshes[0].GetIndices(0));
            CollectionAssert.AreEqual(expectedVertices, meshes[0].vertices);
            CollectionAssert.AreEqual(expectedNormals, meshes[0].normals);
            CollectionAssert.AreEqual(expectedUvs, meshes[0].uv);

            sb.OnDestroy();
        }

        /// <summary>
        /// Test batching 2 meshes together with different materials.
        /// </summary>
        [Test]
        public void TestMultiMaterialBatching()
        {
            // Arrange
            var sb = CreateSB();
            StaticBatching.instance = sb;

            var m1 = GenerateMesh(new Vector3[] { Vector3.one, Vector3.one * 2f, Vector3.one * 3f }, new Vector3[] { Vector3.up, Vector3.up, Vector3.up }, new Vector2[] { Vector2.up, Vector2.up, Vector2.up }, new int[] { 0, 1, 2 }, MeshTopology.Triangles);
            var m2 = GenerateMesh(new Vector3[] { Vector3.one * 4f, Vector3.one * 5f, Vector3.one * 6f }, new Vector3[] { Vector3.down, Vector3.down, Vector3.down }, new Vector2[] { Vector2.down, Vector2.down, Vector2.down }, new int[] { 0, 1, 2 }, MeshTopology.Triangles);
            Material mat = new Material(Shader.Find("Standard"));
            Material mat2 = new Material(Shader.Find("Standard"));
            object owner = new object();

            // Act
            sb.InsertMesh(m1, mat, 8, Matrix4x4.identity, owner);
            sb.InsertMesh(m2, mat2, 8, Matrix4x4.TRS(Vector3.one, Quaternion.identity, Vector3.one), owner);
            sb.Update();

            // Assert
            int[] expectedIndices = new int[] { 0, 1, 2 };
            Vector3[] expectedNormals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up };
            Vector3[] expectedVertices = new Vector3[] { Vector3.one, Vector3.one * 2f, Vector3.one * 3f };
            Vector2[] expectedUvs = new Vector2[] { Vector2.up, Vector2.up, Vector2.up };

            int[] expectedIndices2 = new int[] { 0, 1, 2 };
            Vector3[] expectedNormals2 = new Vector3[] { Vector3.down, Vector3.down, Vector3.down };
            Vector3[] expectedVertices2 = new Vector3[] { Vector3.one * 5f, Vector3.one * 6f, Vector3.one * 7f };
            Vector2[] expectedUvs2 = new Vector2[] { Vector2.down, Vector2.down, Vector2.down };

            var meshes = sb.GetBatchedMeshes(mat, 8);
            var meshes2 = sb.GetBatchedMeshes(mat2, 8);

            Assert.AreEqual(1, meshes.Count);
            Assert.AreEqual(1, meshes2.Count);
            CollectionAssert.AreEqual(expectedIndices, meshes[0].GetIndices(0));
            CollectionAssert.AreEqual(expectedVertices, meshes[0].vertices);
            CollectionAssert.AreEqual(expectedNormals, meshes[0].normals);
            CollectionAssert.AreEqual(expectedUvs, meshes[0].uv);

            CollectionAssert.AreEqual(expectedIndices2, meshes2[0].GetIndices(0));
            CollectionAssert.AreEqual(expectedVertices2, meshes2[0].vertices);
            CollectionAssert.AreEqual(expectedNormals2, meshes2[0].normals);
            CollectionAssert.AreEqual(expectedUvs2, meshes2[0].uv);

            sb.OnDestroy();
        }

        /// <summary>
        /// Test batching 2 meshes together with different worldspace chunks.
        /// </summary>
        [Test]
        public void TestChunkedBatching()
        {
            // Arrange
            var sb = CreateSB();
            sb.chunkSize = 16;
            StaticBatching.instance = sb;

            var m1 = GenerateMesh(new Vector3[] { Vector3.one, Vector3.one * 2f, Vector3.one * 3f }, new Vector3[] { Vector3.up, Vector3.up, Vector3.up }, new Vector2[] { Vector2.up, Vector2.up, Vector2.up }, new int[] { 0, 1, 2 }, MeshTopology.Triangles);
            var m2 = GenerateMesh(new Vector3[] { Vector3.one * 4f, Vector3.one * 5f, Vector3.one * 6f }, new Vector3[] { Vector3.down, Vector3.down, Vector3.down }, new Vector2[] { Vector2.down, Vector2.down, Vector2.down }, new int[] { 0, 1, 2 }, MeshTopology.Triangles);
            Material mat = new Material(Shader.Find("Standard"));
            object owner = new object();

            // Act
            sb.InsertMesh(m1, mat, 8, Matrix4x4.identity, owner);
            sb.InsertMesh(m2, mat, 8, Matrix4x4.TRS(Vector3.one * 32f, Quaternion.identity, Vector3.one), owner);
            sb.Update();

            // Assert
            int[] expectedIndices = new int[] { 0, 1, 2 };
            Vector3[] expectedNormals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up };
            Vector3[] expectedVertices = new Vector3[] { Vector3.one, Vector3.one * 2f, Vector3.one * 3f };
            Vector2[] expectedUvs = new Vector2[] { Vector2.up, Vector2.up, Vector2.up };

            int[] expectedIndices2 = new int[] { 0, 1, 2 };
            Vector3[] expectedNormals2 = new Vector3[] { Vector3.down, Vector3.down, Vector3.down };
            Vector3[] expectedVertices2 = new Vector3[] { Vector3.one * 36f, Vector3.one * 37f, Vector3.one * 38f };
            Vector2[] expectedUvs2 = new Vector2[] { Vector2.down, Vector2.down, Vector2.down };

            var meshes = sb.GetBatchedMeshes(mat, 8);

            Assert.AreEqual(2, meshes.Count);
            CollectionAssert.AreEqual(expectedIndices, meshes[0].GetIndices(0));
            CollectionAssert.AreEqual(expectedVertices, meshes[0].vertices);
            CollectionAssert.AreEqual(expectedNormals, meshes[0].normals);
            CollectionAssert.AreEqual(expectedUvs, meshes[0].uv);

            CollectionAssert.AreEqual(expectedIndices2, meshes[1].GetIndices(0));
            CollectionAssert.AreEqual(expectedVertices2, meshes[1].vertices);
            CollectionAssert.AreEqual(expectedNormals2, meshes[1].normals);
            CollectionAssert.AreEqual(expectedUvs2, meshes[1].uv);

            sb.OnDestroy();
        }

        /*
        NOT IMPLEMENTED YET!
        TODO

        /// <summary>
        /// Test batching 2 meshes together with different gameobject layers.
        /// </summary>
        [Test]
        public void TestMultiLayerBatching()
        {

        }
        */
    }
}