using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace UnityTK.DataBinding.Editor
{
    [CustomPropertyDrawer(typeof(DataBindingMethodAttribute))]
    public class DataBindingMethodDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Multi-editing isnt supportedd
            if (property.serializedObject.isEditingMultipleObjects)
                return;

            DataBindingMethodAttribute attrib = (DataBindingMethodAttribute)this.attribute;

            var parent = property.serializedObject.FindProperty(attrib.parentNodeField).objectReferenceValue;
            if (!(parent is DataBindingNode))
            {
                if (!Essentials.UnityIsNull(parent))
                    Debug.LogError("Databinding field drawer can only be used on databindings whose parents are DataBindingNode implementations!");
                return;
            }

            // Gather information
            var node = (DataBindingNode)parent;
            var fields = node.GetMethods().ToArray();
            var currentField = property.stringValue;
            int currentFieldIndex = System.Array.IndexOf(fields, currentField) + 1;
            var fieldsWithNull = new string[] { "NULL", }.Concat(fields).ToArray();

            int newIndex = EditorGUI.Popup(position, property.name, currentFieldIndex, fieldsWithNull);
            if (newIndex != currentFieldIndex)
            {
                if (newIndex == 0)
                    property.stringValue = null;
                else
                    property.stringValue = fields[newIndex - 1];
            }
        }
    }
}