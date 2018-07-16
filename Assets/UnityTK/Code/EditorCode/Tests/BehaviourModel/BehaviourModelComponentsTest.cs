using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityTK.BehaviourModel.Editor.Test
{
    public class BehaviourModelComponentsTest
    {
        [Test]
        public void ModelFunctionTest()
        {
            ModelFunction<int> function = new ModelFunction<int>();
            function.BindHandler(() => 123);

            Assert.AreEqual(123, function.Invoke());

            LogAssert.Expect(LogType.Error, "Invoked ModelFunction with no function bound!");
            Assert.AreEqual(default(int), new ModelFunction<int>().Invoke());

            // Test generic versions
            _ModelFunctionT1Test();
            _ModelFunctionT1T2Test();
        }

        private void _ModelFunctionT1Test()
        {
            ModelFunction<int, int> function = new ModelFunction<int, int>();
            function.BindHandler((i) =>
            {
                Assert.AreEqual(1337, i);
                return 123;
            });

            Assert.AreEqual(123, function.Invoke(1337));

            LogAssert.Expect(LogType.Error, "Invoked ModelFunction with no function bound!");
            Assert.AreEqual(default(int), new ModelFunction<int, int>().Invoke(1));
        }

        private void _ModelFunctionT1T2Test()
        {
            ModelFunction<int, int, int> function = new ModelFunction<int, int, int>();
            function.BindHandler((i, j) =>
            {
                Assert.AreEqual(1337, i);
                Assert.AreEqual(1338, j);
                return 123;
            });

            Assert.AreEqual(123, function.Invoke(1337, 1338));

            LogAssert.Expect(LogType.Error, "Invoked ModelFunction with no function bound!");
            Assert.AreEqual(default(int), new ModelFunction<int, int, int>().Invoke(1,2));
        }

        [Test]
        public void ModelCollectionPropertyTest()
        {
            // Prepare
            List<int> l1 = new List<int>(), l2 = new List<int>();
            ModelCollectionProperty<int> collectionProperty = new ModelCollectionProperty<int>();

            // Register getters for both lists
            collectionProperty.RegisterGetter(() => l1);
            collectionProperty.RegisterGetter(() => l2);

            // Add an item on both lists
            l1.Add(321);
            l2.Add(123);

            // Assert sequence equality
            CollectionAssert.AreEqual(l1.Concat(l2), collectionProperty);

            // Clear lists
            l1.Clear();
            l2.Clear();

            // Set handlers
            collectionProperty.RegisterInsertHandler((i) =>
            {
                if (i != 123)
                    return false;

                l1.Add(i);
                return true;
            });
            collectionProperty.RegisterInsertHandler((i) =>
            {
                l2.Add(i);
                return true;
            });

            // Set data
            collectionProperty.Insert(123);
            collectionProperty.Insert(321);

            // Assert set data
            Assert.AreEqual(123, l1[0]);
            Assert.AreEqual(321, l2[0]);

            // Append removal handlers
            collectionProperty.RegisterRemovalHandler((i) =>
            {
                if (l1.Contains(i))
                {
                    l1.Remove(i);
                    return true;
                }
                else if (l2.Contains(i))
                {
                    l2.Remove(i);
                    return true;
                }

                return false;
            });

            Assert.IsFalse(collectionProperty.Remove(456));
            Assert.IsTrue(collectionProperty.Remove(123));
            Assert.AreEqual(0, l1.Count);

            // Test ICollection methods
            collectionProperty = new ModelCollectionProperty<int>();
            l1 = new List<int>();
            collectionProperty.RegisterGetter(() => l1);
            collectionProperty.RegisterInsertHandler((i) => { l1.Add(i); return true; });
            collectionProperty.RegisterRemovalHandler((i) => l1.Remove(i));

            ICollection<int> collection = collectionProperty;
            collection.Add(123);
            collection.Add(321);

            Assert.IsTrue(collection.Contains(321));
            Assert.AreEqual(2, collection.Count);
            Assert.IsFalse(collection.IsReadOnly);
            Assert.IsTrue(((ICollection<int>)new ModelCollectionProperty<int>()).IsReadOnly);

            int[] test = new int[2];
            collection.CopyTo(test, 0);
            CollectionAssert.AreEqual(collection, test);

            collection.Clear();
            Assert.AreEqual(0, collection.Count);
        }

        [Test]
        public void ModelActivityTest()
        {
            bool isActive = false, startCondition = false, stopCondition = false;
            bool onJustStart = false, onJustStop = false, onJustFailStart = false, onJustFailStop = false;

            ModelActivity activity = new ModelActivity();
            activity.RegisterActivityGetter(() => isActive);
            activity.RegisterStartCondition(() => startCondition);
            activity.RegisterStopCondition(() => stopCondition);
            activity.onFailStart += () => { onJustFailStart = true; };
            activity.onFailStop += () => { onJustFailStop = true; };
            activity.onStart += () => { onJustStart = true; isActive = true; };
            activity.onStop += () => { onJustStop = true; isActive = false; };

            // Test fail start
            onJustFailStart = false;
            Assert.IsFalse(activity.TryStart());
            Assert.IsTrue(onJustFailStart);
            Assert.IsFalse(activity.IsActive());
            Assert.IsFalse(activity.CanStart());

            // Test success start
            onJustStart = false;
            startCondition = true;
            Assert.IsTrue(activity.CanStart());
            Assert.IsTrue(activity.TryStart());
            Assert.IsTrue(onJustStart);
            Assert.IsTrue(isActive);
            Assert.IsTrue(activity.IsActive());

            // Test fail stop
            onJustFailStop = false;
            Assert.IsFalse(activity.CanStop());
            Assert.IsFalse(activity.TryStop());
            Assert.IsTrue(onJustFailStop);
            Assert.IsTrue(isActive);
            Assert.IsTrue(activity.IsActive());

            // Test success start
            stopCondition = true;
            onJustStop = false;
            Assert.IsTrue(activity.CanStop());
            Assert.IsTrue(activity.TryStop());
            Assert.IsTrue(onJustStop);
            Assert.IsFalse(isActive);
            Assert.IsFalse(activity.IsActive());
            _ActivityTTest();
        }
        
        private void _ActivityTTest()
        {
            bool isActive = false, startCondition = false, stopCondition = false;
            bool onJustStart = false, onJustStop = false, onJustFailStart = false, onJustFailStop = false;
            int p = 123;

            ModelActivity<int> activity = new ModelActivity<int>();
            activity.RegisterActivityGetter(() => isActive);
            activity.RegisterStartCondition((test) => { Assert.AreEqual(p, test); return startCondition; });
            activity.RegisterStopCondition(() => stopCondition);
            activity.onFailStart += (test) => { Assert.AreEqual(p, test); onJustFailStart = true; };
            activity.onFailStop += () => { onJustFailStop = true; };
            activity.onStart += (test) => { Assert.AreEqual(p, test); onJustStart = true; isActive = true; };
            activity.onStop += () => { onJustStop = true; isActive = false; };

            // Test fail start
            onJustFailStart = false;
            Assert.IsFalse(activity.TryStart(p));
            Assert.IsTrue(onJustFailStart);
            Assert.IsFalse(activity.IsActive());
            Assert.IsFalse(activity.CanStart(p));

            // Test success start
            onJustStart = false;
            startCondition = true;
            Assert.IsTrue(activity.CanStart(p));
            Assert.IsTrue(activity.TryStart(p));
            Assert.IsTrue(onJustStart);
            Assert.IsTrue(isActive);
            Assert.IsTrue(activity.IsActive());

            // Test fail stop
            onJustFailStop = false;
            Assert.IsFalse(activity.CanStop());
            Assert.IsFalse(activity.TryStop());
            Assert.IsTrue(onJustFailStop);
            Assert.IsTrue(isActive);
            Assert.IsTrue(activity.IsActive());

            // Test success start
            stopCondition = true;
            onJustStop = false;
            Assert.IsTrue(activity.CanStop());
            Assert.IsTrue(activity.TryStop());
            Assert.IsTrue(onJustStop);
            Assert.IsFalse(isActive);
            Assert.IsFalse(activity.IsActive());
        }

        [Test]
        public void ModelEventTest()
        {
            int p = 123;
            bool wasCalled = false;
            ModelEvent msgEvt = new ModelEvent();
            msgEvt.handler += () => { wasCalled = true; };

            msgEvt.Fire();
            Assert.IsTrue(wasCalled);
            wasCalled = false;

            ModelEvent<int> msgEvtT = new ModelEvent<int>();
            msgEvtT.handler += (test) => { Assert.AreEqual(p, test); wasCalled = true; };

            msgEvtT.Fire(p);
            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void ModelPropertyTest()
        {
            int val = 0;
            ModelProperty<int> property = new ModelProperty<int>();
            property.onSetValue += (v) => { val = v; };
            property.SetGetter(() => val);

            val = 123;
            Assert.AreEqual(val, property.Get());

            property.Set(1337);
            Assert.AreEqual(1337, val);

            // Test overriding
            int val2 = 321;
            property.SetGetter(() => val2);
            Assert.AreEqual(val2, property.Get());
        }

        [Test]
        public void ModelModifiableValueTest()
        {
            ModelModifiableInt integer = new ModelModifiableInt(100);

            Assert.AreEqual(100, integer.Get());

            integer.AddOverrideEvaluator((val) => val * 2, 1);
            Assert.AreEqual(200, integer.Get());

            integer.AddOverrideEvaluator((val) => val * 2, 3);
            Assert.AreEqual(400, integer.Get());

            integer.AddOverrideEvaluator((val) => val * 3, 2);
            Assert.AreEqual(1200, integer.Get());
        }

        [Test]
        public void ModelAttemptTest()
        {
            bool condition = false, onFireCalled = false, onFailCalled = false;
            ModelAttempt attempt = new ModelAttempt();
            attempt.RegisterCondition(() => condition);
            attempt.onFire += () => { onFireCalled = true; };
            attempt.onFail += () => { onFailCalled = true; };

            // Test fail
            onFailCalled = false;
            Assert.IsFalse(attempt.Can());
            Assert.IsFalse(attempt.Try());
            Assert.IsTrue(onFailCalled);

            // Test success
            onFireCalled = false;
            condition = true;
            Assert.IsTrue(attempt.Can());
            Assert.IsTrue(attempt.Try());
            Assert.IsTrue(onFireCalled);

            _ModelAttemptTTest();
            _ModelAttemptT1T2Test();
        }

        private void _ModelAttemptTTest()
        {
            int p = 123;
            bool condition = false, onFireCalled = false, onFailCalled = false;
            ModelAttempt<int> attempt = new ModelAttempt<int>();
            attempt.RegisterCondition((test) => { Assert.AreEqual(p, test); return condition; });
            attempt.onFire += (test) => { Assert.AreEqual(p, test); onFireCalled = true; };
            attempt.onFail += (test) => { Assert.AreEqual(p, test); onFailCalled = true; };

            // Test fail
            onFailCalled = false;
            Assert.IsFalse(attempt.Can(p));
            Assert.IsFalse(attempt.Try(p));
            Assert.IsTrue(onFailCalled);

            // Test success
            onFireCalled = false;
            condition = true;
            Assert.IsTrue(attempt.Can(p));
            Assert.IsTrue(attempt.Try(p));
            Assert.IsTrue(onFireCalled);
        }

        private void _ModelAttemptT1T2Test()
        {
            int p = 123, p2 = 1337;
            bool condition = false, onFireCalled = false, onFailCalled = false;
            ModelAttempt<int, int> attempt = new ModelAttempt<int, int>();
            attempt.RegisterCondition((test, test2) => { Assert.AreEqual(p, test); Assert.AreEqual(p2, test2); return condition; });
            attempt.onFire += (test, test2) => { Assert.AreEqual(p, test); Assert.AreEqual(p2, test2); onFireCalled = true; };
            attempt.onFail += (test, test2) => { Assert.AreEqual(p, test); Assert.AreEqual(p2, test2); onFailCalled = true; };

            // Test fail
            onFailCalled = false;
            Assert.IsFalse(attempt.Can(p, p2));
            Assert.IsFalse(attempt.Try(p, p2));
            Assert.IsTrue(onFailCalled);

            // Test success
            onFireCalled = false;
            condition = true;
            Assert.IsTrue(attempt.Can(p, p2));
            Assert.IsTrue(attempt.Try(p, p2));
            Assert.IsTrue(onFireCalled);
        }
    }
}