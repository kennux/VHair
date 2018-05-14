using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.BehaviourModel
{
	/// <summary>
	/// Defines an arbitrary property that can be assigned a getter and a setter.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ModelProperty<T>
	{
		private System.Func<T> onGetValue;
		public event System.Action<T> onSetValue;

		public void SetGetter(System.Func<T> getter)
		{
            if (this.onGetValue != null)
                Debug.LogWarning("Model property overridden from " + this.onGetValue + " to " + getter);

			this.onGetValue = getter;
		}

		public T Get()
		{
			if (this.onGetValue == null)
			{
				Debug.LogWarning("Tried getting value event with no getter!");
				return default(T);
			}

			return this.onGetValue();
		}

		public void Set(T obj)
		{
			if (this.onSetValue == null)
				Debug.LogWarning("Tried setting value event with no setter!");
			else
				this.onSetValue(obj);
		}
	}
}