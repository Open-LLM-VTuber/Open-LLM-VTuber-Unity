using UnityEngine;
using System.Collections.Generic;

namespace ECS
{
    public sealed class AudioPlaybackSystem : System
    {
        private const int POOL_SIZE = 2;
        private readonly Queue<AudioSource> _sourcePool = new Queue<AudioSource>();
        private readonly Dictionary<int, AudioSource> _activeSources = new Dictionary<int, AudioSource>();
        private readonly Transform _audioRoot;

        public AudioPlaybackSystem(
            EntityManager em,
            ComponentManager cm,
            GameObject audioRoot
        ) : base(SystemType.Audio, em, cm)
        {
            _audioRoot = audioRoot.transform;
            InitializePool();
        }

        private void InitializePool()
        {
            for (int i = 0; i < POOL_SIZE; i++)
            {
                var source = new GameObject($"AudioSource_{i}")
                    .AddComponent<AudioSource>();

                source.transform.SetParent(_audioRoot);
                source.playOnAwake = false;
                _sourcePool.Enqueue(source);
            }
        }

        public override void Update()
        {
            CleanFinishedSources();
            ProcessAudioRequests();
        }

        private void CleanFinishedSources()
        {
            List<int> finished = new List<int>();

            foreach (var pair in _activeSources)
            {
                if (!pair.Value.isPlaying)
                {
                    ReturnSource(pair.Value);
                    finished.Add(pair.Key);
                }
            }

            foreach (var id in finished)
            {
                _activeSources.Remove(id);
            }
        }

        private void ProcessAudioRequests()
        {
            foreach (var entity in GetEntities(typeof(AudioComponent)))
            {
                var comp = GetComponent<AudioComponent>(entity);

                if (ShouldPlay(comp))
                {
                    PlayAudio(entity, comp);
                }
            }
        }

        private bool ShouldPlay(AudioComponent comp)
        {
            return comp.Enabled &&
                  (comp.PlayOnCreate || comp.Loop) &&
                  !comp.IsPlaying;
        }

        private void PlayAudio(int entity, AudioComponent comp)
        {
            if (_sourcePool.Count == 0) return;

            AudioSource source = _sourcePool.Dequeue();
            ConfigureSource(source, comp);
            source.Play();

            comp.Source = source;
            comp.PlayOnCreate = false;
            _activeSources[entity] = source;
        }

        private void ConfigureSource(AudioSource source, AudioComponent config)
        {
            source.clip = config.Clip;
            source.loop = config.Loop;
            source.volume = config.Volume;
            source.spatialBlend = 0; // 2D音效
        }

        private void ReturnSource(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            _sourcePool.Enqueue(source);
        }
    }
}