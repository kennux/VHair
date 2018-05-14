using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace UnityTK.Test
{
    public class ReusableBoxTest
    {
        [Test]
        public void ReusableBoxTests()
        {
            ReusableBox<int> int1 = ReusableBox<int>.Box(123);
            ReusableBox<int> int2 = ReusableBox<int>.Box(123);
            ReusableBox<int> int3 = ReusableBox<int>.Box(321);

            // Equality
            Assert.AreEqual(123, (int)int1);
            Assert.AreEqual(321, (int)int3);
            Assert.AreEqual(int1, int2);
            Assert.AreNotEqual(int1, int3);
            
            // Hash codes
            Assert.AreEqual(int1.GetHashCode(), int2.GetHashCode());
            Assert.AreNotEqual(int1.GetHashCode(), int3.GetHashCode());

            // ToString
            Assert.AreEqual(int1.ToString(), int2.ToString());
            Assert.AreNotEqual(int1.ToString(), int3.ToString());

            // Pooling mechanic
            int2.Dispose();
            ReusableBox<int> int4 = ReusableBox<int>.Box(1337);

            Assert.AreSame(int2, int4);
            Assert.AreEqual(1337, int4.value);
        }
    }
}