using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.BehaviourModel
{
    /// <summary>
    /// Base class for implementing behaviour model mechanics, which are defining behaviour using the behaviour model components.
    /// For example a player behaviour model could have its behaviour model component and another movement mechanic component(which derives from <see cref="BehaviourModelMechanic"/>).
    /// <see cref="BehaviourModelMechanicComponents"/> can be used to set up the components for this mechanic.
    /// </summary>
    [RequireComponent(typeof(BehaviourModel))]
    public abstract class BehaviourModelMechanic : BehaviourModelComponent
    {
        /// <summary>
        /// Can be overridden to setup constraints, for example activity constraints so a fire activity cant be triggered while running (<see cref="ModelActivity.RegisterStartCondition(ModelActivity.Condition)"/>).
        /// Called from <see cref="Awake"/>
        /// </summary>
        protected abstract void SetupConstraints();

        /// <summary>
        /// Called in order to register this mechanic on the model.
        /// </summary>
        protected virtual void RegisterOnModel()
        {
            this.model.RegisterMechanic(this);
        }

        /// <summary>
        /// Called in order to deregister this mechanic on the model.
        /// </summary>
        protected virtual void DeregisterOnModel()
        {
            this.model.DeregisterMechanic(this);
        }

        protected virtual void Awake()
        {
            this.SetupConstraints();
            this.RegisterOnModel();
        }

        protected virtual void OnDestroy()
        {
            this.DeregisterOnModel();
        }
    }
}