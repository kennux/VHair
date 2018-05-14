using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

namespace VHair
{
    /// <summary>
    /// The data saved in the asset should _NEVER_ be changed at runtime, but instead be used as a source from where to create a copy (instance).
    /// </summary>
    [CreateAssetMenu(fileName = "HairAsset", menuName = "VHair/Hair Asset")]
    public class HairAsset : ScriptableObject
    {
        /// <summary>
        /// The vertices used for the hair.
        /// </summary>
        [SerializeField]
        private Vector3[] vertices;

        /// <summary>
        /// The strands of this hair asset.
        /// </summary>
        [SerializeField]
        private HairStrand[] strands;

        public HairMovability movability
        {
            get { return this.movability; }
        }

        /// <summary>
        /// The movability mask
        /// </summary>
        [SerializeField]
        private HairMovability _movability;

        /// <summary>
        /// Whether or not this asset already has data imported.
        /// </summary>
        public bool wasImported = false;

        public int strandCount
        {
            get { return this.strands.Length; }
        }

        public int vertexCount
        {
            get { return this.vertices.Length; }
        }

        public void GetVertexData(out Vector3[] vertices)
        {
            vertices = new Vector3[this.vertices.Length];
            System.Array.Copy(this.vertices, vertices, vertices.Length);
        }

        public void GetStrandData(out HairStrand[] strands)
        {
            strands = new HairStrand[this.strands.Length];
            System.Array.Copy(this.strands, strands, strands.Length);
        }

        public void GetRawDataCopy(out Vector3[] vertices, out HairStrand[] strands)
        {
            GetVertexData(out vertices);
            GetStrandData(out strands);
        }

        /// <summary>
        /// <see cref="GetRawDataCopy(out Vector3[], out HairStrand[])"/> for unity native arrays.
        /// </summary>
        public void GetRawDataCopyNA(out NativeArray<Vector3> vertices, out NativeArray<HairStrand> strands, Allocator allocator = Allocator.Persistent)
        {
            vertices = new NativeArray<Vector3>(this.vertices, allocator);
            strands = new NativeArray<HairStrand>(this.strands, allocator);
        }

        public void GetStrandInformation(int index, out int vertexCount, ref Vector3[] vertices)
        {
            // Range check
            if (index < 0 || index >= this.strands.Length)
                throw new System.ArgumentOutOfRangeException("index");

            // Read vertex data
            int start = this.strands[index].firstVertex;
            int end = this.strands[index].lastVertex;
            vertexCount = (end - start)+1;

            // Make sure preAlloc is set
            if (ReferenceEquals(vertices, null) || vertices.Length < vertexCount)
                vertices = new Vector3[vertexCount];

            // Write vertex data
            /*
            Unoptimized:
            int j = 0;
            for (int i = start; i <= end; i++)
            {
                vertices[j] = this.vertices[i];
                j++;
            }
            */
            // Direct memory copy
            System.Array.Copy(this.vertices, start, vertices, 0, vertexCount);
        }

        public void SetData(Vector3[] vertices, HairStrand[] strands)
        {
            this.vertices = new Vector3[vertices.Length];
            this.strands = new HairStrand[strands.Length];

            vertices.CopyTo(this.vertices, 0);
            strands.CopyTo(this.strands, 0);
        }
    }
}