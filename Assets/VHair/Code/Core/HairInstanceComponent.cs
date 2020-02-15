using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK;

namespace VHair
{
	/// <summary>
	/// Base class for VHair <see cref="HairInstance"/> components.
	/// Contains a lazy loaded component ref to the hair instance for convenience.
	/// </summary>
	/// <typeparam name="T">The <see cref="HairInstance"/> implementation this component can be used with.</typeparam>
	[RequireComponent(typeof(HairInstance))]
	public abstract class HairInstanceComponent<T> : MonoBehaviour where T : HairInstance
	{
		// TODO: Implement OnValidate for T
		public T instance
		{
			get
			{
				return this._instance.Get(this);
			}
		}
		private LazyLoadedComponentRef<T> _instance = new LazyLoadedComponentRef<T>();
	}
}