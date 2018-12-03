using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace UnityTK.Editor
{
    /// <summary>
    /// Property drawer implementation for <see cref="ParentComponentAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(ParentComponentAttribute))]
    public class ParentComponentDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Multi-editing isnt supportedd
            if (property.serializedObject.isEditingMultipleObjects)
                return;

            ParentComponentAttribute attrib = (ParentComponentAttribute)this.attribute;

            // Type check
            var self = property.serializedObject.targetObject;
            GameObject go = null;
            var value = property.objectReferenceValue;
            if (!(self is Component))
            {
                var goProp = attrib.gameObjectOverrideField;
                if (string.IsNullOrEmpty(goProp))
                {
                    Debug.LogError("Parent component attribute used on an object that doesnt drive from Component and hasnt specified");
                    return;
                }
                else
                    go = property.serializedObject.FindProperty(goProp).objectReferenceValue as GameObject;
            }
            else
                go = (self as Component).gameObject;

            if (Essentials.UnityIsNull(go))
            {
                Debug.LogError("Cannot retrieve a gameobject for the property " + property);
                return;
            }

            // Gather info
            var valueComponent = value as Component;
            var availableComponents = go.GetComponentsInParent(attrib.targetType, true).Concat(go.GetComponents(attrib.targetType)).Distinct().Where((c) => !object.ReferenceEquals(self, c)).ToArray();
            var availableComponentsStr = new string[] { "NULL", }.Concat(availableComponents.Select((c) => c.ToString())).ToArray();
            var currentlySelected = property.objectReferenceValue;
            int currentlySelectedIndex = System.Array.IndexOf(availableComponents, valueComponent) + 1;

            int newIndex = EditorGUI.Popup(position, property.name, currentlySelectedIndex, availableComponentsStr);
            if (newIndex != currentlySelectedIndex)
            {
                if (newIndex == 0)
                    property.objectReferenceValue = null;
                else
                    property.objectReferenceValue = availableComponents[newIndex-1];
            }
        }
    }
}