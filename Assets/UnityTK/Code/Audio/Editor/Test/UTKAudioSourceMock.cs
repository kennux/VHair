using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Audio.Editor.Test
{
    public class UTKAudioMock : IUTKAudioSource
    {
        public event System.Action<AudioClip, bool> onPlay;
        public event System.Action onStop;
        public event System.Action onResetConfig;

        public virtual float volume
        {
            get; set;
        }

        public virtual float pitch
        {
            get; set;
        }
        public virtual AudioClip clip
        {
            get; set;
        }

        public virtual bool isPlaying
        {
            get; private set;
        }

        public virtual float minDistance
        {
            get; set;
        }

        public virtual float maxDistance
        {
            get; set;
        }

        public virtual float time
        {
            get; set;
        }

        public virtual AudioRolloffMode rolloffMode
        {
            get; set;
        }

        public virtual void Play(AudioClip clip, bool loop = false)
        {
            if (this.onPlay != null)
                this.onPlay(clip, loop);
        }

        public virtual void Stop()
        {
            if (this.onStop != null)
                this.onStop();
        }

        public virtual void ResetConfig()
        {
            if (this.onResetConfig != null)
                this.onResetConfig();
        }
    }
}