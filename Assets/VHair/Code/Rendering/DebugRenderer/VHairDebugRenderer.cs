using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

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
        
        private Vector3[] _vertices;
        private HairStrand[] _strands;
        private List<int> _indices = new List<int>();

        public void Start()
        {
            this._vertices = new Vector3[this.instance.asset.vertexCount];
            this._strands = new HairStrand[this.instance.asset.strandCount];
            this.mesh = new Mesh();
            this.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.MarkDynamic();

            this.UpdateMeshInitial();
        }

        private void UpdateMeshInitial()
        {
            this._indices.Clear();
            this.instance.strands.cpuReference.CopyTo(this._strands, 0);

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
            this.instance.vertices.cpuReference.CopyTo(this._vertices, 0);

            // Set mesh data
            mesh.vertices = this._vertices;
            mesh.RecalculateBounds();
            mesh.UploadMeshData(false);
        }

        public void LateUpdate()
        {
            this.UpdateMesh();
            Graphics.DrawMesh(this.mesh, Matrix4x4.identity, this.material, this.gameObject.layer);
        }
    }
}