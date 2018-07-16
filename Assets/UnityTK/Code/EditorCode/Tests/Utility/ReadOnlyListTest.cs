using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections.Generic;

namespace UnityTK.Test
{
    public class ReadOnlyListTest
    {
        [Test]
        public void _ReadOnlyListTest()
        {
            List<int> list = new List<int>();
            list.Add(123);
            list.Add(321);
            list.Add(1);

            ReadOnlyList<int> readOnlyList = new ReadOnlyList<int>(list);
            Assert.AreEqual(list[0], readOnlyList[0]);
            Assert.AreEqual(list[1], readOnlyList[1]);
            Assert.AreEqual(list[2], readOnlyList[2]);
            CollectionAssert.AreEqual(list, readOnlyList);

            Assert.AreEqual(list.Count, readOnlyList.count);
        }
    }
}