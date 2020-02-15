using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace VHair
{
    /// <summary>
    /// Simple and inefficient implementation of a vhair renderer.
    /// Mainly used for debug purposes. Should not be used in production!
    /// </summary>
    public class VHairDebugRenderer : HairRenderer
    {
        /// <summary>
        /// The mesh this renderer is using for rendering.
        /// </summary>
        private Mesh mesh;

        /// <summary>
        /// The material used for rendering.
        /// </summary>
        [SerializeField]
        private Material material;
        
        private NativeArray<float3> _vertices;
        private HairStrand[] _strands;
        private List<int> _indices = new List<int>();

        public void Start()
        {
            this._vertices = new NativeArray<float3>(this.instance.asset.VertexCount, Allocator.Persistent);
            this._strands = new HairStrand[this.instance.asset.StrandCount];
            this.mesh = new Mesh();
            this.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.MarkDynamic();

            this.UpdateMeshInitial();
        }

        private void UpdateMeshInitial()
        {
            this._indices.Clear();
            this.instance.strands.CpuReference.CopyTo(this._strands);

            int sLen = _strands.Length;
            for (int i = 0; i < sLen; i++)
            {
                HairStrand strand = this._strands[i];
                for (int j = strand.firstVertex; j < strand.lastVertex; j++)
                {
                    this._indices.Add(j);
                    this._indices.Add(j + 1);
                }
            }

            UpdateMesh();
            mesh.SetIndices(this._indices.ToArray(), MeshTopology.Lines, 0);
        }

        private void UpdateMesh()
        {
            // Get data copy
            this.instance.vertices.CpuReference.CopyTo(this._vertices);

            // Set mesh data
            mesh.SetVertices<float3>(_vertices);
            mesh.RecalculateBounds();
            mesh.UploadMeshData(false);
        }

        public void LateUpdate()
        {
            this.UpdateMesh();
            Graphics.DrawMesh(this.mesh, Matrix4x4.identity, this.material, this.gameObject.layer);
        }

		protected void OnDestroy()
		{
			if (this._vertices.IsCreated)
				this._vertices.Dispose();
		}
	}
}