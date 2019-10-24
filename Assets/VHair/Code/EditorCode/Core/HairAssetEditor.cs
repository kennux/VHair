using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace VHair.Editor
{
    [CustomEditor(typeof(HairAsset))]
    public class HairAssetEditor : UnityEditor.Editor
    {
        private IHairAssetImporter currentImporter;
		private IHairAssetProcessor currentProcessor;

        public override void OnInspectorGUI()
        {
            var target = (this.target as HairAsset);

            // Render importer
            var importers = HairImport.GetImporters();
            int currentSelected = importers.IndexOf(this.currentImporter);
            int newSelected = EditorGUILayout.Popup("Importer: ", currentSelected == -1 ? 0 : currentSelected, importers.Select((importer) => importer.displayName).ToArray());

            if (currentSelected != newSelected)
            {
                this.currentImporter = importers[newSelected];
            }

            // Importer set?
            if (!ReferenceEquals(this.currentImporter, null))
            {
                EditorGUI.BeginDisabledGroup(!this.currentImporter.OnInspectorGUI(target));

                if (GUILayout.Button("Import"))
                {
                    // Temporary variables
                    Vector3[] vertices;
                    HairStrand[] strands;

                    // Import
                    this.currentImporter.Import(out vertices, out strands);
                    target.InitializeVertices(vertices);
                    target.InitializeStrands(strands);
                    target.InitializeMovability();
                    target.wasImported = true;

                    // Mark dirty
                    EditorUtility.SetDirty(target);
                }

                EditorGUI.EndDisabledGroup();
            }

            // Render actual inspector
            if (target.wasImported)
            {
                EditorGUILayout.LabelField("Strand count: " + target.strandCount);
                EditorGUILayout.LabelField("Vertex count: " + target.vertexCount);

				// Render processor
				var processors = HairAssetProcessors.GetProcessors();
				int currentSelectedProcessor = processors.IndexOf(currentProcessor);
				
				int newSelectedProcessor = EditorGUILayout.Popup(new GUIContent("Processor: ", "The processor to run on this asset"), currentSelectedProcessor == -1 ? 0 : currentSelectedProcessor, processors.Select((p) => new GUIContent(p.Name, p.Description)).ToArray());

				if (currentSelectedProcessor != newSelectedProcessor)
				{
					currentProcessor = processors[newSelectedProcessor];
				}

				if (currentProcessor != null)
				{
					currentProcessor.OnGUI();

					if (GUILayout.Button("Run"))
					{
						currentProcessor.Run(target);
						EditorUtility.SetDirty(target);
					}
				}
            }
        }
    }
}