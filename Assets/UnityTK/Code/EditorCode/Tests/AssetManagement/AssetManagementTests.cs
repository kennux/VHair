using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace UnityTK.AssetManagement.Editor.Test
{
    public class AssetManagementTests
    {
        public class AssetExample : ManagedScriptableObject
        {

        }

        public class AssetExample2 : ManagedScriptableObject
        {

        }

        private ManagedGameObject CreateManagedGameObject(string[] tags, string identifier, string name)
        {
            GameObject go = new GameObject("ManagedGO");
            var mgo = go.AddComponent<ManagedGameObject>();

            mgo.tags = tags;
            mgo.identifier = identifier;
            mgo.name = name;

            return mgo;
        }

        private T CreateManagedScriptableObject<T>(string[] tags, string identifier, string name) where T : ManagedScriptableObject
        {
            T ex = ScriptableObject.CreateInstance<T>();
            ex.tags = tags;
            ex.identifier = identifier;
            ex.name = name;

            return ex;
        }

        [Test]
        public void AssetManagementTestRegister()
        {
            var a1 = CreateManagedGameObject(new string[] { "test" }, "asset1", "1");
            var a2 = CreateManagedScriptableObject<AssetExample>(new string[] { "test2" }, "asset2", "2");

            AssetManager.instance.RegisterAsset(a1);
            AssetManager.instance.RegisterAsset(a2);

            // Try retrieving
            Assert.AreSame(a1.transform, AssetManager.instance.GetObject<Transform>("asset1"));
            Assert.AreSame(a2, AssetManager.instance.GetObject<AssetExample>("asset2"));

            // Exception throw test
            AssetManager.instance.GetObject<AudioSource>("asset1");
            try
            {
                AssetManager.instance.GetObject<AudioSource>("asset1", true);
                Assert.IsTrue(false);
            }
            catch (System.InvalidCastException)
            {

            }

            // Deregister
            AssetManager.instance.DeregisterAsset(a1);
            AssetManager.instance.DeregisterAsset(a2);

            Assert.AreSame(null, AssetManager.instance.GetObject<Transform>("asset1"));
        }

        [Test]
        public void AssetManagementTestOverriding()
        {
            var a1 = CreateManagedGameObject(new string[] { "test" }, "asset1", "1");
            var a2 = CreateManagedGameObject(new string[] { "test2" }, "asset1", "2");

            AssetManager.instance.RegisterAsset(a1);
            AssetManager.instance.RegisterAsset(a2);

            // Try retrieving
            Assert.AreSame(a2.transform, AssetManager.instance.GetObject<Transform>("asset1"));

            // Override again
            AssetManager.instance.RegisterAsset(a1);
            Assert.AreSame(a1.transform, AssetManager.instance.GetObject<Transform>("asset1"));

            // Deregister
            AssetManager.instance.DeregisterAsset(a1);
            AssetManager.instance.DeregisterAsset(a2);

            Assert.AreSame(null, AssetManager.instance.GetObject<Transform>("asset1"));
        }

        [Test]
        public void AssetManagementTestQuery()
        {
            var a1 = CreateManagedGameObject(new string[] { "test" }, "asset1", "1");
            var a2 = CreateManagedScriptableObject<AssetExample>(new string[] { "test2" }, "asset2", "2");
            var a3 = CreateManagedScriptableObject<AssetExample>(new string[] { "test3", "test4" }, "asset3", "3");
            var a4 = CreateManagedScriptableObject<AssetExample2>(new string[] { "test5", "test6" }, "asset4", "4");

            AssetManager.instance.RegisterAsset(a1);
            AssetManager.instance.RegisterAsset(a2);
            AssetManager.instance.RegisterAsset(a3);
            AssetManager.instance.RegisterAsset(a4);

            // Tags query
            var query = AssetManagerQueryPool<AssetManagerQuery>.Get();
            query.AddTagCriteria("test");

            var qRes = AssetManager.instance.Query<Transform>(query);
            Assert.AreSame(a1.transform, qRes[0]);
            Assert.AreEqual(1, qRes.Count);

            // Exception throw test
            var aRes = AssetManager.instance.Query<AudioSource>(query);
            Assert.AreEqual(0, aRes.Count);

            try
            {
                AssetManager.instance.Query<AudioSource>(query, aRes, -1, true);
                Assert.IsTrue(false);
            }
            catch (System.InvalidCastException)
            {

            }
            Assert.AreEqual(0, aRes.Count);
            AssetManagerQueryPool<AssetManagerQuery>.Return(query);

            // Name query
            query = AssetManagerQueryPool<AssetManagerQuery>.Get();
            query.AddNameCriteria("2");

            var eRes = AssetManager.instance.Query<AssetExample>(query);

            Assert.AreSame(eRes[0], a2);
            Assert.AreEqual(1, eRes.Count);
            AssetManagerQueryPool<AssetManagerQuery>.Return(query);

            // Multitag query
            query = AssetManagerQueryPool<AssetManagerQuery>.Get();
            query.AddTagCriteria("test3");
            query.AddTagCriteria("test4");

            var oRes = AssetManager.instance.Query<UnityEngine.Object>(query);

            Assert.AreSame(oRes[0], a3);
            Assert.AreEqual(1, oRes.Count);
            AssetManagerQueryPool<AssetManagerQuery>.Return(query);

            // Type query
            query = AssetManagerQuery.GetPooledTypeQuery(typeof(AssetExample2));
            var tRes = AssetManager.instance.Query<UnityEngine.Object>(query);

            Assert.AreEqual(1, tRes.Count);
            Assert.AreSame(tRes[0], a4);

            // Deregister
            AssetManager.instance.DeregisterAsset(a1);
            AssetManager.instance.DeregisterAsset(a2);
            AssetManager.instance.DeregisterAsset(a3);

            Assert.AreSame(null, AssetManager.instance.GetObject<Transform>("asset1"));
        }

    }
}