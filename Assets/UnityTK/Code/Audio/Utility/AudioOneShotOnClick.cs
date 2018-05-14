using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityTK.Audio
{
    /// <summary>
    /// Plays an <see cref="AudioEvent"/>
    /// </summary>
    public class AudioOneShotOnClick : MonoBehaviour, IPointerClickHandler
    {
        public AudioEvent audioEvent;

        /// <summary>
        /// Optional parameter, the audio source used for playing the event.
        /// If not specified, <see cref="AudioOneShotPlayer"/> api will be used!
        /// </summary>
        [Tooltip("Optional parameter, the audio source used for playing the event. If not specified, AudioOneShotPlayer api will be used!")]
        public UTKAudioSource audioSource;

        /// <summary>
        /// The playback type.
        /// </summary>
        public AudioOneShotPlayer.PlaybackType playbackType = AudioOneShotPlayer.PlaybackType.WORLDSPACE;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Essentials.UnityIsNull(this.audioSource))
            {
                switch (this.playbackType)
                {
                    case AudioOneShotPlayer.PlaybackType.WORLDSPACE: AudioOneShotPlayer.instance.PlayWorldspace(this.audioEvent, this.gameObject); break;
                    case AudioOneShotPlayer.PlaybackType.NONSPATIAL: AudioOneShotPlayer.instance.PlayNonSpatial(this.audioEvent); break;
                    case AudioOneShotPlayer.PlaybackType.PROXIMITY: AudioOneShotPlayer.instance.PlayProximity(this.audioEvent, this.gameObject); break;
                }
            }
            else
                this.audioEvent.Play(this.audioSource);
        }
    }
}