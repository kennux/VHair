using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK.AssetManagement;

namespace UnityTK.Audio
{
    /// <summary>
    /// Abstract implementation of an audio "event".
    /// An audio event can be played on a unity audio source as soon as something in the game happens (for example a gun is being shot).
    /// They can define arbitrary playback actions like for example playing back the clip in a random pitch.
    /// 
    /// This abstract design is inspired by "Unite 2016 - Overthrowing the MonoBehaviour Tyranny in a Glorious Scriptable Object Revolution" - https://www.youtube.com/watch?v=6vmRwLYWNRo
    /// </summary>
    public abstract class AudioEvent : ManagedScriptableObject
    {
        /// <summary>
        /// Called when this audio event is being fired.
        /// Plays the event on the specified audio source.
        /// </summary>
        /// <param name="audioSource">The audio source to play the event on.</param>
        /// <param name="loop">Whether or not starting to play this sound in a loop, if it is being looped it will play until you call <see cref="IUTKAudioSource.Stop"/> on your audio source!</param>
        public abstract void Play(IUTKAudioSource audioSource, bool loop = false);
    }
}