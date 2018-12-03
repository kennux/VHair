using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityTK.Audio;

namespace UnityTK.Test.Audio
{
    public class EventTests
    {
        [Test]
        public void TestSimpleAudioEvent()
        {
            // Arrange
            UTKAudioMock aSource = new UTKAudioMock();
            SimpleAudioEvent evt = ScriptableObject.CreateInstance<SimpleAudioEvent>();
            evt.volume = new RangedFloat(.5f, .75f);
            evt.maxDistance = 2;
            evt.minDistance = 1;
            evt.pitch = new RangedFloat(.5f, .75f);
            evt.rolloffMode = AudioRolloffMode.Linear;

            bool playWasCalled = false;
            bool playWasLooped = false;
            AudioClip playedAudioClip = null;
            aSource.onPlay += (clip, loop) =>
            {
                playWasCalled = true;
                playWasLooped = loop;
                playedAudioClip = clip;
            };

            // Act
            evt.Play(aSource, false);

            // Assert
            Assert.IsTrue(aSource.volume >= evt.volume.minValue && aSource.volume <= evt.volume.maxValue);
            Assert.IsTrue(aSource.pitch >= evt.pitch.minValue && aSource.pitch <= evt.pitch.maxValue);
            Assert.AreEqual(evt.minDistance, aSource.minDistance);
            Assert.AreEqual(evt.maxDistance, aSource.maxDistance);
            Assert.AreEqual(evt.rolloffMode, aSource.rolloffMode);
            Assert.IsTrue(playWasCalled);
            Assert.IsFalse(playWasLooped);
            Assert.AreEqual(evt.clip, playedAudioClip);

            // Clean up
            playWasCalled = playWasLooped = false;
            playedAudioClip = null;

            // Act again (looped this time)
            evt.Play(aSource, true);

            // Assert
            Assert.IsTrue(aSource.volume >= evt.volume.minValue && aSource.volume <= evt.volume.maxValue);
            Assert.IsTrue(aSource.pitch >= evt.pitch.minValue && aSource.pitch <= evt.pitch.maxValue);
            Assert.AreEqual(evt.minDistance, aSource.minDistance);
            Assert.AreEqual(evt.maxDistance, aSource.maxDistance);
            Assert.AreEqual(evt.rolloffMode, aSource.rolloffMode);
            Assert.IsTrue(playWasCalled);
            Assert.IsTrue(playWasLooped);
            Assert.AreEqual(evt.clip, playedAudioClip);
        }
    }
}