using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.DataBinding
{
    /// <summary>
    /// Databinding field attribute that can be used to draw inspector fields for databinding node fields using a ui popup.
    /// </summary>
    public class DataBindingFieldAttribute : PropertyAttribute
    {
        public string parentNodeField = "parentNode";
    }
}