using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace UnityTK.Test
{
    public class LinkedListTest
    {
        [Test]
        public void TestUTKLinkedList()
        {
            UTKLinkedList<int> lst = new UTKLinkedList<int>();

            lst.Add(123);
            lst.Add(456);
            lst.Add(789);

            // Test removal
            CollectionAssert.AreEqual(new int[] { 123, 456, 789 }, lst);
            Assert.AreEqual(123, lst.first.value);

            // Remove middle
            lst.Remove(456);
            CollectionAssert.AreEqual(new int[] { 123, 789 }, lst);
            Assert.AreEqual(123, lst.first.value);
            Assert.AreEqual(789, lst.last.value);

            // Remove first
            lst.Remove(123);
            CollectionAssert.AreEqual(new int[] { 789 }, lst);
            Assert.AreEqual(789, lst.last.value);
            Assert.AreEqual(789, lst.first.value);

            // Rebuild list
            lst.Clear();
            lst.AddFirst(456);
            lst.AddFirst(123);
            lst.AddLast(789);
            CollectionAssert.AreEqual(new int[] { 123, 456, 789 }, lst);
            Assert.AreEqual(123, lst.first.value);
            Assert.AreEqual(789, lst.last.value);

            // Remove last
            lst.Remove(789);
            Assert.AreEqual(123, lst.first.value);
            Assert.AreEqual(456, lst.last.value);

            // Rebuild list
            lst.Clear();
            lst.AddFirst(456);
            lst.AddFirst(123);
            lst.AddLast(789);
            CollectionAssert.AreEqual(new int[] { 123, 456, 789 }, lst);
            Assert.AreEqual(123, lst.first.value);
            Assert.AreEqual(789, lst.last.value);

            // Insert before
            lst.Insert(lst.first.next, 1337, true);
            CollectionAssert.AreEqual(new int[] { 123, 1337, 456, 789 }, lst);
            Assert.AreEqual(123, lst.first.value);

            // Insert after
            lst.Remove(1337);
            lst.Insert(lst.first.next, 1337, false);
            CollectionAssert.AreEqual(new int[] { 123, 456, 1337, 789 }, lst);
            Assert.AreEqual(123, lst.first.value);

            // Insert end
            lst.Insert(lst.last, 1337, false);
            CollectionAssert.AreEqual(new int[] { 123, 456, 1337, 789, 1337 }, lst);
            Assert.AreEqual(1337, lst.last.value);

            // Insert first
            lst.Insert(lst.first, 1337, true);
            CollectionAssert.AreEqual(new int[] { 1337, 123, 456, 1337, 789, 1337 }, lst);
            Assert.AreEqual(1337, lst.first.value);
        }
    }
}