using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK
{
    /// <summary>
    /// Implements a bit data storage that can be used to mass-store bits in an efficient way.
    /// Its essentially a bit array using a uint array in the underlying implementation.
    /// 
    /// Specific bits are set / read using bit arithmetic.
    /// </summary>
    public class NBitDataMap
    {
        public uint[] data;
        public readonly int nBits;

        public NBitDataMap(int elements, int nBits)
        {
            this.nBits = nBits;
            data = new uint[Mathf.CeilToInt((elements / 32f) * (float)nBits)];
        }

        public void Clear()
        {
            for (int i = 0; i < data.Length; i++)
                data[i] = 0;
        }

        private void GetAddress(int index, int bitNum, out int arrayIndex, out uint mask)
        {
            var bitIndex = (index * this.nBits) + bitNum;
            arrayIndex = Mathf.FloorToInt(bitIndex / 32f);
            mask = 1u << (bitIndex % 32);
        }
        
        public bool Get(int index, int bitNum)
        {
            if (bitNum >= this.nBits)
            {
                Debug.LogError("Out of bounds acces in bit data map! bitNum >= nBits");
                return false;
            }

            // Get bit address
            int arrayIndex;
            uint mask;
            GetAddress(index, bitNum, out arrayIndex, out mask);

            // Check for validity
            if (arrayIndex >= this.data.Length || arrayIndex < 0)
            {
                Debug.LogError("Out of bounds access in bit data map!");
                return false;
            }

            // Get
            return (data[arrayIndex] & mask) != 0;
        }
        
        public void Set(int index, int bitNum, bool value)
        {
            if (bitNum >= this.nBits)
            {
                Debug.LogError("Out of bounds acces in bit data map! bitNum >= nBits");
            }
            // Get bit address
            int arrayIndex;
            uint mask;
            GetAddress(index, bitNum, out arrayIndex, out mask);

            // Check for validity
            if (arrayIndex >= this.data.Length || arrayIndex < 0)
            {
                Debug.LogError("Out of bounds access in bit data map!");
                return;
            }

            // Set
            if (value)
                data[arrayIndex] |= mask;
            else
                data[arrayIndex] &= ~mask;
        }
    }

}