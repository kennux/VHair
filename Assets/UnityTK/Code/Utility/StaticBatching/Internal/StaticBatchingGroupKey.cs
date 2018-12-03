using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

namespace UnityTK
{
    /// <summary>
    /// The static batching key to be used to group batches together in <see cref="StaticBatching"/>.
    /// This key is composed of:
    /// - MeshRenderer.sharedMaterial
    /// - GameObject.layer
    /// - The region of this mesh
    /// </summary>
    internal struct StaticBatchingGroupKey : IEquatable<StaticBatchingGroupKey>
    {
        /// <summary>
        /// The material of this batching group key.
        /// MeshRenderer.sharedMaterial from the visual representation.
        /// </summary>
        public Material material;

        /// <summary>
        /// The layer of this batching group key.
        /// <see cref="GameObject.layer"/>
        /// </summary>
        public int layer;

        /// <summary>
        /// The chunk of this mesh.
        /// </summary>
        public Vector3Int chunk;

        public override int GetHashCode()
        {
            return Essentials.CombineHashCodes(this.chunk.GetHashCode(), Essentials.CombineHashCodes(this.material.GetHashCode(), this.layer.GetHashCode()));
        }

        public bool Equals(StaticBatchingGroupKey other)
        {
            return other.layer == this.layer && ReferenceEquals(this.material, other.material);
        }
    }
}
