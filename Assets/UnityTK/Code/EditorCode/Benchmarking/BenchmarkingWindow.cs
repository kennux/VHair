using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using UnityEditor.IMGUI.Controls;

namespace UnityTK.Benchmarking.Editor
{
    /// <summary>
    /// In-editor window for executing <see cref="Benchmark"/> benchmarks.
    /// </summary>
    public class BenchmarkingWindow : EditorWindow
    {
        ///<summary>
        /// SerializeField is used to ensure the view state is written to the window 
        /// layout file. This means that the state survives restarting Unity as long as the window
        /// is not closed. If the attribute is omitted then the state is still serialized/deserialized.
        /// </summary>
        [SerializeField] TreeViewState treeViewState;

        [MenuItem("Window/UnityTK/Benchmarking")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            BenchmarkingWindow window = (BenchmarkingWindow)EditorWindow.GetWindow(typeof(BenchmarkingWindow));
            window.Show();
        }

        private BenchmarkingTreeView treeView;

        protected void OnEnable()
        {
            this.titleContent = new GUIContent("Benchmarking");

            if (treeViewState == null)
                treeViewState = new TreeViewState();

            treeView = new BenchmarkingTreeView(treeViewState);
        }

        protected void OnGUI()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Run selected"))
            {
                Benchmark(this.treeView.GetSelectedItems());
                this.treeView.Reload();
            }
            if (GUILayout.Button("Run all"))
            {
                Benchmark(this.treeView.GetAllItems());
                this.treeView.Reload();
            }
            if (GUILayout.Button("Reload"))
            {
                this.treeViewState = new TreeViewState();
                this.treeView = new BenchmarkingTreeView(this.treeViewState);
            }
            GUILayout.EndHorizontal();

            // Draw tree view
            treeView.OnGUI(new Rect(0, 30, position.width, position.height));
        }

        private void Benchmark(List<BenchmarkingTreeView.BenchmarkItem> items)
        {
            List<System.Type> scriptTypes = new List<System.Type>();
            List<BenchmarkResult> results = new List<BenchmarkResult>();

            foreach (var item in items)
            {
                scriptTypes.Add(item.script.GetClass());
            }

            System.GC.Collect();
            BenchmarkRunner.RunBenchmark(scriptTypes, results);
            System.GC.Collect();

            for (int i = 0; i < items.Count; i++)
            {
                items[i].result = results[i];
            }
        }
    }
}