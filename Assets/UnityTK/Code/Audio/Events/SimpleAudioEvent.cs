using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Audio
{
    /// <summary>
    /// Simple audio event implementation playing back a sound on an audio source.
    /// </summary>
    [CreateAssetMenu(fileName = "SimpleAudioEvent", menuName = "UnityTK/Audio/Simple Audio Event")]
    public class SimpleAudioEvent : AudioEvent
    {
        /// <summary>
        /// The volume range in which to play this event.
        /// </summary>
        [MinMaxRange(0,1)]
        public RangedFloat volume = new RangedFloat(1, 1);

        /// <summary>
        /// The pitch range in which to play this event.
        /// </summary>
        [MinMaxRange(0,2)]
        public RangedFloat pitch = new RangedFloat(1, 1);

        /// <summary>
        /// The clip to play.
        /// </summary>
        public AudioClip clip;

        /// <summary>
        /// Roll off minimum distance.
        /// </summary>
        public float minDistance;

        /// <summary>
        /// Roll off maximum distance.
        /// </summary>
        public float maxDistance;

        /// <summary>
        /// The rolloff mode for the audio event.
        /// Custom rolloff can _not_ be set at runtime and thus is not a field of this event class.
        /// 
        /// Custom rolloff must be preconfigured in the editor on the audio source directly :/
        /// </summary>
        public AudioRolloffMode rolloffMode;

        /// <summary>
        /// Plays <see cref="clip"/>
        /// </summary>
        public override void Play(IUTKAudioSource audioSource, bool loop = false)
        {
            audioSource.volume = this.volume.GetRandomInRange();
            audioSource.pitch = this.pitch.GetRandomInRange();
            audioSource.minDistance = this.minDistance;
            audioSource.maxDistance = this.maxDistance;
            audioSource.rolloffMode = this.rolloffMode;
            audioSource.Play(this.clip, loop);
        }
    }
}