using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;

namespace UnityTK.DataBinding.Editor.Test
{
    public class DataBindingLeavesTest
    {
        public static DataBindingGenericTemplate uiTextGenericTemplate
        {
            get
            {
                if (Essentials.UnityIsNull(_uiTextGenericTemplate))
                {
                    _uiTextGenericTemplate = ScriptableObject.CreateInstance<DataBindingGenericTemplate>();
                    _uiTextGenericTemplate.SetTargetType(typeof(UnityEngine.UI.Text));
                    _uiTextGenericTemplate.SetTargetField(typeof(UnityEngine.UI.Text).GetProperty("text"));
                }

                return _uiTextGenericTemplate;
            }
        }
        private static DataBindingGenericTemplate _uiTextGenericTemplate;

        public static DataBindingCollectionLeaf CreateCollectionLeaf(DataBindingNode node, string field, DataBindingCollectionElement prefab)
        {
            var leafGo = new GameObject("CollectionLeaf");
            leafGo.SetActive(false);

            leafGo.transform.parent = node.transform;

            var leaf = leafGo.AddComponent<DataBindingCollectionLeaf>();
            leaf.parentNode = node;
            leaf.field = field;
            leaf.elementPrefab = prefab;
            leaf.Awake();

            return leaf;
        }

        public static DataBindingCollectionElement CreateCollectionElementPrefab()
        {
            var go = new GameObject("CollectionPrefab");
            go.SetActive(false);

            var element = go.AddComponent<DataBindingCollectionElement>();
            element.SetElementType(typeof(DataBindingTestExample.Nest));
            
            Text testStrText;
            CreateUITextGenericTemplatedLeaf(element, "testStr", out testStrText);

            return element;
        }

        public static DataBindingGenericTemplatedLeaf CreateUITextGenericTemplatedLeaf(DataBindingNode node, string field, out Text text)
        {
            var leafGo = new GameObject("UITextGenericTemplatedLeaf");
            leafGo.transform.parent = node.transform;
            var leaf = leafGo.AddComponent<DataBindingGenericTemplatedLeaf>();
            leaf.parentNode = node;
            leaf.field = field;
            leaf.template = uiTextGenericTemplate;
            leaf.bindTarget = text = leafGo.AddComponent<Text>();
            leaf.Awake();

            return leaf;
        }

        public static DataBindingInvoker CreateInvoker(DataBindingNode node, MethodInfo method)
        {
            var invokerGo = new GameObject("Invoker");
            invokerGo.transform.parent = node.transform;
            var invoker = invokerGo.AddComponent<DataBindingInvoker>();
            invoker.parentNode = node;
            invoker.SetMethod(method);
            invoker.Awake();

            return invoker;
        }

        [Test]
        public void DataBindingInvokerTest()
        {
            // Create root
            DataBindingTestExample example;
            var root = DataBindingRootsTest.CreateRootWithTest(out example);
            example.testStr = "Teststring";

            // Create invoker
            var parameterizedInvoker = CreateInvoker(root, typeof(DataBindingTestExample).GetMethod("Test3"));
            var parameterLessInvoker = CreateInvoker(root, typeof(DataBindingTestExample).GetMethod("UnitTest"));
            var returningInvoker = CreateInvoker(root, typeof(DataBindingTestExample).GetMethod("UnitTest2"));
            parameterizedInvoker.SetParameterBinding(0, root, "testStr");

            // Expect debug logging from DataBindingExample.Test3
            LogAssert.Expect(LogType.Log, "Teststring");
            parameterizedInvoker.Invoke();

            // Test parameterless invocation, Test2 method will set testStr to a random float
            Assert.AreEqual("Teststring", example.testStr);
            parameterLessInvoker.Invoke();
            Assert.AreNotEqual("Teststring", example.testStr);
            example.testStr = "Teststring";

            // Test invoker binding to the return value
            Assert.AreEqual(typeof(string), returningInvoker.boundType);
            Assert.AreEqual("Teststring", returningInvoker.boundObject);
        }

        [Test]
        public void DataBindingCollectionLeafTest()
        {
            // Create root
            DataBindingTestExample example;
            var root = DataBindingRootsTest.CreateRootWithTest(out example);

            // Create collection leaf
            var collectionLeaf = CreateCollectionLeaf(root, "nestArray", CreateCollectionElementPrefab());
            Assert.AreEqual(typeof(List<DataBindingTestExample.Nest>), collectionLeaf.boundType);
            Assert.AreEqual(typeof(DataBindingTestExample.Nest), collectionLeaf.elementPrefab.boundType);

            // Set up test data
            example.nestArray.Clear();
            example.nestArray.Add(new DataBindingTestExample.Nest()
            {
                testNumber = 13371,
                testStr = "lol"
            });
            example.nestArray.Add(new DataBindingTestExample.Nest()
            {
                testNumber = 13372,
                testStr = "lol2"
            });

            // Update collection leaf
            collectionLeaf.UpdateBinding();

            // Initialize child leaves
            foreach (var element in collectionLeaf.GetComponentsInChildren<DataBindingCollectionElement>())
            {
                // Awake leaves
                foreach (var leaf in element.GetComponentsInChildren<DataBinding>())
                    leaf.Awake();

                // Update element
                element.UpdateBinding();
            }

            var elements = collectionLeaf.GetComponentsInChildren<DataBindingCollectionElement>();
            Assert.AreEqual(2, elements.Length);
            Assert.AreEqual(example.nestArray[0], elements[0].boundObject);
            Assert.AreEqual(example.nestArray[1], elements[1].boundObject);
            Assert.AreEqual(example.nestArray[0].testStr, elements[0].GetComponentInChildren<Text>().text);
            Assert.AreEqual(example.nestArray[1].testStr, elements[1].GetComponentInChildren<Text>().text);
        }

        [Test]
        public void DataBindingGenericTemplatedLeafTest()
        {
            // Create root
            DataBindingTestExample example;
            var root = DataBindingRootsTest.CreateRootWithTest(out example);

            // Create branch
            var branch = DataBindingNodesTest.CreateBranch(root, "nest");

            // Create leaf on branch
            example.nest.testNumber = 1337;
            Text text;
            var leafOnBranch = CreateUITextGenericTemplatedLeaf(branch, "testNumber", out text);

            // Type and bound object check
            Assert.AreEqual("1337", leafOnBranch.boundObject);
            Assert.AreEqual(typeof(object), leafOnBranch.boundType); // Strings are being handled as objects in the leaves.

            // Update the binding
            leafOnBranch.UpdateBinding();

            // Check if leaf wrote text property
            Assert.AreEqual("1337", text.text);

            // Create leaf on root
            example.testStr = "test";
            var leafOnRoot = CreateUITextGenericTemplatedLeaf(root, "testStr", out text);

            // Type and bound object check
            Assert.AreEqual("test", leafOnRoot.boundObject);
            Assert.AreEqual(typeof(object), leafOnRoot.boundType); // Strings are being handled as objects in the leaves.

            // Update the binding
            leafOnRoot.UpdateBinding();

            // Check if leaf wrote text property
            Assert.AreEqual("test", text.text);
        }
    }
}