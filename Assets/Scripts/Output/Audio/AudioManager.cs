using UnityEngine;
using ECS;

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

    public int CreateAudioEntityFromBase64(string base64, int sampleRate = 44100)
    {
        AudioClip clip = Base64AudioClipConverter.ConvertBase64ToAudioClip(base64, sampleRate);
        return CreateAudioEntity(clip);
    }

    public void PlayAudio(int entityId)
    {
        if (_componentManager.GetComponent<AudioComponent>(entityId) is AudioComponent comp)
        {
            comp.PlayOnCreate = true;
            _componentManager.AddComponent(entityId, comp);
        }
    }

    public void StopAudio(int entityId)
    {
        if (_componentManager.GetComponent<AudioComponent>(entityId) is AudioComponent comp)
        {
            comp.Source?.Stop();
            comp.PlayOnCreate = false;
            _componentManager.AddComponent(entityId, comp);
        }
    }

    void OnDestroy()
    {
        // 清理所有音频实体
        foreach (var entity in _entityManager.GetActiveEntities())
        {
            if (_componentManager.GetComponent<AudioComponent>(entity) is AudioComponent comp)
            {
                comp.Dispose();
            }
            _entityManager.DestroyEntity(entity);
        }

        // 注销系统
        SystemManager.Instance.UnregisterSystem(_audioSystem);
    }
}