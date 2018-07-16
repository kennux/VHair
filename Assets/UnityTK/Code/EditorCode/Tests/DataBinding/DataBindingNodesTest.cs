using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace UnityTK.DataBinding.Editor.Test
{
    public class DataBindingNodesTest
    {
        /// <summary>
        /// Creates a new data binding branch on a new gameobject parented to node's gameobject.
        /// Also initializes the branch bind target with the node (so the branch will bind to the node).
        /// </summary>
        public static DataBindingBranch CreateBranch(DataBindingNode node, string field)
        {
            var branchGo = new GameObject("Branch");
            branchGo.transform.parent = node.transform;
            var branch = branchGo.AddComponent<DataBindingBranch>();
            branch.parentNode = node;
            branch.field = field;
            branch.Awake();

            return branch;
        }
        
        [Test]
        public void DataBindingBranchTest()
        {
            // Create root
            DataBindingTestExample example;
            var root = DataBindingRootsTest.CreateRootWithTest(out example);

            // Create branch
            var branch = CreateBranch(root, "nest");

            Assert.AreEqual(example.nest, branch.boundObject);
            Assert.AreEqual(typeof(DataBindingTestExample.Nest), branch.boundType);
        }
    }
}