using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Audio
{
    /// <summary>
    /// Interface used for an abstraction layer between unity engine audio and the UnityTK audio system.
    /// </summary>
    public interface IUTKAudioSource
    {
        float volume { get; set; }
        float pitch { get; set; }
        float time { get; set; }
        AudioClip clip { get; }
        bool isPlaying { get; }

        float minDistance { get; set; }
        float maxDistance { get; set; }
        AudioRolloffMode rolloffMode { get; set; }

        /// <summary>
        /// Plays the specified audio clip.
        /// <see cref="AudioEvent.Play(IUTKAudioSource, bool)"/>
        /// </summary>
        /// <param name="clip">The clip to play</param>
        /// <param name="loop">Whether or not starting to play this sound in a loop, if it is being looped it will play until you call <see cref="IUTKAudioSource.Stop"/> on your audio source!</param>
        void Play(AudioClip clip, bool loop = false);

        /// <summary>
        /// Immediately stops playing any ongoing playbacks.
        /// </summary>
        void Stop();

        /// <summary>
        /// Called in order to "clean up" the confiuration of this audio source.
        /// Used for example when audio sources are being pool, <see cref="AudioOneShotPlayer"/>.
        /// </summary>
        void ResetConfig();
    }
}