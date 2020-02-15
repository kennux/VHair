using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Collections;

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
		/// <summary>
		/// Cpu side of the data
		/// </summary>
		private NativeArray<T> cpuData;

		/// <summary>
		/// Gpu side of the data
		/// </summary>
		private ComputeBuffer gpuData;

		private bool gpuDirty;
		private bool cpuDirty;

		private T[] tmpGpuSync; // TODO: Get rid of this once unity api is not retarded anymore and has ComputeBuffer.GetData() for NativeArray<T> (They have SetData(NativeArray<T>) already >_>).

		public int Count
		{
			get { return this.cpuData.Length; }
		}

		public NativeArray<T> CpuReference
		{
			get
			{
				if (this.cpuDirty)
					UpdateCPUData();
				return this.cpuData;
			}
		}

		public ComputeBuffer GpuReference
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
			this.cpuData = new NativeArray<T>(data, Allocator.Persistent);
			this.UpdateGPUData();
		}

		public CPUGPUData(NativeArray<T> data, int dataStride)
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
			if (tmpGpuSync == null || tmpGpuSync.Length != this.gpuData.count)
				tmpGpuSync = new T[this.gpuData.count];

			this.gpuData.GetData(tmpGpuSync);
			cpuData.CopyFrom(tmpGpuSync);
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
			this.cpuData.Dispose();
		}
	}
}
