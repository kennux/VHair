using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

namespace UnityTK.Benchmarking.Editor
{
    /// <summary>
    /// In-editor window for executing <see cref="Benchmark"/> benchmarks.
    /// </summary>
    public class BenchmarkingWindow : EditorWindow
    {
        public class BenchmarkEntry
        {
            public bool isSelected;
            public BenchmarkResult result;
            public MonoScript script;

            public BenchmarkEntry(MonoScript script)
            {
                this.script = script;
                this.result = null;
                this.isSelected = false;
            }
        }

        private const string gameObjectName = "_UnityTK_Benchmark_";

        [MenuItem("Window/UnityTK/Benchmarking")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            BenchmarkingWindow window = (BenchmarkingWindow)EditorWindow.GetWindow(typeof(BenchmarkingWindow));
            window.Show();
        }

        private List<BenchmarkEntry> benchmarks;

        protected void OnEnable()
        {
            this.titleContent = new GUIContent("Benchmarking");

            // Setup window data
            // This setup is split in seperate steps to make debugging easier
            string[] assets = AssetDatabase.FindAssets("t:MonoScript");
            List<MonoScript> scripts = assets.Select((guid) => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(MonoScript))).Cast<MonoScript>().ToList();
            this.benchmarks = scripts.Where((script) =>
            {
                var type = script.GetClass();
                if (ReferenceEquals(type, null))
                    return false;
                return !type.IsAbstract && typeof(Benchmark).IsAssignableFrom(type);
            }).Select((script) => new BenchmarkEntry(script)).ToList();
        }

        protected void Run(IEnumerable<BenchmarkEntry> benchmarks)
        {
            // Load temporary scene
            // TODO

            // Create GO for all benchmarks
            GameObject benchmarksGo = new GameObject(gameObjectName);

            try
            {
                // Create benchmarks
                Dictionary<BenchmarkEntry, Benchmark> instantiated = new Dictionary<BenchmarkEntry, Benchmark>();
                foreach (var benchmark in benchmarks)
                {
                    instantiated.Add(benchmark, benchmarksGo.AddComponent(benchmark.script.GetClass()) as Benchmark);
                }

                // Run benchmarks
                foreach (var kvp in instantiated)
                {
                    kvp.Key.result = kvp.Value.Run();
                }
            }
            finally
            {
                DestroyImmediate(benchmarksGo);
            }

            // Reload old scene setup
            // TODO
        }

        protected void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label("Benchmarking", EditorStyles.boldLabel);

            // Render out all benchmarks
            foreach (var benchmark in this.benchmarks)
            {
                benchmark.isSelected = GUILayout.Toggle(benchmark.isSelected, benchmark.script.GetClass().Name);
            }

            // Actions
            if (GUILayout.Button("Reload"))
            {
                this.OnEnable();
            }
            if (GUILayout.Button("Run selected"))
            {
                Run(this.benchmarks.Where((b) => b.isSelected));
            }
            if (GUILayout.Button("Run all"))
            {
                Run(this.benchmarks);
            }

            GUILayout.EndVertical();
            GUILayout.BeginVertical();

            StringBuilder sb = new StringBuilder();
            foreach (var benchmark in this.benchmarks.Where((b) => b.isSelected && !ReferenceEquals(b.result, null)))
            {
                sb.AppendLine("Benchmark " + benchmark.script.name);
                sb.AppendLine("-------------------------------------------------");
                sb.AppendLine(benchmark.result.GetReportString());

                sb.AppendLine();
                sb.AppendLine();
            }
            GUILayout.TextArea(sb.ToString(), GUILayout.ExpandWidth(true));

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }
}