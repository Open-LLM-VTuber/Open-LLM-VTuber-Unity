using UnityEngine;
using System;

namespace ECS
{
    public enum AudioType
    {
        AssistantVoice = 0,
        HumanVoice = 1,
        SoundEffect = 2,
        Music = 3,
        Environment = 4,
    }

    public class AudioComponent : Component
    {
        // 基础配置
        public AudioClip Clip;
        public bool PlayOnCreate = true;
        public bool Loop = false;
        public AudioType Type = AudioType.AssistantVoice;
        [Range(0, 1)] public float Volume = 1.0f;

        // 运行时状态
        [NonSerialized] public AudioSource Source;
         // Delegate for AssistantVoice per-frame sample callback
        [NonSerialized] public Action<float[]> OnSamplesPlayed;
        public bool IsPlaying => Source != null && Source.isPlaying;
    }
}