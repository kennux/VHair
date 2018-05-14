using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityTK.BehaviourModel
{
	[System.Serializable]
	public class ModelModifiableFloat : ModelModifiableValue<float>
	{
		public ModelModifiableFloat(float val) : base(val) { }
		public ModelModifiableFloat() : base() { }
	}
	[System.Serializable]
	public class ModelModifiableInt : ModelModifiableValue<int>
	{
		public ModelModifiableInt(int val) : base(val) { }
		public ModelModifiableInt() : base() { }
	}

	/// <summary>
	/// Base class for a field that can be modified by code.
	/// This can be used on components if you want to provide an easy way to override them.
	/// 
	/// They need to be initialized with their base (usually in Awake()).
	/// Note that this generic class cannot be used if unity serialization is needed.
	/// For this purpose there are specialized variants of this class without generics available:
	/// 
	/// <see cref="ModelModifiableFloat"/>
	/// <see cref="ModelModifiableInt"/>
	/// 
	/// This class is unlike the <see cref="ModelProperty{T}"/> meant to be set by the editor instead of being set at runtime.
	/// If you need a value that can only be obtained by runtime evaluation, you want to use <see cref="ModelProperty{T}"/>.
	/// 
	/// Overriding behaviour is using a layered approach, you can set an <see cref="OverrideEvaluator"/> on a specific layer.
	/// When the value is evaluated (get) those evaluators are evaluated in ascending layer order.
	/// </summary>
	[System.Serializable]
	public abstract class ModelModifiableValue<T>
	{
		/// <summary>
		/// Override evaluator entry, essentially a tuple.
		/// </summary>
		private class OverrideEvaluatorEntry
		{
			public static int Compare(OverrideEvaluatorEntry entry, OverrideEvaluatorEntry other)
			{
				return entry.layer.CompareTo(other.layer);
			}

			public OverrideEvaluator evaluator;
			public int layer;

			public OverrideEvaluatorEntry(OverrideEvaluator evaluator, int layer)
			{
				this.evaluator = evaluator;
				this.layer = layer;
			}
		}

		/// <summary>
		/// Delegate for applying an override evaluator.
		/// <see cref="AddOverrideEvaluator(OverrideEvaluator)"/>
		/// </summary>
		/// <param name="value">The value that is being overridden (evaluated from lower layers)</param>
		/// <returns>The overriden value</returns>
		public delegate T OverrideEvaluator(T value);

		[FormerlySerializedAs("value")]
		[SerializeField]
		private T baseValue;

		/// <summary>
		/// Sorted boxed list storing all override evaluators on all layers.
		/// </summary>
		private List<OverrideEvaluatorEntry> layers = new List<OverrideEvaluatorEntry>();
		private bool _layerSortDirty = false;

		public ModelModifiableValue(T baseValue)
		{
			this.baseValue = baseValue;
		}

		public ModelModifiableValue()
		{

		}
		
		/// <summary>
		/// Adds an override evaluator on the specified layer.
		/// </summary>
		/// <param name="evaluator"></param>
		public void AddOverrideEvaluator(OverrideEvaluator evaluator, int layer)
		{
			this.layers.Add(new OverrideEvaluatorEntry(evaluator, layer));
			this._layerSortDirty = true;
		}

		public T GetBase()
		{
			return this.baseValue;
		}

		/// <summary>
		/// Evaluates all overrides and returns the modified value.
		/// </summary>
		public T Get()
		{
			// Sort if needed
			if (this._layerSortDirty)
			{
				this.layers.Sort(OverrideEvaluatorEntry.Compare);
				this._layerSortDirty = false;
			}

			T value = this.baseValue;
			for (int i = 0; i < this.layers.Count; i++)
			{
				// Evaluate
				value = this.layers[i].evaluator(value);
			}

			return value;
		}
	}
}
