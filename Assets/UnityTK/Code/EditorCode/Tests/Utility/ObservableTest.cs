using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections.Generic;

namespace UnityTK.Test
{
    public class ObservableTest
    {
        [Test]
        public void ObservablePropertyTest()
        {
            // Arrange
            ObservableProperty<float> prop = new ObservableProperty<float>();
            float wasCalled = 0;
            prop.onChanged += (v) => { wasCalled = v; };

            // Act
            prop.value = 123;

            //Assert
            Assert.IsTrue(wasCalled != 0);
            Assert.AreEqual(123, prop.value);
        }

        [Test]
        public void ObservableListTest()
        {
            ObservableList<float> list = new ObservableList<float>(new List<float>());
            float lastAddCall = 0, lastRemoveCall = 0;
            bool wasClearCalled = false;
            list.onAdd += (v) => { lastAddCall = v; };
            list.onRemove += (v) => { lastRemoveCall= v; };
            list.onClear += () => { wasClearCalled = true; };

            list.Add(123);
            Assert.AreEqual(123, lastAddCall);
            lastAddCall = 0;

            list.Insert(0, 1337);
            Assert.AreEqual(1337, lastAddCall);
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(0, list.IndexOf(1337));
            lastAddCall = 0;

            list.Remove(1337);
            Assert.AreEqual(1337, lastRemoveCall);
            lastRemoveCall = 0;

            list.RemoveAt(0);
            Assert.AreEqual(123, lastRemoveCall);
            lastRemoveCall = 0;

            list.Add(-1);
            list.Clear();
            Assert.AreEqual(-1, lastRemoveCall);
            Assert.IsTrue(wasClearCalled);
            lastRemoveCall = 0;
            wasClearCalled = false;
        }
    }
}