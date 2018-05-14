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
    [Serializable]
    public class HairMovability
    {
        /// <summary>
        /// Internal data for storing the movability bits.
        /// </summary>
        [SerializeField]
        private uint[] _data;

        public HairMovability(int vertices)
        {
            this._data = new uint[Mathf.CeilToInt(vertices / 32f)];
        }

        public void SetMovable(int index, bool isMovable)
        {
            int dataIndex = Mathf.FloorToInt(index / 32f);
            if (isMovable)
                this._data[dataIndex] |= (1u >> (index % 32));
            else
                this._data[dataIndex] &= ~(1u >> (index % 32));
        }

        public bool IsMovable(int index)
        {
            int dataIndex = Mathf.FloorToInt(index / 32f);
            return (this._data[dataIndex] & (1u >> (index % 32))) != 0;
        }
    }
}
