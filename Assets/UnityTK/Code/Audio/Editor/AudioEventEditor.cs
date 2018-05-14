using UnityEngine;
using System.Collections;
using UnityEditor;

namespace UnityTK.Audio.Editor
{
    /// <summary>
    /// Customized unity inspector for <see cref="AudioEvent"/>.
    /// Supports previewing the audio event in the editor by clicking a button.
    /// 
    /// Taken from "Unite 2016 - Overthrowing the MonoBehaviour Tyranny in a Glorious Scriptable Object Revolution" - https://www.youtube.com/watch?v=6vmRwLYWNRo
    /// </summary>
    [CustomEditor(typeof(AudioEvent), true)]
    public class AudioEventEditor : UnityEditor.Editor
    {

        [SerializeField] private NonSpatialAudioSource _previewer;

        public void OnEnable()
        {
            _previewer = EditorUtility.CreateGameObjectWithHideFlags("Audio preview", HideFlags.HideAndDontSave, typeof(NonSpatialAudioSource)).GetComponent<NonSpatialAudioSource>();
        }

        public void OnDisable()
        {
            DestroyImmediate(_previewer.gameObject);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);
            if (GUILayout.Button("Preview"))
            {
                ((AudioEvent)target).Play(_previewer);
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}