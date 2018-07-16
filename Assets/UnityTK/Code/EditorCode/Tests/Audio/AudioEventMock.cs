using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Audio.Editor.Test
{
    public class AudioEventMock : AudioEvent
    {
        public event System.Action<IUTKAudioSource, bool> onPlay;

        public override void Play(IUTKAudioSource audioSource, bool loop = false)
        {
            if (this.onPlay != null)
                this.onPlay(audioSource, loop);
        }
    }
}