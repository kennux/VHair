using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace UnityTK.BehaviourModel
{
    /// <summary>
    /// An activity can be started and stopped. A start or stop can fail.
    /// An example for an activity would be running in a first person shooter.
    /// When the player presses the run button, the activity is being started and its being stopped when the button is being released.
	/// </summary>
	public class ModelActivity
	{
		public delegate void Callback();
		public delegate bool Condition();

		public event Callback onStart;
		public event Callback onStop;
		public event Callback onFailStart;
		public event Callback onFailStop;

		protected List<Condition> startConditions = new List<Condition>();
		protected List<Condition> stopConditions = new List<Condition>();
		protected List<Condition> isActiveGetters = new List<Condition>();

		public bool IsActive()
		{
			bool isActive = false;
			for (int i = 0; i < isActiveGetters.Count && !isActive; i++)
				isActive |= isActiveGetters[i]();
			return isActive;
		}

		/// <summary>
		/// Forces starting the specified activity.
		/// Use this with care, usually when you are trying to start an event you should use <see cref="TryStart"/>
		/// </summary>
		public void ForceStart()
		{
			if (this.onStart != null)
				this.onStart();
		}

		/// <summary>
		/// Forces stopping the specified activity.
		/// Use this with care, usually when you are trying to start an event you should use <see cref="TryStop"/>
		/// </summary>
		public void ForceStop()
		{
			if (this.onStop != null)
				this.onStop();
		}

		/// <summary>
		/// Tries to start this activity and returns whether or not it was started.
		/// Will invoke <see cref="onStart"/> or <see cref="onFailStart"/>.
		/// </summary>
		/// <returns></returns>
		public bool TryStart()
		{
			if (CanStart())
			{
				this.ForceStart();
				return true;
			}
			else
			{
				if (this.onFailStart != null)
					this.onFailStart();

				return false;
			}
		}

		/// <summary>
		/// Tries to stop this activity and returns whether or not it was started.
		/// Will invoke <see cref="onStop"/> or <see cref="onFailStop"/>.
		/// </summary>
		/// <returns></returns>
		public bool TryStop()
		{
			if (CanStop())
			{
				this.ForceStop();
				return true;
			}
			else
			{
				if (this.onFailStop != null)
					this.onFailStop();

				return false;
			}
		}

		/// <summary>
		/// Determines whether or not this activity can be started by evaluating all start conditions <see cref="startConditions"/>
		/// </summary>
		public bool CanStart()
		{
			return CheckConditions(this.startConditions);
		}

		/// <summary>
		/// Determines whether or not this activity can be stopped by evaluating all stop conditions <see cref="stopConditions"/>
		/// </summary>
		public bool CanStop()
		{
			return IsActive() && CheckConditions(this.stopConditions);
		}

		public void RegisterStartCondition(Condition condition)
		{
			this.startConditions.Add(condition);
		}

		public void RegisterStopCondition(Condition condition)
		{
			this.stopConditions.Add(condition);
		}

		/// <summary>
		/// Registers the specified condition as an "activity getter" which returns a boolean.
		/// The output of the registered activity getter's is used to determine whether or not this activity is currently active.
		/// </summary>
		/// <param name="getter"></param>
		public void RegisterActivityGetter(Condition getter)
		{
			this.isActiveGetters.Add(getter);
		}

		private bool CheckConditions(List<Condition> lst)
		{
			bool canStart = true;
			for (int i = 0; i < lst.Count && canStart; i++)
				canStart &= lst[i]();
			return canStart;
		}
	}


    /// <summary>
    /// An activity can be started and stopped. A start or stop can fail.
    /// An example for an activity would be running in a first person shooter.
    /// When the player presses the run button, the activity is being started and its being stopped when the button is being released.
    /// </summary>
    public class ModelActivity<T>
	{
		public delegate void Callback(T obj);
		public delegate void CallbackNoParam();
		public delegate bool Condition(T obj);
		public delegate bool ConditionNoParam();

		public event Callback onStart;
		public event CallbackNoParam onStop;
		public event Callback onFailStart;
		public event CallbackNoParam onFailStop;

		protected List<Condition> startConditions = new List<Condition>();
		protected List<ConditionNoParam> stopConditions = new List<ConditionNoParam>();
		protected List<ConditionNoParam> isActiveGetters = new List<ConditionNoParam>();

		public bool IsActive()
		{
			bool isActive = false;
			for (int i = 0; i < isActiveGetters.Count && !isActive; i++)
				isActive |= isActiveGetters[i]();
			return isActive;
		}

		/// <summary>
		/// Forces starting the specified activity.
		/// Use this with care, usually when you are trying to start an event you should use <see cref="TryStart"/>
		/// </summary>
		public void ForceStart(T obj)
		{
			if (this.onStart != null)
				this.onStart(obj);
		}

		/// <summary>
		/// Forces stopping the specified activity.
		/// Use this with care, usually when you are trying to start an event you should use <see cref="TryStop"/>
		/// </summary>
		public void ForceStop()
		{
			if (this.onStop != null)
				this.onStop();
		}

		/// <summary>
		/// Tries to start this activity and returns whether or not it was started.
		/// Will invoke <see cref="onStart"/> or <see cref="onFailStart"/>.
		/// </summary>
		/// <returns></returns>
		public bool TryStart(T obj)
		{
			if (CanStart(obj))
			{
				if (this.onStart != null)
					this.onStart(obj);

				return true;
			}
			else
			{
				if (this.onFailStart != null)
					this.onFailStart(obj);

				return false;
			}
		}

		/// <summary>
		/// Tries to stop this activity and returns whether or not it was started.
		/// Will invoke <see cref="onStop"/> or <see cref="onFailStop"/>.
		/// </summary>
		/// <returns></returns>
		public bool TryStop()
		{
			if (CanStop())
			{
				if (this.onStop != null)
					this.onStop();

				return true;
			}
			else
			{
				if (this.onFailStop != null)
					this.onFailStop();

				return false;
			}
		}

		/// <summary>
		/// Determines whether or not this activity can be started by evaluating all start conditions <see cref="startConditions"/>
		/// </summary>
		public bool CanStart(T obj)
		{
			return CheckConditions(this.startConditions, obj);
		}

		/// <summary>
		/// Determines whether or not this activity can be stopped by evaluating all stop conditions <see cref="stopConditions"/>
		/// </summary>
		public bool CanStop()
		{
			return CheckConditions(this.stopConditions);
		}

		public void RegisterStartCondition(Condition condition)
		{
			this.startConditions.Add(condition);
		}

		public void RegisterStopCondition(ConditionNoParam condition)
		{
			this.stopConditions.Add(condition);
		}

		/// <summary>
		/// Registers the specified condition as an "activity getter" which returns a boolean.
		/// The output of the registered activity getter's is used to determine whether or not this activity is currently active.
		/// </summary>
		/// <param name="getter"></param>
		public void RegisterActivityGetter(ConditionNoParam getter)
		{
			this.isActiveGetters.Add(getter);
		}

		private bool CheckConditions(List<Condition> lst, T obj)
		{
			bool canStart = true;
			for (int i = 0; i < lst.Count && canStart; i++)
				canStart &= lst[i](obj);
			return canStart;
		}

		private bool CheckConditions(List<ConditionNoParam> lst)
		{
			bool canStart = true;
			for (int i = 0; i < lst.Count && canStart; i++)
				canStart &= lst[i]();
			return canStart;
		}

	}
}