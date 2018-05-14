using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

namespace VHair
{
    /// <summary>
    /// Hair instance implementation.
    /// </summary>
    public class HairInstance : MonoBehaviour
    {
        public HairAsset asset;

        #region Data Buffers

        /// <summary>
        /// The vertex buffer of this instance.
        /// This maps essentially to a direct copy of <see cref="HairAsset.vertices"/>
        /// </summary>
        private ComputeBuffer vertices;

        /// <summary>
        /// The strands buffer of this instance.
        /// This maps essentially to a direct copy of <see cref="HairAsset.strands"/>
        /// </summary>
        private ComputeBuffer strands;

        /// <summary>
        /// The copy of <see cref="HairAsset.vertices"/>
        /// </summary>
        private Vector3[] _vertices;

        /// <summary>
        /// The copy of <see cref="HairAsset.strands"/>
        /// </summary>
        private HairStrand[] _strands;

        private bool cpuDataVerticesModified = false;
        private bool cpuDataStrandsModified = false;
        private bool gpuDataVerticesModified = false;
        private bool gpuDataStrandsModified = false;
        #endregion

        #region Data access

        public int vertexCount { get { return this._vertices.Length; } }
        public int strandCount { get { return this._strands.Length; } }

        private void SyncCpuVertices()
        {
            if (this.gpuDataVerticesModified)
            {
                // Data was modified on the gpu, we need to read it back
                this.vertices.GetData(this._vertices);
                this.gpuDataVerticesModified = false;
            }
        }

        private void SyncCpuStrands()
        {
            if (this.gpuDataStrandsModified)
            {
                // Data was modified on the gpu, we need to read it back
                this.strands.GetData(this._strands);
                this.gpuDataStrandsModified = false;
            }
        }

        private void SyncGpuVertices()
        {
            if (this.cpuDataVerticesModified)
            {
                this.vertices.SetData(this._vertices);
                this.cpuDataVerticesModified = false;
            }
        }

        private void SyncGpuStrands()
        {
            if (this.cpuDataStrandsModified)
            {
                this.strands.SetData(this._strands);
                this.cpuDataStrandsModified = false;
            }
        }

        /// <summary>
        /// Returns the internal array with the vertex data of the hair.
        /// This array is supposed to be the cpu-side of the hair data, you can write to it.
        /// IMPORTANT: If you modify data, you have to tell the hair instance by calling <see cref="SetVerticesModified(bool)"/>
        /// 
        /// Hair instance vertex data is stored in worldspace!
        /// </summary>
        public Vector3[] GetVertexArray()
        {
            // Sync
            SyncCpuVertices();
            return this._vertices;
        }

        /// <summary>
        /// Returns the internal array with the strand data of the hair.
        /// This array is supposed to be the cpu-side of the hair data, you can write to it.
        /// IMPORTANT: If you modify data, you have to tell the hair instance by calling <see cref="SetVerticesModified(bool)"/>
        /// </summary>
        public HairStrand[] GetStrandsArray()
        {
            // Sync
            SyncCpuStrands();
            return this._strands;
        }

        /// <summary>
        /// Writes the vertex data of the hair into the specified array.
        /// Hair instance vertex data is stored in worldspace!
        /// </summary>
        public void ReadVertexData(Vector3[] data)
        {
            // Sync
            SyncCpuVertices();
            System.Array.Copy(this._vertices, data, Mathf.Min(data.Length, this._vertices.Length));
        }

        /// <summary>
        /// Writes the strand data of the hair into the specified array.
        /// </summary>
        public void ReadStrandData(HairStrand[] strands)
        {
            // Sync
            SyncCpuStrands();
            System.Array.Copy(this._strands, strands, Mathf.Min(strands.Length, this._strands.Length));
        }

        /// <summary>
        /// Returns a compute buffer with the vertex data of the hair.
        /// This array is supposed to be the gpu-side of the hair data, you can write to it.
        /// IMPORTANT: If you modify data, you have to tell the hair instance by calling <see cref="SetVerticesModified(bool)"/>
        /// Hair instance vertex data is stored in worldspace!
        /// </summary>
        public ComputeBuffer GetVertexComputeBuffer()
        {
            // Sync
            SyncGpuVertices();
            return this.vertices;
        }

        /// <summary>
        /// Returns a compute buffer with the strand data of the hair.
        /// This array is supposed to be the gpu-side of the hair data, you can write to it.
        /// IMPORTANT: If you modify data, you have to tell the hair instance by calling <see cref="SetStrandsModified(bool)"/>
        /// </summary>
        public ComputeBuffer GetStrandsComputeBuffer()
        {
            // Sync
            SyncGpuStrands();
            return this.strands;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cpuOrGpu">Where the data was modified, True = cpu, false = gpu</param>
        public void SetVerticesModified(bool cpuOrGpu)
        {
            if (cpuOrGpu)
                this.cpuDataVerticesModified = true;
            else
                this.gpuDataVerticesModified = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cpuOrGpu">Where the data was modified, True = cpu, false = gpu</param>
        public void SetStrandsModified(bool cpuOrGpu)
        {
            if (cpuOrGpu)
                this.cpuDataStrandsModified = true;
            else
                this.gpuDataStrandsModified = true;
        }

        #endregion

        /// <summary>
        /// Initializes the hair instance.
        /// </summary>
        public void Awake()
        {
            // Get hair data copy
            this.asset.GetRawDataCopy(out this._vertices, out this._strands);

            // Create compute buffers
            this.vertices = new ComputeBuffer(this._vertices.Length, 12); // 12 bytes stride, Vector3 (3x 4 byte float)
            this.strands = new ComputeBuffer(this._strands.Length, 8); // 8 bytes stride, HairStrand (2x 4 byte signed int)
            this.cpuDataStrandsModified = this.cpuDataVerticesModified = true;
        }

        /// <summary>
        /// Releases GPU resources
        /// </summary>
        public void OnDestroy()
        {
            this.vertices.Dispose();
            this.strands.Dispose();
        }
    }
}