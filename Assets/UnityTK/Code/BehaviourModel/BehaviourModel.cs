using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.BehaviourModel
{
    /// <summary>
    /// A behaviour model is the base component of the behaviour model framework and acts as hub for the parts of the behaviour model (the mechanics).
    /// This is the base class for every object using the behaviour model framework.
    /// </summary>
    public class BehaviourModel : MonoBehaviour
    {
        /// <summary>
        /// Cached mapping table containing all mechanics on this model.
        /// </summary>
        private Dictionary<System.Type, BehaviourModelMechanic> mechanics = new Dictionary<System.Type, BehaviourModelMechanic>();

        /// <summary>
        /// Returns the mechanic on this model of type t.
        /// </summary>
        public T GetMechanic<T>() where T : BehaviourModelMechanic
        {
            return (T)GetMechanic(typeof(T));
        }

        /// <summary>
        /// Returns the mechanic on this model of type t.
        /// </summary>
        public BehaviourModelMechanic GetMechanic(System.Type t)
        {
            // Type check
            if (!typeof(BehaviourModelMechanic).IsAssignableFrom(t))
                throw new System.InvalidOperationException("Mechanic getter of BehaviourModel cant be called with a type not deriving from BehaviourModelMechanic");

            // Read mechanic from mapping table
            BehaviourModelMechanic mech;
            this.mechanics.TryGetValue(t, out mech);
            return mech;
        }

        /// <summary>
        /// Registers the specified mechanic.
        /// </summary>
        public void RegisterMechanic(BehaviourModelMechanic mechanic)
        {
            this.mechanics.Add(mechanic.GetType(), mechanic);
        }

        /// <summary>
        /// Deregisters the specfied mechanic.
        /// The mechanic must have been previously registered with <see cref="RegisterMechanic(BehaviourModelMechanic)"/>.
        /// </summary>
        public void DeregisterMechanic(BehaviourModelMechanic mechanic)
        {
            this.mechanics.Remove(mechanic.GetType());
        }
    }
}