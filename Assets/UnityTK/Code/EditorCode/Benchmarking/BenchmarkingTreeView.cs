using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using UnityEditor.IMGUI.Controls;

namespace UnityTK.Benchmarking.Editor
{
    class BenchmarkingTreeView : TreeView
    {
        /// <summary>
        /// Internal tree view item class.
        /// This is simply a class deriving from unity's TreeViewItem and adding additional fields for column data.
        /// </summary>
        class MyTreeViewItem : TreeViewItem
        {
            /// <summary>
            /// The string to be displayed in the "time" column.
            /// </summary>
            public string time;

            public MyTreeViewItem(int id, int depth, string displayName, List<TreeViewItem> children, string time, TreeViewItem parent) : base(id, depth, displayName)
            {
                this.time = time;
                this.children = children;
                this.parent = parent;
            }
        }

        /// <summary>
        /// Columns enumeration for the tree view.
        /// <see cref="RowGUI(RowGUIArgs)"/>
        /// </summary>
        enum Columns
        {
            CONTROLS,
            DISPLAY_NAME,
            TIME
        }

        /// <summary>
        /// Internal data structure to keep information about a benchmark.
        /// </summary>
        public class BenchmarkItem
        {
            /// <summary>
            /// The script of the benchmark-
            /// </summary>
            public MonoScript script;

            /// <summary>
            /// The result, may be null if the benchmark wasnt executed yet!
            /// </summary>
            public BenchmarkResult result;

            /// <summary>
            /// Creates a new benchmark item with null <see cref="result"/>
            /// </summary>
            /// <param name="script">The benchmark script.</param>
            public BenchmarkItem(MonoScript script)
            {
                this.script = script;
                this.result = null;
            }
        }

        /// <summary>
        /// Readonly view on <see cref="_benchmarks"/>
        /// </summary>
        public ReadOnlyList<BenchmarkItem> benchmarks
        {
            get { return new ReadOnlyList<BenchmarkItem>(this._benchmarks); }
        }

        /// <summary>
        /// All benchmark item objects, this list is always in sync with the current displayed tree view data.
        /// </summary>
        private List<BenchmarkItem> _benchmarks;

        /// <summary>
        /// Maps <see cref="TreeViewItem.id"/> to indices in <see cref="_benchmarks"/>.
        /// Rebuilt in <see cref="BuildRoot"/>
        /// </summary>
        private Dictionary<int, int> treeViewItemIdBenchmarkMappings = new Dictionary<int, int>();

        /// <summary>
        /// The state for the coulmn header.
        /// </summary>
        private MultiColumnHeaderState mchState;

        /// <summary>
        /// Creates a new benchmarking tree view.
        /// <see cref="TreeView"/>
        /// </summary>
        /// <param name="state">The state of the tree view.</param>
        public BenchmarkingTreeView(TreeViewState state) : base(state)
        {
            ReflectionReload();

            this.mchState = new MultiColumnHeaderState(new MultiColumnHeaderState.Column[]
                {
                    new MultiColumnHeaderState.Column()
                    {
                        headerContent = new GUIContent(""),
                        minWidth = 100,
                        canSort = false
                    },
                    new MultiColumnHeaderState.Column()
                    {
                        headerContent = new GUIContent("Path"),
                        canSort = false,
                        width = 500
                    },
                    new MultiColumnHeaderState.Column()
                    {
                        headerContent = new GUIContent("Time ms"),
                        canSort = false,
                        width = 150
                    }
                });
            this.multiColumnHeader = new MultiColumnHeader(this.mchState);
        }

        /// <summary>
        /// Returns a new list filled with content from <see cref="_benchmarks"/>
        /// </summary>
        public List<BenchmarkItem> GetAllItems()
        {
            return new List<BenchmarkItem>(this._benchmarks);
        }

        /// <summary>
        /// Gets all benchmark items from <see cref="_benchmarks"/> which are currently selected.
        /// </summary>
        public List<BenchmarkItem> GetSelectedItems()
        {
            List<BenchmarkItem> items = new List<BenchmarkItem>();

            foreach (var item in this.GetSelection())
            {
                // Read index mapping
                int idx;
                if (this.treeViewItemIdBenchmarkMappings.TryGetValue(item, out idx))
                {
                    // Add item to list
                    items.Add(this._benchmarks[idx]);
                }
            }

            return items;
        }

        /// <summary>
        /// Reloads all reflected data, essentially fully rebuilding benchmarking information.
        /// This will implicitly call <see cref="TreeView.Reload"/>
        /// </summary>
        public void ReflectionReload()
        {
            // Setup data
            // This setup is split in seperate steps to make debugging easier
            string[] assets = AssetDatabase.FindAssets("t:MonoScript");
            List<MonoScript> scripts = assets.Select((guid) => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(MonoScript))).Cast<MonoScript>().ToList();

            // Read benchmarks
            this._benchmarks = scripts.Where((script) =>
            {
                var type = script.GetClass();
                if (ReferenceEquals(type, null))
                    return false;
                return !type.IsAbstract && typeof(Benchmark).IsAssignableFrom(type);
            }).Select((script) => new BenchmarkItem(script)).ToList();

            // Do the treeview reload to rebuild its ui
            Reload();
        }

        /// <summary>
        /// Builds tree view items recursively from the specified benchmark results.
        /// </summary>
        /// <param name="labels">The benchmark result labels, retrieved from <see cref="BenchmarkResult.GetLabels(List{BenchmarkResultLabel})"/></param>
        /// <param name="items">The list where the created tree view items (of type <see cref="MyTreeViewItem"/>) will be added to.</param>
        /// <param name="id">Reference to the current tree view item index counter. The counter will be incremented for every item added to items.</param>
        /// <param name="depth">The start depth of the tree view items. This will be incremented every time this method is called recursively.</param>
        private void BuildTreeViewItems(List<BenchmarkResultLabel> labels, List<TreeViewItem> items, ref int id, TreeViewItem parent, int depth = 1)
        {
            for (int j = 0; j < labels.Count; j++)
            {
                var children = new List<TreeViewItem>();

                // Construct label and increment id counter
                var label = labels[j];
                var item = new MyTreeViewItem(id, depth, label.label, children, label.time.ToString("0.00"), parent);
                items.Add(item);
                id++;

                // Construct children if there are children
                if (label.children.Count > 0)
                {
                    // Recursion
                    BuildTreeViewItems(label.children, children, ref id, item, depth + 1);
                }
            }
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (MyTreeViewItem)args.item;

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (Columns)args.GetColumn(i));
            }
        }

        /// <summary>
        /// Helper method for drawing cell gui.
        /// </summary>
        /// <param name="cellRect">The rect in which the cell content will be drawn.</param>
        /// <param name="item">The item to be drawn.</param>
        /// <param name="column">The coulmn to be drawn.</param>
        void CellGUI(Rect cellRect, MyTreeViewItem item, Columns column)
        {
            switch (column)
            {
                case Columns.CONTROLS:
                    
                    break;
                case Columns.DISPLAY_NAME:
                    cellRect.xMin += Mathf.Clamp(item.depth * depthIndentWidth, 0, float.MaxValue);
                    GUI.Label(cellRect, item.displayName);
                    break;
                case Columns.TIME:
                    GUI.Label(cellRect, item.time);
                    break;
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            treeViewItemIdBenchmarkMappings.Clear();

            // Build tree view
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            int id = 1;
            List<TreeViewItem> rootChildren = new List<TreeViewItem>();
            Dictionary<TreeViewItem, List<TreeViewItem>> items = new Dictionary<TreeViewItem, List<TreeViewItem>>();

            for (int i = 0; i < _benchmarks.Count; i++)
            {
                // Build mapping and increment id counter
                var benchmark = benchmarks[i];
                treeViewItemIdBenchmarkMappings.Add(id, i);

                // Build item
                var children = new List<TreeViewItem>();
                var item = new MyTreeViewItem(id++, 0, benchmark.script.name, children, "", root);

                // Build children
                if (!ReferenceEquals(benchmark.result, null))
                {
                    var labels = benchmark.result.GetLabels();
                    BuildTreeViewItems(labels, children, ref id, item);
                }

                items.Add(item, children);
                rootChildren.Add(item);
            }

            root.children = rootChildren;
            // Set parents and children
            // SetupParentsAndChildrenFromDepths(root, items.Keys.ToList());
            /*foreach (var kvp in items)
                SetupParentsAndChildrenFromDepths(kvp.Key, kvp.Value);*/

            return root;
        }
    }
}
