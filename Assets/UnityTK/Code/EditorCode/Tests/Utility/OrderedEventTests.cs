using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace UnityTK.Test.Utility
{
    public class OrderedEventTests
    {
        private int firstCalled;
        private int secondCalled;

        [Test]
        public void OrderedEventTest()
        {
            // Arrange
            OrderedEvent evt = new OrderedEvent();
            int q1 = 100, q2 = 200;
            firstCalled = secondCalled = -1;

            evt.Bind(this.FirstCall, q1);

            evt.Bind(this.SecondCall, q2);

            // Act
            evt.Invoke();

            // Assert
            Assert.AreEqual(1, firstCalled);
            Assert.AreEqual(2, secondCalled);

            // Test unbind
            firstCalled = secondCalled = -1;

            // Act
            evt.Unbind(SecondCall);
            evt.Invoke();

            // Assert
            Assert.AreEqual(1, firstCalled);
            Assert.AreEqual(-1, secondCalled);
        }

        private void FirstCall()
        {
            if (firstCalled == -1)
            {
                firstCalled = 1;
            }
            else if (secondCalled == -1)
            {
                secondCalled = 1;
            }
        }

        private void SecondCall()
        {
            if (firstCalled == -1)
            {
                firstCalled = 2;
            }
            else if (secondCalled == -1)
            {
                secondCalled = 2;
            }
        }
    }
}