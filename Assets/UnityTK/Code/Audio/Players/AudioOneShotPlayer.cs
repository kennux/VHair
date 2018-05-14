using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Audio
{
    /// <summary>
    /// Manager behaviour / singleton player implementation that can be used to oneshot play <see cref="AudioEvent"/>.
    /// </summary>
    public class AudioOneShotPlayer : MonoBehaviour
    {
        /// <summary>
        /// Playback type
        /// </summary>
        public enum PlaybackType
        {
            WORLDSPACE,
            PROXIMITY,
            NONSPATIAL
        }

        /// <summary>
        /// Playback datastructure.
        /// </summary>
        public struct Playback
        {
            public PlaybackType type;
            public IUTKAudioSource source;
            public AudioEvent evt;
        }

        public static AudioOneShotPlayer instance { get { return UnitySingleton<AudioOneShotPlayer>.Get(); } }

        [Header("Prefabs")]
        public UTKAudioSource worldspacePrefab;
        public ProximityBasedAudio proximityBasedPrefab;
        public NonSpatialAudioSource nonSpatialPrefab;

        /// <summary>
        /// Audio sources used for worldspace playback.
        /// </summary>
        private ObjectPool<UTKAudioSource> worldspaceAudioSources;

        /// <summary>
        /// Audio sources used for proximity based playback.
        /// </summary>
        private ObjectPool<ProximityBasedAudio> proximityAudioSources;

        /// <summary>
        /// Audio sources used for non spatial playback.
        /// </summary>
        private ObjectPool<NonSpatialAudioSource> nonSpatialAudioSources;

        /// <summary>
        /// All currently ongoing playbacks.
        /// </summary>
        private List<Playback> playbacks = new List<Playback>();

        public void Awake()
        {
            UnitySingleton<AudioOneShotPlayer>.Register(this);
            this.worldspaceAudioSources = new ObjectPool<UTKAudioSource>(() => this.CreateAudioSource<UTKAudioSource>(this.worldspacePrefab, this.ReturnAudioSource), 250, this.ReturnAudioSource);
            this.proximityAudioSources = new ObjectPool<ProximityBasedAudio>(() => this.CreateAudioSource<ProximityBasedAudio>(this.proximityBasedPrefab, this.ReturnAudioSource), 250, this.ReturnAudioSource);
            this.nonSpatialAudioSources = new ObjectPool<NonSpatialAudioSource>(() => this.CreateAudioSource<NonSpatialAudioSource>(this.nonSpatialPrefab, this.ReturnAudioSource), 250, this.ReturnAudioSource);
        }

        /// <summary>
        /// Generic audio source return method called when audio sources are being returned into the pools.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        private void ReturnAudioSource<T>(T source) where T : UTKAudioSource
        {
            // Stop playing and reset
            source.Stop();
            source.ResetConfig();

            // Setup gameobject
            source.gameObject.SetActive(false);
            source.transform.parent = this.transform;
            source.transform.localPosition = Vector3.zero;
            source.transform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// Creates an audio source instance from a prefab and initializes it using the initializer.
        /// </summary>
        private T CreateAudioSource<T> (T prefab, System.Action<T> initializer) where T : UTKAudioSource
        {
            // Create
            GameObject go = Instantiate(prefab.gameObject);
            T source = go.GetComponent<T>();

            // Run initializer
            initializer(source);
            return source;
        }

        public void Update()
        {
            // Figure out which audio sources stopped playing audio
            List<int> stopped = ListPool<int>.Get();
            for (int i = 0; i < this.playbacks.Count; i++)
            {
                if (!this.playbacks[i].source.isPlaying)
                    stopped.Add(i);
            }

            // Remove all stopped audio sources
            for (int i = 0; i < stopped.Count; i++)
            {
                var playback = this.playbacks[stopped[i]];
                this.playbacks.RemoveAt(stopped[i] - i);
                
                switch (playback.type)
                {
                    case PlaybackType.PROXIMITY: this.proximityAudioSources.Return(playback.source as ProximityBasedAudio); break;
                    case PlaybackType.WORLDSPACE: this.worldspaceAudioSources.Return(playback.source as UTKAudioSource); break;
                    case PlaybackType.NONSPATIAL: this.nonSpatialAudioSources.Return(playback.source as NonSpatialAudioSource); break;
                }
            }

            ListPool<int>.Return(stopped);
        }

        /// <summary>
        /// Generic implementation for playing a specific event.
        /// 
        /// <see cref="PlayNonSpatial(AudioEvent)"/>
        /// <see cref="PlayProximity(AudioEvent, GameObject)"/>
        /// <see cref="PlayWorldspace(AudioEvent, GameObject)"/>
        /// </summary>
        /// <typeparam name="T">The audio source type.</typeparam>
        /// <param name="evt">The event to play</param>
        /// <param name="source">The source on where to play it.</param>
        /// <returns>The created playback of the event.</returns>
        private Playback Play<T>(AudioEvent evt, T source, PlaybackType type) where T : IUTKAudioSource
        {
            evt.Play(source);

            var playback = new Playback()
            {
                evt = evt,
                source = source,
                type = type
            };
            this.playbacks.Add(playback);
            return playback;
        }

        /// <summary>
        /// Plays the specified event once on a <see cref="NonSpatialAudioSource"/>.
        /// </summary>
        /// <param name="evt">The event to play</param>
        public Playback PlayNonSpatial(AudioEvent evt)
        {
            var source = this.nonSpatialAudioSources.Get();
            source.gameObject.SetActive(true);
            Playback playback = Play(evt, source, PlaybackType.NONSPATIAL);
            return playback;
        }

        /// <summary>
        /// Plays the specified event once on the specified player gameobject.
        /// This will make the playback audio source be parented to player at origin position and identity rotation.
        /// 
        /// Note that this method is not doing any spatial playback configuration on the audio source _at all_.
        /// The audio event should do the spatial setup.
        /// 
        /// Playback is done with an audio source which has the <see cref="ProximityBasedAudio"/> component.
        /// </summary>
        /// <param name="evt">The event to play</param>
        /// <param name="player">The object which is playing the event.</param>
        public Playback PlayProximity(AudioEvent evt, GameObject player)
        {
            var source = this.proximityAudioSources.Get();
            source.gameObject.SetActive(true);
            Playback playback = Play(evt, source, PlaybackType.PROXIMITY);

            source.transform.parent = player.transform;
            source.transform.localPosition = Vector3.zero;
            source.transform.localRotation = Quaternion.identity;
            return playback;
        }

        /// <summary>
        /// Plays the specified event once on the specified player gameobject.
        /// This will make the playback audio source be parented to player at origin position and identity rotation.
        /// 
        /// Note that this method is not doing any spatial playback configuration on the audio source _at all_.
        /// The audio event should do the spatial setup.
        /// </summary>
        /// <param name="evt">The event to play</param>
        /// <param name="player">The object which is playing the event.</param>
        /// <returns>Playback information</returns>
        public Playback PlayWorldspace(AudioEvent evt, GameObject player)
        {
            var source = this.worldspaceAudioSources.Get();
            source.gameObject.SetActive(true);
            Playback playback = Play(evt, source, PlaybackType.WORLDSPACE);

            source.transform.parent = player.transform;
            source.transform.localPosition = Vector3.zero;
            source.transform.localRotation = Quaternion.identity;
            return playback;
        }
    }
}