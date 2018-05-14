using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.BehaviourModel
{
    /// <summary>
    /// Represents a function for a behaviour model.
    /// The function returns a value and can only be bound once.
    /// </summary>
    public class ModelFunction<TReturn>
    {
        public delegate TReturn Function();

        /// <summary>
        /// The function that is currently bound.
        /// </summary>
        private Function function;

        /// <summary>
        /// Binds the specified function as <see cref="function"/>.
        /// A function can be overridden, tho only one function can always be bound.
        /// 
        /// When overriding a warning is being logged.
        /// </summary>
        /// <param name="supressOverrideWarning">Whether or not the warning that is logged on overriding is being supressed</param>
        public void BindHandler(Function function, bool supressOverrideWarning = false)
        {
            if (!supressOverrideWarning && !ReferenceEquals(this.function, null))
                Debug.LogWarning("Overriding ModelFunction!");

            this.function = function;
        }

        /// <summary>
        /// Invokes the registered <see cref="function"/>
        /// </summary>
        public TReturn Invoke()
        {
            if (ReferenceEquals(this.function, null))
            {
                Debug.LogError("Invoked ModelFunction with no function bound!");
                return default(TReturn);
            }

            return this.function();
        }
    }

    /// <summary>
    /// Represents a function for a behaviour model.
    /// The function returns a value and can only be bound once.
    /// </summary>
    public class ModelFunction<T1, TReturn>
    {
        public delegate TReturn Function(T1 obj1);

        /// <summary>
        /// The function that is currently bound.
        /// </summary>
        private Function function;

        /// <summary>
        /// Binds the specified function as <see cref="function"/>.
        /// A function can be overridden, tho only one function can always be bound.
        /// 
        /// When overriding a warning is being logged.
        /// </summary>
        /// <param name="supressOverrideWarning">Whether or not the warning that is logged on overriding is being supressed</param>
        public void BindHandler(Function function, bool supressOverrideWarning = false)
        {
            if (!supressOverrideWarning && !ReferenceEquals(this.function, null))
                Debug.LogWarning("Overriding ModelFunction!");

            this.function = function;
        }

        /// <summary>
        /// Invokes the registered <see cref="function"/>
        /// </summary>
        public TReturn Invoke(T1 obj1)
        {
            if (ReferenceEquals(this.function, null))
            {
                Debug.LogError("Invoked ModelFunction with no function bound!");
                return default(TReturn);
            }

            return this.function(obj1);
        }
    }

    /// <summary>
    /// Represents a function for a behaviour model.
    /// The function returns a value and can only be bound once.
    /// </summary>
    public class ModelFunction<T1, T2, TReturn>
    {
        public delegate TReturn Function(T1 obj1, T2 obj2);

        /// <summary>
        /// The function that is currently bound.
        /// </summary>
        private Function function;

        /// <summary>
        /// Binds the specified function as <see cref="function"/>.
        /// A function can be overridden, tho only one function can always be bound.
        /// 
        /// When overriding a warning is being logged.
        /// </summary>
        /// <param name="supressOverrideWarning">Whether or not the warning that is logged on overriding is being supressed</param>
        public void BindHandler(Function function, bool supressOverrideWarning = false)
        {
            if (!supressOverrideWarning && !ReferenceEquals(this.function, null))
                Debug.LogWarning("Overriding ModelFunction!");

            this.function = function;
        }

        /// <summary>
        /// Invokes the registered <see cref="function"/>
        /// </summary>
        public TReturn Invoke(T1 obj1, T2 obj2)
        {
            if (ReferenceEquals(this.function, null))
            {
                Debug.LogError("Invoked ModelFunction with no function bound!");
                return default(TReturn);
            }

            return this.function(obj1, obj2);
        }
    }

}