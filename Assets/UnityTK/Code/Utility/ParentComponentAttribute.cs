using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK
{
    /// <summary>
    /// Parent component property attribute that can be used to annotate fields which are being selected from a set of components gathered from the gameobject parents.
    /// </summary>
    public class ParentComponentAttribute : PropertyAttribute
    {
        public System.Type targetType;

        /// <summary>
        /// Can be set to specify a field where the gameobject that is being used to retrieve the parents is retrieved from.
        /// If not setup, the tpye this attribute is used on is assumed to derive from <see cref="UnityEngine.Component"/>.
        /// </summary>
        public string gameObjectOverrideField = null;

        public ParentComponentAttribute(System.Type targetType)
        {
            this.targetType = targetType;
        }
    }
}