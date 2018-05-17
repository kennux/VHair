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

        #region Data

        public CPUGPUData<Vector3> vertices
        {
            get { return this._vertices; }
        }
        private CPUGPUData<Vector3> _vertices;

        public CPUGPUData<HairStrand> strands
        {
            get { return this._strands; }
        }
        private CPUGPUData<HairStrand> _strands;

        public CPUGPUData<uint> movability
        {
            get { return this._movability; }
        }
        private CPUGPUData<uint> _movability;

        public int vertexCount { get { return this._vertices.count; } }
        public int strandCount { get { return this._strands.count; } }

        #endregion
        

        /// <summary>
        /// Initializes the hair instance.
        /// </summary>
        public void Awake()
        {
            // Get hair data copy
            Vector3[] vertices;
            HairStrand[] strands;
            uint[] movability;
            this.asset.GetRawDataCopy(out vertices, out strands, out movability);

            // Create compute buffers
            this._vertices = new CPUGPUData<Vector3>(vertices, 12); // 12 bytes stride, Vector3 (3x 4 byte float)
            this._strands = new CPUGPUData<HairStrand>(strands, 8); // 8 bytes stride, HairStrand (2x 4 byte signed int)
            this._movability = new CPUGPUData<uint>(movability, 4); // 4 bytes stride, 32 bits (unsigned integer)
        }

        /// <summary>
        /// Releases GPU resources
        /// </summary>
        public void OnDestroy()
        {
            this._vertices.Dispose();
            this._strands.Dispose();
            this._movability.Dispose();
        }
    }
}