using UnityEditor;
using UnityEngine;

namespace VHair.Editor
{
	[CustomEditor(typeof(CPUTFXLocalShapeConstraints)), CanEditMultipleObjects]
	public class PositionHandleExampleEditor : UnityEditor.Editor
	{
		protected virtual void OnSceneGUI()
		{
			if (!Application.isPlaying)
				return;

			CPUTFXLocalShapeConstraints example = (CPUTFXLocalShapeConstraints)target;

			if (example.debugGlobalTransform)
			{
				EditorGUI.BeginChangeCheck();
				var verts = example.instance.vertices.CpuReference;
				var globalTransforms = example.globalTransform;
				for (int i = 0; i < verts.Length; i++)
				{
					if (i % example.debugSteps == 0)
						Handles.PositionHandle(verts[i], globalTransforms[i]);
				}

				EditorGUI.EndChangeCheck();
			}

			if (example.debugRealtime)
			{
				EditorGUI.BeginChangeCheck();
				var verts = example.instance.vertices.CpuReference;
				var globalTransforms = example.realtimeDebug;
				for (int i = 0; i < verts.Length; i++)
				{
					if (i % example.debugSteps == 0)
						Handles.PositionHandle(verts[i], globalTransforms[i]);
				}

				EditorGUI.EndChangeCheck();
			}

			if (example.debugLocalTransform)
			{
				EditorGUI.BeginChangeCheck();
				var verts = example.instance.vertices.CpuReference;
				var globalTransforms = example.globalTransform;
				var localTransforms = example.localTransform;
				for (int i = 0; i < verts.Length; i++)
				{
					if (i % example.debugSteps == 0)
						Handles.PositionHandle(verts[i], globalTransforms[i] * localTransforms[i]);
				}

				EditorGUI.EndChangeCheck();
			}
		}
	}
}