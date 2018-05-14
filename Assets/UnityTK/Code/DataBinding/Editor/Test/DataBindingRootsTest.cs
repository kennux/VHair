using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace UnityTK.DataBinding.Editor.Test
{
    public class DataBindingRootsTest
    {
        /// <summary>
        /// Creates a databinding root binding to an instance of <see cref="DataBindingTest"/>.
        /// </summary>
        public static DataBindingRoot CreateRootWithTest(out DataBindingTestExample testBindTarget)
        {
            var rootGo = new GameObject("Root");
            testBindTarget = rootGo.AddComponent<DataBindingTestExample>();
            var root = rootGo.AddComponent<DataBindingRoot>();
            root.target = testBindTarget;
            root.Awake();

            return root;
        }

        [Test]
        public void DataBindingRootTest()
        {
            // Create root
            DataBindingTestExample example;
            var root = CreateRootWithTest(out example);

            Assert.AreEqual(example, root.boundObject);
            Assert.AreEqual(typeof(DataBindingTestExample), root.boundType);
        }
    }
}