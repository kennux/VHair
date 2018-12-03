using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Audio
{
    /// <summary>
    /// Composite audio event.
    /// Has several different audio events referenced and will select one randomly when played.
    /// </summary>
    [CreateAssetMenu(fileName = "SimpleAudioEvent", menuName = "UnityTK/Audio/Composite Audio Event")]
    public class CompositeAudioEvent : AudioEvent
    {
        /// <summary>
        /// Source for drawing the random event when <see cref="Play(IUTKAudioSource, bool)"/> is called.
        /// </summary>
        public AudioEvent[] events;

        /// <summary>
        /// Will play one event randomly selected from <see cref="events"/>
        /// <see cref="AudioEvent.Play(IUTKAudioSource, bool)"/>
        /// </summary>
        public override void Play(IUTKAudioSource audioSource, bool loop = false)
        {
            this.events.RandomItem().Play(audioSource, loop);
        }
    }
}