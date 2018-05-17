using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

namespace VHair
{
    /// <summary>
    /// Data structure for hair movability mask.
    /// 
    /// This is stored internally as an array of unsigned integers.
    /// </summary>
    public static class HairMovability
    {
        /// <summary>
        /// Creates a new array to store the movability data for the specified vertex count.
        /// Array size: Ceil(vertexCount / 32f)
        /// </summary>
        /// <param name="vertexCount"></param>
        /// <returns>New uint array instance</returns>
        public static uint[] CreateData(int vertexCount)
        {
            return new uint[Mathf.CeilToInt(vertexCount / 32f)];
        }

        /// <summary>
        /// Helper method of setting movability bits in the specified data array.
        /// </summary>
        /// <param name="index">The vertex index to set</param>
        /// <param name="isMovable">The flag state to set</param>
        /// <param name="movability">Movability data, previously created with <see cref="CreateData(int)"/></param>
        public static void SetMovable(int index, bool isMovable, uint[] movability)
        {
            int dataIndex = Mathf.FloorToInt(index / 32f);
            if (isMovable)
                movability[dataIndex] |= (1u << (index % 32));
            else
                movability[dataIndex] &= ~(1u << (index % 32));
        }

        /// <summary>
        /// Returns whether or not the specified vertex index is set as movable in the specified movability data.
        /// </summary>
        /// <param name="index">The vertex index</param>
        /// <param name="movability">The movability data</param>
        /// <returns>Whether or not this vertex is movable.</returns>
        public static bool IsMovable(int index, uint[] movability)
        {
            int dataIndex = Mathf.FloorToInt(index / 32f);
            return (movability[dataIndex] & (1u << (index % 32))) != 0;
        }
    }
}
