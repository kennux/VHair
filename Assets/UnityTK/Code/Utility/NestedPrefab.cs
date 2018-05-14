using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK
{
    /// <summary>
    /// A very poor (tho the best you can get most likely) nested prefab implementation.
    /// This component will simply instantiate the specified prefab on Awake() in runtime, after that it will Destroy() itself to save memory.
    /// Note: Prefabs are being instantiated as child of the NestedPrefab gameobject in _LOCAL SPACE_ (_NOT_ worldspace).
    /// 
    /// This is a simple, yet effective way to realize nested prefabs until unity finally implements them.
    /// </summary>
    public class NestedPrefab : MonoBehaviour
    {
        /// <summary>
        /// The prefab that will be instantiated.
        /// </summary>
        public GameObject prefab;

        public void Awake()
        {
            Instantiate(this.prefab, this.transform, false);
            Destroy(this);
        }
    }
}