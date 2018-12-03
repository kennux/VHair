using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.BehaviourModel
{
    /// <summary>
    /// Simple event for behaviour models, essentially a wrapper around a System.Action.
    /// </summary>
    public class ModelEvent<T1>
    {
        public event System.Action<T1> handler;

        public void Fire(T1 param)
        {
            if (this.handler != null)
                this.handler(param);
        }
    }

    /// <summary>
    /// Simple event for behaviour models, essentially a wrapper around a System.Action.
    /// </summary>
    public class ModelEvent<T1, T2>
    {
        public event System.Action<T1, T2> handler;

        public void Fire(T1 param1, T2 param2)
        {
            if (this.handler != null)
                this.handler(param1, param2);
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