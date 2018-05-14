using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityTK.Audio
{
    /// <summary>
    /// "Normal" spatial audio source implementation, essentially a wrapper around <see cref="AudioSource"/> from the unity engine.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class UTKAudioSource : MonoBehaviour, IUTKAudioSource
    {
        public virtual AudioSource underlying
        {
            get { return this._underlying.Get(this); }
        }
        private LazyLoadedComponentRef<AudioSource> _underlying = new LazyLoadedComponentRef<AudioSource>();

        public virtual float volume
        {
            get { return this.underlying.volume; }
            set { this.underlying.volume = value; }
        }

        public virtual float pitch
        {
            get { return this.underlying.pitch; }
            set { this.underlying.pitch = value; }
        }
        public virtual AudioClip clip
        {
            get { return this.underlying.clip; }
            set { this.underlying.clip = value; }
        }

        public virtual bool isPlaying
        {
            get { return this.underlying.isPlaying; }
        }

        public virtual float minDistance
        {
            get { return this.underlying.minDistance; }
            set { this.underlying.minDistance = value; }
        }

        public virtual float maxDistance
        {
            get { return this.underlying.maxDistance; }
            set { this.underlying.maxDistance = value; }
        }

        public virtual float time
        {
            get { return this.underlying.time; }
            set { this.underlying.time = value; }
        }

        public virtual AudioRolloffMode rolloffMode
        {
            get { return this.underlying.rolloffMode; }
            set { this.underlying.rolloffMode = value; }
        }

        public virtual void Play(AudioClip clip, bool loop = false)
        {
            this.clip = clip;
            this.underlying.loop = loop;
            this.underlying.Play();
        }

        public virtual void Stop()
        {
            this.clip = null;
            this.underlying.Stop();
        }

        public virtual void ResetConfig()
        {
            this.volume = 1;
            this.pitch = 1;
            this.clip = null;
            this.minDistance = 1;
            this.maxDistance = 500;
            this.rolloffMode = AudioRolloffMode.Logarithmic;
        }
    }
}
