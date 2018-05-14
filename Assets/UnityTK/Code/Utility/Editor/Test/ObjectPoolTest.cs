using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityTK;
using System;

namespace UnityTK.Test
{
    public class ObjectPoolTest
    {

        [Test]
        public void ObjectPoolTests()
        {
            ObjectPool<List<int>> pool = new ObjectPool<List<int>>(() => new List<int>(), 100, (lst) => lst.Clear(), (lst) => lst.Add(0));

            List<int> l = null;
            pool.GetIfNull(ref l);

            Assert.AreEqual(1, l.Count);
            Assert.AreEqual(0, l[0]);

            l.Add(1337);
            pool.Return(l);
            l = null;

            pool.GetIfNull(ref l);

            Assert.AreEqual(1, l.Count);
            Assert.AreEqual(0, l[0]);

            try
            {
                pool.Return(l);
                pool.Return(l, true);

                Assert.IsTrue(false);
            }
            catch (InvalidOperationException)
            {

            }

            for (int i = 0; i < 1000; i++)
            {
                pool.Return(new List<int>());
            }

            Assert.AreEqual(100, pool.count);

        }
    }
}