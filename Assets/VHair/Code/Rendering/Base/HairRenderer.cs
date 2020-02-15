using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VHair
{
	/// <summary>
	/// Abstract base class for implementing VHair renderers.
	/// </summary>
	public abstract class HairRenderer : MonoBehaviour
	{
		public HairInstance instance;

		public virtual void OnValidate()
		{
			if (instance == null)
				instance = GetComponent<HairInstance>();
		}
	}
}