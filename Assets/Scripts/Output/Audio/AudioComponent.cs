using UnityEngine;
using System;

namespace ECS
{
    public class AudioComponent : Component
    {
        // 基础配置
        public AudioClip Clip;
        public bool PlayOnCreate = true;
        public bool Loop = false;
        [Range(0, 1)] public float Volume = 1.0f;

        // 运行时状态
        [NonSerialized] public AudioSource Source;
        public bool IsPlaying => Source != null && Source.isPlaying;
    }
}