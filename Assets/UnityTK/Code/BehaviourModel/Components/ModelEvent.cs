using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.BehaviourModel
{
    /// <summary>
    /// Simple event for behaviour models, essentially a wrapper around a System.Action.
    /// </summary>
    public class ModelEvent<T>
	{
		public event System.Action<T> handler;

		public void Fire(T param)
		{
			if (this.handler != null)
				this.handler(param);
		}
	}

	/// <summary>
	/// Simple event for behaviour models, essentially a wrapper around a System.Action.
	/// </summary>
	public class ModelEvent
	{
		public event System.Action handler;

		public void Fire()
		{
			if (this.handler != null)
				this.handler();
		}
	}
}