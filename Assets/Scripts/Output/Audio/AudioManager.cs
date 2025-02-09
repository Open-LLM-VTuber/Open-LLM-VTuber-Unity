using UnityEngine;
using ECS;
using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

public class AudioManager : InitOnceSingleton<AudioManager>
{
    // 配置参数
    [SerializeField] private GameObject audioRootPrefab;

    // ECS 核心组件
    private EntityManager _entityManager;
    private ComponentManager _componentManager;
    private AudioPlaybackSystem _audioSystem;

    // 根物体
    private GameObject _audioRoot;

    // 按种类区分
    private Dictionary<ECS.AudioType, List<int>> _audioEntitiesByType 
        = new Dictionary<ECS.AudioType, List<int>>();

    void Start() // 在 ECS Awake 之后调用, 只能用 Start
    {
        InitOnce(() => 
        {
            CreateAudioRoot();
            InitializeECS();
        });
        
    }

    private void InitializeECS()
    {
        // 共享单例组件
        _entityManager = EntityManager.Instance;
        _componentManager = ComponentManager.Instance;

        // 创建音频系统
        _audioSystem = new AudioPlaybackSystem(
            _entityManager,
            _componentManager,
            _audioRoot
        );

        // 注册到 SystemManager
        SystemManager.Instance.RegisterSystem(_audioSystem);
    }

    private void CreateAudioRoot()
    {
        _audioRoot = audioRootPrefab != null ?
            Instantiate(audioRootPrefab) :
            new GameObject("AudioRoot");

        DontDestroyOnLoad(_audioRoot);
    }

    // ----------- 公共接口保持不变 -----------
    public int CreateAudioEntity(
        AudioClip clip, 
        bool playOnCreate = true, 
        ECS.AudioType type = ECS.AudioType.AssistantVoice
    )
    {
        int entity = _entityManager.CreateEntity();
        var audioComp = new AudioComponent
        {
            Clip = clip,
            PlayOnCreate = playOnCreate,
            Type = type,
        };
        _componentManager.AddComponent(entity, audioComp);

        // 将实体按类型分类
        if (!_audioEntitiesByType.ContainsKey(type))
        {
            _audioEntitiesByType[type] = new List<int>();
        }
        _audioEntitiesByType[type].Add(entity);

        return entity;
    }

    public int CreateAudioEntityFromBase64(string base64, bool playOnCreate = true)
    {
        AudioClip clip = Base64AudioClipConverter.ConvertBase64ToAudioClip(base64);
        return CreateAudioEntity(clip, playOnCreate);
    }

    public void PlayAudio(int entityId, Action onFinishedCallback = null)
    {
        if (_componentManager.GetComponent<AudioComponent>(entityId) is AudioComponent comp)
        {
            // 开始播放音频, 通过更改状态让音频播放系统感知到变化
            comp.PlayOnCreate = true;
            // 使用协程等待音频播放完成
            StartCoroutine(WaitForAudioCompletion(comp, onFinishedCallback));
        }
    }

    private IEnumerator WaitForAudioCompletion(AudioComponent comp, Action onFinishedCallback)
    {
        // 等待音频开始播放
        while (comp.Source == null || !comp.Source.isPlaying)
        {
            yield return null;
        }

        // 等待音频播放完成
        while (comp.Source != null && comp.Source.isPlaying)
        {
            yield return null;
        }

        // 音频播放完成后调用回调函数
        if (onFinishedCallback != null)
        {
            onFinishedCallback();
        }
    }

    public void StopAudio(AudioComponent comp)
    {
        if (comp.Source != null && comp.Source.isPlaying)
        {
            comp.Source.Stop();
            comp.PlayOnCreate = false;
        }
    }

    public void RemoveAudio(int entityId)
    {
        _componentManager.RemoveComponent<AudioComponent>(entityId);
        _entityManager.RemoveEntity(entityId);
    }

    public void RemoveAllAudioByType(ECS.AudioType type)
    {

        if (_audioEntitiesByType.ContainsKey(type))
        {
            foreach (var entityId in _audioEntitiesByType[type])
            {
                if (_componentManager.GetComponent<AudioComponent>(entityId) 
                        is AudioComponent audioComp)
                {
                    StopAudio(audioComp);
                    RemoveAudio(entityId);
                }
            }
            // 清空该类型的实体列表
            _audioEntitiesByType[type].Clear();
        }

    }

    void OnDestroy()
    {
        _audioEntitiesByType.Clear();
        // 清理所有音频实体
        foreach (var entityId in _entityManager.GetActiveEntitiesSnapshot())
        {
            RemoveAudio(entityId);
        }
        if (SystemManager.Instance != null)
        {
            // 注销系统
            SystemManager.Instance.UnregisterSystem(_audioSystem);
        }
    }
}