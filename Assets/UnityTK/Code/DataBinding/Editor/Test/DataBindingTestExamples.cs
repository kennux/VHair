using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;

namespace UnityTK.DataBinding.Editor.Test
{
    public class DataBindingTestExample : MonoBehaviour
    {
        public float testNumber;
        public string testStr;
        public Nest nest = new Nest();

        public List<Nest> nestArray = new List<Nest>();

        [System.Serializable]
        public class Nest
        {
            public string testStr;
            public float testNumber;
        }

        public string Test()
        {
            this.testStr = Random.value.ToString();
            return this.testStr;
        }

        public string Test2()
        {
            return Random.value.ToString();
        }

        public string UnitTest2()
        {
            return this.testStr;
        }

        public void UnitTest()
        {
            this.testStr = Random.value.ToString();
        }

        public void Test3(string test)
        {
            Debug.Log(test);
        }
    }
}