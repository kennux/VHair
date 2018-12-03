using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Collections;
using System.Threading.Tasks;
using Unity.Jobs;

namespace UnityTK
{
    /// <summary>
    /// The job data for rebuilding jobs.
    /// <see cref="runningJobData"/>
    /// </summary>
    internal struct StaticBatchingRebuildJobData
    {
        /// <summary>
        /// The group key of the static batch this job is related to.
        /// </summary>
        public StaticBatchingGroupKey groupKey;

        /// <summary>
        /// The handle that must be completed.
        /// </summary>
        public JobHandle jobHandle;

        /// <summary>
        /// The mesh instances to be built.
        /// </summary>
        public List<StaticMeshInstance> meshInstances;

        /// <summary>
        /// The vertex offsets for <see cref="meshInstances"/>
        /// </summary>
        public List<int> vertexOffsets;

        /// <summary>
        /// The index offsets for <see cref="meshInstances"/>
        /// </summary>
        public List<int> indexOffsets;

        /// <summary>
        /// Vertices of the built mesh.
        /// Job data array
        /// </summary>
        public NativeArray<Vector3> nVertices;

        /// <summary>
        /// Normals of the built mesh.
        /// Job data array
        /// </summary>
        public NativeArray<Vector3> nNormals;

        /// <summary>
        /// Tangents of the built mesh.
        /// Job data array
        /// </summary>
        public NativeArray<Vector4> nTangents;

        /// <summary>
        /// Uvs of the built mesh.
        /// Job data array
        /// </summary>
        public NativeArray<Vector2> nUvs;

        /// <summary>
        /// The indices of the built mesh.
        /// Job data array
        /// </summary>
        public NativeArray<int> nIndices;

        /// <summary>
        /// Vertices of the built mesh.
        /// Final target list
        /// </summary>
        public List<Vector3> vertices;

        /// <summary>
        /// Normals of the built mesh.
        /// Final target list
        /// </summary>
        public List<Vector3> normals;

        /// <summary>
        /// Tangents of the built mesh.
        /// Final target list
        /// </summary>
        public List<Vector4> tangents;

        /// <summary>
        /// Uvs of the built mesh.
        /// Final target list
        /// </summary>
        public List<Vector2> uvs;

        /// <summary>
        /// The indices of the built mesh.
        /// Final target list
        /// </summary>
        public List<int> indices;

        /// <summary>
        /// The internal job index.
        /// </summary>
        public uint jobId;

        public void Flush(Mesh m)
        {
            m.SetVertices(this.vertices);
            m.SetNormals(this.normals);
            m.SetTangents(this.tangents);
            m.SetUVs(0, this.uvs);
            m.SetTriangles(this.indices, 0);
            Deallocate();
        }

        public void Deallocate()
        {
            ListPool<Vector3>.Return(this.vertices);
            ListPool<Vector3>.Return(this.normals);
            ListPool<Vector4>.Return(this.tangents);
            ListPool<Vector2>.Return(this.uvs);
            ListPool<int>.Return(this.indices);
            ListPool<int>.Return(this.vertexOffsets);
            ListPool<int>.Return(this.indexOffsets);
            ListPool<StaticMeshInstance>.Return(this.meshInstances);
            this.nIndices.Dispose();
            this.nVertices.Dispose();
            this.nNormals.Dispose();
            this.nTangents.Dispose();
            this.nUvs.Dispose();
        }

        public void PreAllocate()
        {
            this.vertices = ListPool<Vector3>.Get();
            this.normals = ListPool<Vector3>.Get();
            this.tangents = ListPool<Vector4>.Get();
            this.uvs = ListPool<Vector2>.Get();
            this.indices = ListPool<int>.Get();
            this.vertexOffsets = ListPool<int>.Get();
            this.indexOffsets = ListPool<int>.Get();
            this.meshInstances = ListPool<StaticMeshInstance>.Get();
        }

        public void Allocate(int vertexCount, int indexCount)
        {
            // Allocate temp arrays
            this.nVertices = new NativeArray<Vector3>(vertexCount, Allocator.TempJob);
            this.nNormals = new NativeArray<Vector3>(vertexCount, Allocator.TempJob);
            this.nTangents = new NativeArray<Vector4>(vertexCount, Allocator.TempJob);
            this.nUvs = new NativeArray<Vector2>(vertexCount, Allocator.TempJob);
            this.nIndices = new NativeArray<int>(indexCount, Allocator.TempJob);
        }
    }
}
