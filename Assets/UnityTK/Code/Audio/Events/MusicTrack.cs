using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Audio
{
    /// <summary>
    /// Audio event implementation specifically built for music playback with <see cref="MusicPlayer"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "MusicTrack", menuName = "UnityTK/Audio/Music Track")]
    public class MusicTrack : AudioEvent
    {
        [Header("Metadata")]
        public string title;
        public string interpret;

        [Header("Fading")]
        public float fadeInTime;
        public AnimationCurve fadeCurve;

        /// <summary>
        /// The clip to play.
        /// </summary>
        [Header("Configuration")]
        public AudioClip clip;

        /// <summary>
        /// Plays <see cref="clip"/>
        /// </summary>
        public override void Play(IUTKAudioSource source, bool loop = false)
        {
            source.Play(this.clip, loop);
        }
    }
}