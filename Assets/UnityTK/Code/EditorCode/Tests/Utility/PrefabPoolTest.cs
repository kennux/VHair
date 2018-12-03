using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace UnityTK.Test.Utility
{
    public class PrefabPoolTest
    {
        [Test]
        public void TestPrefabPool()
        {
            // Prepare
            GameObject pool = new GameObject();
            GameObject prefab = new GameObject();
            prefab.AddComponent<MeshFilter>();
            var p = pool.AddComponent<PrefabPool>();
            p.Awake();

            // Test singleton
            Assert.IsFalse(Essentials.UnityIsNull(PrefabPool.instance));

            // Test getting
            var instance1 = PrefabPool.instance.GetInstance(prefab);
            var instance2 = PrefabPool.instance.GetInstance(prefab);

            // Assert
            Assert.AreNotSame(instance1, instance2);
            Assert.IsTrue(instance1.activeSelf);
            Assert.IsTrue(instance2.activeSelf);
            Assert.AreSame(instance1.transform.parent, null);
            Assert.AreSame(instance2.transform.parent, null);

            // Return
            PrefabPool.instance.Return(instance1);
            PrefabPool.instance.Return(instance2);

            // Assert
            Assert.IsFalse(instance1.activeSelf);
            Assert.IsFalse(instance2.activeSelf);
            Assert.AreSame(instance1.transform.parent, pool.transform);
            Assert.AreSame(instance2.transform.parent, pool.transform);

            // Draw again
            var quat = Quaternion.LookRotation(Vector3.up);
            var pos = Vector3.one;
            var instance3 = PrefabPool.instance.GetInstance(prefab, pos, quat);
            var instance4 = PrefabPool.instance.GetInstance(prefab);

            Assert.AreSame(instance3, instance2);
            Assert.AreSame(instance4, instance1);
            Assert.AreEqual(pos, instance3.transform.position);
            Assert.AreEqual(quat, instance3.transform.rotation);
        }
    }
}