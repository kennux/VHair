using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnityTK
{
    /// <summary>
    /// Datastructure for creating object and structure fields which can be "observed".
    /// Observers are able to bind handlers to events regarding the property.
    /// </summary>
    public struct ObservableProperty<T>
    {
        public delegate void PropertyChangedEvent(T newValue);

        /// <summary>
        /// Invoked every time the value of this property is being set.
        /// </summary>
        public event PropertyChangedEvent onChanged;

        /// <summary>
        /// Wrapped value field.
        /// </summary>
        public T value
        {
            get { return _value; }
            set
            {
                this._value = value;
                this.onChanged?.Invoke(value);
            }
        }
        private T _value;
    }
}