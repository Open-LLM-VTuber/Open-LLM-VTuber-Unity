using UnityEngine;
using System.Collections.Generic;

namespace ECS
{
    public sealed class AudioPlaybackSystem : System
    {
        private const int POOL_SIZE = 2;
        private const float UPDATE_INTERVAL = 1.0f;
        private readonly Queue<AudioSource> _sourcePool = new Queue<AudioSource>();
        private readonly Dictionary<int, AudioSource> _activeSources = new Dictionary<int, AudioSource>();
         private Dictionary<int, int> _lastSamplePositions = new Dictionary<int, int>();
        private readonly Transform _audioRoot;
        private float _lastUpdateTime;

        public AudioPlaybackSystem(
            EntityManager em,
            ComponentManager cm,
            GameObject audioRoot
        ) : base(SystemType.Audio, em, cm)
        {
            _audioRoot = audioRoot.transform;
            _lastUpdateTime = Time.time;
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
            ProcessAudioRequests();

            // Process per-frame sample callbacks for AssistantVoice
            ProcessSampleCallbacks();
                        // Periodic cleanup
            if (Time.time - _lastUpdateTime >= UPDATE_INTERVAL)
            {
                CleanFinishedSources();
                _lastUpdateTime = Time.time;
            }   
        }

        private void ProcessSampleCallbacks()
        {
            foreach (var pair in _activeSources)
            {
                int entity = pair.Key;
                var comp = GetComponent<AudioComponent>(entity);

                if (comp != null && comp.IsPlaying && comp.OnSamplesPlayed != null)
                {
                    AudioSource source = pair.Value;
                    int currentSample = source.timeSamples;
                    int lastSample = _lastSamplePositions.ContainsKey(entity) ? _lastSamplePositions[entity] : 0;
                    int samplesPlayed = currentSample - lastSample;

                    if (samplesPlayed > 0) // Only process if there’s progress
                    {
                        float[] sampleData = new float[samplesPlayed];
                        comp.Clip.GetData(sampleData, lastSample);
                        comp.OnSamplesPlayed(sampleData);
                    }

                    _lastSamplePositions[entity] = currentSample;
                }
            }
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
                    _lastSamplePositions.Remove(pair.Key);
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
            _lastSamplePositions[entity] = 0; // Initialize sample position 
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