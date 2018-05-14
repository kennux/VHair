using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace UnityTK.Test
{
    public class TimerTest
    {
        private static Timer timer
        {
            get
            {
                if (Essentials.UnityIsNull(_timer))
                {
                    // Get timer singleton
                    _timer = Timer.instance;

                    // Init timer
                    _timer.Awake();
                }
                return _timer;
            }
        }
        private static Timer _timer;

        [Test]
        public void TimerHandleRecycleTest()
        {
            // Setup tests
            object state = new object();
            int updateCalls = 0, fixedUpdateCalls = 0;
            var handle = timer.InvokeIn(-1, (s) =>
            {
                // Test state passed being correct
                Assert.AreSame(state, s);

                fixedUpdateCalls++;
            }, TimerEvent.FIXED_UPDATE, state);
            timer.ReturnTimerHandle(handle);
            var handle2 = timer.InvokeIn(-1, (s) =>
            {
                // Test state passed being correct
                Assert.AreSame(state, s);

                updateCalls++;
            }, TimerEvent.UPDATE, state);

            Assert.AreSame(handle, handle2);
        }

        [Test]
        public void TimerInvokeInTest()
        {
            // Setup tests
            object state = new object();
            int updateCalls = 0, fixedUpdateCalls = 0, lateUpdateCalls = 0;
            timer.InvokeIn(-1, (s) =>
            {
                // Test state passed being correct
                Assert.AreSame(state, s);

                fixedUpdateCalls++;
            }, TimerEvent.FIXED_UPDATE, state);
            timer.InvokeIn(-1, (s) =>
            {
                // Test state passed being correct
                Assert.AreSame(state, s);

                updateCalls++;
            }, TimerEvent.UPDATE, state);
            timer.InvokeIn(-1, (s) =>
            {
                // Test state passed being correct
                Assert.AreSame(state, s);

                lateUpdateCalls++;
            }, TimerEvent.LATE_UPDATE, state);

            // Call events
            Assert.AreEqual(0, fixedUpdateCalls);
            timer.FixedUpdate();
            Assert.AreEqual(1, fixedUpdateCalls);

            Assert.AreEqual(0, updateCalls);
            timer.Update();
            Assert.AreEqual(1, updateCalls);

            Assert.AreEqual(0, lateUpdateCalls);
            timer.LateUpdate();
            Assert.AreEqual(1, lateUpdateCalls);
        }

        [Test]
        public void TimerInvokeInIntervalTest()
        {
            // Setup tests
            object state = new object();
            int updateCalls = 0, fixedUpdateCalls = 0, lateUpdateCalls = 0;
            timer.InvokeInInterval(-1, (s) =>
            {
                // Test state passed being correct
                Assert.AreSame(state, s);

                fixedUpdateCalls++;
            }, TimerEvent.FIXED_UPDATE, state);
            timer.InvokeInInterval(-1, (s) =>
            {
                // Test state passed being correct
                Assert.AreSame(state, s);

                updateCalls++;
            }, TimerEvent.UPDATE, state);
            timer.InvokeInInterval(-1, (s) =>
            {
                // Test state passed being correct
                Assert.AreSame(state, s);

                lateUpdateCalls++;
            }, TimerEvent.LATE_UPDATE, state);

            // Call events
            Assert.AreEqual(0, fixedUpdateCalls);
            timer.FixedUpdate();
            timer.FixedUpdate();
            Assert.AreEqual(2, fixedUpdateCalls);

            Assert.AreEqual(0, updateCalls);
            timer.Update();
            timer.Update();
            Assert.AreEqual(2, updateCalls);

            Assert.AreEqual(0, lateUpdateCalls);
            timer.LateUpdate();
            timer.LateUpdate();
            Assert.AreEqual(2, lateUpdateCalls);
        }
    }
}