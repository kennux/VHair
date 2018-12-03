using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Collections;

namespace UnityTK
{
    /// <summary>
    /// Internal data structure for a static visualization cache entry in <see cref="StaticBatching"/>.
    /// </summary>
    internal struct StaticVisualRepresentationCache : IDisposable
    {
        /// <summary>
        /// Vertex positions of the mesh
        /// </summary>
        public NativeArray<Vector3> vertices;

        /// <summary>
        /// The normals of the mesh
        /// </summary>
        public NativeArray<Vector3> normals;

        /// <summary>
        /// The tangents of the mesh
        /// </summary>
        public NativeArray<Vector4> tangents;

        /// <summary>
        /// Uvs of the mesh
        /// </summary>
        public NativeArray<Vector2> uv0;

        /// <summary>
        /// Indices of the mesh
        /// </summary>
        public NativeArray<int> indices;

        public StaticVisualRepresentationCache(Mesh mesh)
        {
            this.vertices = new NativeArray<Vector3>(mesh.vertices, Allocator.Persistent);
            this.normals = new NativeArray<Vector3>(mesh.normals, Allocator.Persistent);
            this.tangents = new NativeArray<Vector4>(mesh.tangents, Allocator.Persistent);
            this.uv0 = new NativeArray<Vector2>(mesh.uv, Allocator.Persistent);
            this.indices = new NativeArray<int>(mesh.triangles, Allocator.Persistent);
        }

        public void Dispose()
        {
            this.vertices.Dispose();
            this.normals.Dispose();
            this.tangents.Dispose();
            this.uv0.Dispose();
            this.indices.Dispose();
        }
    }
}
