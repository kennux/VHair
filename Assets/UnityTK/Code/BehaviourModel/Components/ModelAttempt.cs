using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityTK.BehaviourModel
{
    /// <summary>
    /// An attempt event type that can be used to model actions that can be attempted, like reloading a gun or firing a gun.
    /// The attempt event has a list of conditions that must be met in order to fire the event.
    /// </summary>
    public class ModelAttempt<T>
    {
        public delegate void Handler(T obj);
        public delegate bool Condition(T obj);

        private List<Condition> conditions = new List<Condition>();
        /// <summary>
        /// Called on <see cref="Try"/> on success.
        /// </summary>
        public event Handler onFire;
        /// <summary>
        /// Called on <see cref="Try"/> on fail.
        /// </summary>
        public event Handler onFail;

        /// <summary>
        /// Registers a condition for firing the attempt event.
        /// </summary>
        /// <param name="condition">The condition lambda</param>
        public void RegisterCondition(Condition condition)
        {
            this.conditions.Add(condition);
        }

        /// <summary>
        /// Evaluates all <see cref="conditions"/>
        /// </summary>
        /// <returns>Whether or not this event can be fired with <see cref="Try"/></returns>
        public bool Can(T obj)
        {
            foreach (var c in this.conditions)
                if (!c(obj))
                    return false;
            return true;
        }

        /// <summary>
        /// Runs <see cref="attempt"/> and returns the value it returned.
        /// </summary>
        public bool Try(T obj)
        {
            if (this.onFire == null || !Can(obj))
            {
                if (this.onFail != null)
                    this.onFail(obj);
                return false;
            }

            this.onFire(obj);
            return true;
        }
    }

    /// <summary>
    /// An attempt event type that can be used to model actions that can be attempted, like reloading a gun or firing a gun.
    /// The attempt event has a list of conditions that must be met in order to fire the event.
    /// </summary>
    public class ModelAttempt<T1, T2>
    {
        public delegate void Handler(T1 obj1, T2 obj2);
        public delegate bool Condition(T1 obj1, T2 obj2);

        private List<Condition> conditions = new List<Condition>();
        /// <summary>
        /// Called on <see cref="Try"/> on success.
        /// </summary>
        public event Handler onFire;
        /// <summary>
        /// Called on <see cref="Try"/> on fail.
        /// </summary>
        public event Handler onFail;

        /// <summary>
        /// Registers a condition for firing the attempt event.
        /// </summary>
        /// <param name="condition">The condition lambda</param>
        public void RegisterCondition(Condition condition)
        {
            this.conditions.Add(condition);
        }

        /// <summary>
        /// Evaluates all <see cref="conditions"/>
        /// </summary>
        /// <returns>Whether or not this event can be fired with <see cref="Try"/></returns>
        public bool Can(T1 obj1, T2 obj2)
        {
            foreach (var c in this.conditions)
                if (!c(obj1, obj2))
                    return false;
            return true;
        }

        /// <summary>
        /// Runs <see cref="attempt"/> and returns the value it returned.
        /// </summary>
        public bool Try(T1 obj1, T2 obj2)
        {
            if (this.onFire == null || !Can(obj1, obj2))
            {
                if (this.onFail != null)
                    this.onFail(obj1, obj2);
                return false;
            }

            this.onFire(obj1, obj2);
            return true;
        }
    }

	/// <summary>
	/// An attempt event type that can be used to model actions that can be attempted, like reloading a gun or firing a gun.
    /// The attempt event has a list of conditions that must be met in order to fire the event.
	/// </summary>
	public class ModelAttempt
	{
        public delegate void Handler();
		public delegate bool Condition();

        private List<Condition> conditions = new List<Condition>();
        /// <summary>
        /// Called on <see cref="Try"/> on success.
        /// </summary>
        public event Handler onFire;
        /// <summary>
        /// Called on <see cref="Try"/> on fail.
        /// </summary>
        public event Handler onFail;

        /// <summary>
        /// Registers a condition for firing the attempt event.
        /// </summary>
        /// <param name="condition">The condition lambda</param>
        public void RegisterCondition(Condition condition)
        {
            this.conditions.Add(condition);
        }

        /// <summary>
        /// Evaluates all <see cref="conditions"/>
        /// </summary>
        /// <returns>Whether or not this event can be fired with <see cref="Try"/></returns>
        public bool Can()
        {
            foreach (var c in this.conditions)
                if (!c())
                    return false;
            return true;
        }

		/// <summary>
		/// Runs <see cref="attempt"/> and returns the value it returned.
		/// </summary>
		public bool Try()
		{
            if (this.onFire == null || !Can())
            {
                if (this.onFail != null)
                    this.onFail();
                return false;
            }

            this.onFire();
            return true;
        }
	}
}