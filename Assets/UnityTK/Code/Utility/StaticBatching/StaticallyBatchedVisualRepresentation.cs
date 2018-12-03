using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK
{
    /// <summary>
    /// Statically batched visual representation.
    /// 
    /// This behaviour can be used on static gameobjects, in order to create a visual representation.
    /// It takes a gameobject and will bake it into a statically batched mesh in <see cref="StaticBatching"/>.
    /// 
    /// Whenever your transform changed it will cause a static mesh rebake, so only use this on objects that dont ever move.
    /// Use it on moving objects with caution!
    /// </summary>
    public class StaticallyBatchedVisualRepresentation : MonoBehaviour
    {
        /// <summary>
        /// The visualization used by this static batched visual representation.
        /// </summary>
        [Header("Configuration")]
        public GameObject visualization;

        private Vector3 pos;
        private Quaternion rot;
        private Vector3 scale;

        /// <summary>
        /// Updates the visualization using <see cref="StaticBatching"/>
        /// </summary>
        private void UpdateStaticallyBatchedVis()
        {
            StaticBatching.instance.DestroyVisualRepresentations(this);
            StaticBatching.instance.InsertVisualRepresentation(this.visualization, this.transform.localToWorldMatrix, this);

            this.pos = this.transform.position;
            this.rot = this.transform.rotation;
            this.scale = this.transform.lossyScale;
        }

        public void Start()
        {
            UpdateStaticallyBatchedVis();
        }

        public void Update()
        {
            if (!this.gameObject.isStatic)
            {
                Vector3 p = this.transform.position, s = this.transform.lossyScale;
                Quaternion r = this.transform.rotation;

                if (p != this.pos || r != this.rot || s != this.scale)
                    UpdateStaticallyBatchedVis();
            }
        }

        public void OnDestroy()
        {
            StaticBatching batching = StaticBatching.GetInstanceOnDestroy();
            if (!Essentials.UnityIsNull(batching))
                batching.DestroyVisualRepresentations(this);
        }
    }
}