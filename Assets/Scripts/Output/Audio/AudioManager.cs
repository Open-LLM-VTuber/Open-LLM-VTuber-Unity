using UnityEngine;
using ECS;
using System;
using System.Collections;
using System.Threading.Tasks;

public class AudioManager : MonoBehaviour
{
    // 配置参数
    [SerializeField] private GameObject audioRootPrefab;

    // ECS 核心组件
    private EntityManager _entityManager;
    private ComponentManager _componentManager;
    private AudioPlaybackSystem _audioSystem;

    // 根物体
    private GameObject _audioRoot;

    public static AudioManager Instance { get; private set; }

    void Start() // 在 ECS Awake 之后调用, 只能用 Start
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        CreateAudioRoot();
        InitializeECS();
        
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
    public int CreateAudioEntity(AudioClip clip, bool playOnCreate = true)
    {
        int entity = _entityManager.CreateEntity();
        var audioComp = new AudioComponent
        {
            Clip = clip,
            PlayOnCreate = playOnCreate
        };
        _componentManager.AddComponent(entity, audioComp);
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
            StartCoroutine(WaitForAudioCompletion(comp.Clip, onFinishedCallback));
        }
    }

    private IEnumerator WaitForAudioCompletion(AudioClip clip, Action onFinishedCallback)
    {
        yield return new WaitForSeconds(clip.length);
        if (onFinishedCallback != null)
        {
            onFinishedCallback(); // 音频播放完成后调用回调函数
        }
    }

    public void StopAudio(int entityId)
    {
        if (_componentManager.GetComponent<AudioComponent>(entityId) is AudioComponent comp)
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

    void OnDestroy()
    {
        // 清理所有音频实体
        foreach (var entityId in _entityManager.GetActiveEntitiesSnapshot())
        {
            RemoveAudio(entityId);
        }
        // 注销系统
        SystemManager.Instance.UnregisterSystem(_audioSystem);
    }
}