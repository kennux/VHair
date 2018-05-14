using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityTK.BuildSystem
{
    /// <summary>
    /// Custom inspector implementation for <see cref="BuildJob"/>
    /// </summary>
    [CustomEditor(typeof(BuildJob))]
    public class BuildJobEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            if (this.serializedObject.isEditingMultipleObjects)
                return;

            var job = (BuildJob)this.target;
            var dstProp = this.serializedObject.FindProperty("destination");
            var clearDstProp = this.serializedObject.FindProperty("deleteExistingDestination");
            var tasksProp = this.serializedObject.FindProperty("tasks");
            EditorGUILayout.PropertyField(tasksProp, true);

            // Draw tasks
            var tasks = ((BuildJob)this.target).tasks;
            if (tasks != null)
            {
                for (int i = 0; i < tasks.Length; i++)
                {
                    if (Essentials.UnityIsNull(tasks[i]))
                        continue;

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(tasks[i].ToString(), EditorStyles.boldLabel);
                    EditorGUILayout.Space();

                    CreateEditor(tasks[i]).OnInspectorGUI();
                }
            }

            for (int i = 0; i < 5; i++)
                EditorGUILayout.Space();

            EditorGUILayout.LabelField("Build controls", EditorStyles.boldLabel);
            // Build controls
            // Destination settings
            EditorGUILayout.PropertyField(dstProp);
            if (GUILayout.Button("Select Path"))
            {
                dstProp.stringValue = EditorUtility.OpenFolderPanel("Select build destination", dstProp.stringValue, "Build");
            }
            
            EditorGUILayout.PropertyField(clearDstProp);

            // Job settings
            if (GUILayout.Button("Build Job"))
            {
                job.Run(new BuildJobParameters(dstProp.stringValue, clearDstProp.boolValue));
            }

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}