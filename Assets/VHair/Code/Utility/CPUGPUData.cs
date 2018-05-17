using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

namespace VHair
{
    /// <summary>
    /// An array of structs thats being kept on GPU and CPU.
    /// Once one of them was modified, a lazy update pattern updates the counterpart as soon as its needed.
    /// 
    /// Access is synchronized whenever the reference properties are accessed.
    /// 
    /// Whenever accessing the data this object stores, do _NEVER_ cache references to the data references.
    /// Invoking the property getter will synchronize them if dirty, if you dont invoke it before working your data might is not synchronized!
    /// </summary>
    public class CPUGPUData<T> : IDisposable where T : struct
    {
        public int count
        {
            get { return this.cpuData.Length; }
        }

        /// <summary>
        /// Cpu side of the data
        /// </summary>
        private T[] cpuData;

        /// <summary>
        /// Gpu side of the data
        /// </summary>
        private ComputeBuffer gpuData;

        private bool gpuDirty;
        private bool cpuDirty;

        public T[] cpuReference
        {
            get
            {
                if (this.cpuDirty)
                    UpdateCPUData();
                return this.cpuData;
            }
        }

        public ComputeBuffer gpuReference
        {
            get
            {
                if (this.gpuDirty)
                    UpdateGPUData();
                return this.gpuData;
            }
        }

        public CPUGPUData(T[] data, int dataStride)
        {
            this.gpuData = new ComputeBuffer(data.Length, dataStride);
            this.cpuData = data;
            this.UpdateGPUData();
        }

        /// <summary>
        /// Copies the data from cpu to gpu
        /// </summary>
        private void UpdateGPUData()
        {
            this.gpuData.SetData(this.cpuData);
            this.gpuDirty = false;
        }

        /// <summary>
        /// Copies the data from gpu to cpu
        /// </summary>
        private void UpdateCPUData()
        {
            this.gpuData.GetData(this.cpuData);
            this.cpuDirty = false;
        }

        /// <summary>
        /// Marks the gpu data as dirty (it was modified on the cpu).
        /// The next retrieval will update it.
        /// </summary>
        public void SetGPUDirty()
        {
            this.gpuDirty = true;
        }

        /// <summary>
        /// Marks the cpu data as dirty.
        /// The next retrieval will update it.
        /// </summary>
        public void SetCPUDirty()
        {
            this.cpuDirty = true;
        }

        public void Dispose()
        {
            this.gpuData.Dispose();
        }
    }
}
