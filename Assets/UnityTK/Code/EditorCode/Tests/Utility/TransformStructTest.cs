using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace UnityTK.Test.Utility
{
    public class TransformStructTest
    {
        [Test]
        public void TransformStructTestSimplePasses()
        {
            Transform t = new GameObject("lul").transform;
            Transform t2 = new GameObject("lul2").transform;
            t.localPosition = -Vector3.one;
            t.localRotation = Quaternion.Euler(new Vector3(0, 45, 0));
            t2.parent = t;
            t2.localPosition = Vector3.one;
            t2.localRotation = Quaternion.Euler(new Vector3(45, 0, 0));

            // Test construction
            TransformStruct st = new TransformStruct(t);
            TransformStruct st2 = new TransformStruct(t2);

            Assert.AreEqual(st.localPosition, t.localPosition);
            Assert.AreEqual(st2.localPosition, t2.localPosition);
            Assert.AreEqual(st.localRotation, t.localRotation);
            Assert.AreEqual(st2.localRotation, t2.localRotation);
            Assert.AreEqual(st2.parent, t2.parent);

            // Test modification
            st2.localEulerAngles = new Vector3(90, 0, 0);
            t2.localEulerAngles = new Vector3(90, 0, 0);
            t2.localPosition = Vector3.one * 2;
            st2.localPosition = Vector3.one * 2;
            
            Assert.AreEqual(st2.localPosition, t2.localPosition);
            Assert.IsTrue(Mathf.Approximately(st2.localRotation.x, t2.localRotation.x));
            Assert.IsTrue(Mathf.Approximately(st2.localRotation.y, t2.localRotation.y));
            Assert.IsTrue(Mathf.Approximately(st2.localRotation.z, t2.localRotation.z));
            Assert.IsTrue(Mathf.Approximately(st2.localRotation.w, t2.localRotation.w));

            // Test application
            t2.localEulerAngles = new Vector3(0, 0, 0);
            t2.localPosition = Vector3.zero;
            st2.Apply(t2);

            Assert.AreEqual(st2.localPosition, t2.localPosition);
            Assert.IsTrue(Mathf.Approximately(st2.localRotation.x, t2.localRotation.x));
            Assert.IsTrue(Mathf.Approximately(st2.localRotation.y, t2.localRotation.y));
            Assert.IsTrue(Mathf.Approximately(st2.localRotation.z, t2.localRotation.z));
            Assert.IsTrue(Mathf.Approximately(st2.localRotation.w, t2.localRotation.w));
        }
    }
}