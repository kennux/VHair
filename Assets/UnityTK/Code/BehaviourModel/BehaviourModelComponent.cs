using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.BehaviourModel
{
    /// <summary>
    /// Base class for implementing components of a specific <see cref="BehaviourModel"/>.
    /// </summary>
    [RequireComponent(typeof(BehaviourModel))]
    public abstract class BehaviourModelComponent : MonoBehaviour
    {
        public BehaviourModel model
        {
            get { return this._model.Get(this); }
        }
        private LazyLoadedComponentRef<BehaviourModel> _model = new LazyLoadedComponentRef<BehaviourModel>();
    }
}