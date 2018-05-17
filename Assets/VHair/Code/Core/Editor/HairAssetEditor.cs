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

                // TODO: Implement post processing modules
                if (GUILayout.Button("Set standard Movability"))
                {
                    uint[] movability = HairMovability.CreateData(target.vertexCount);
                    HairStrand[] strands;
                    target.GetStrandData(out strands);

                    for (int i = 0; i < strands.Length; i++)
                    {
                        HairStrand strand = strands[i];
                        HairMovability.SetMovable(strand.firstVertex, false, movability);
                        int cnt = (strand.lastVertex - strand.firstVertex)+1;
                        for (int j = 1; j < cnt; j++)
                        {
                            HairMovability.SetMovable(strand.firstVertex + j, true, movability);
                        }
                    }

                    target.InitializeMovability(movability);
                }

                if (GUILayout.Button("Halven strand count"))
                {
                    for (int i = 0; i < target.strandCount; i+=2)
                    {
                    }
                }
            }
        }
    }
}