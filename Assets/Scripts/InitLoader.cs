using ECS;
using UnityEngine;

public class InitLoader : MonoBehaviour
{

    void Start()
    {
        // 检查是否已经存在 Global Settings 实例
        if (FindObjectOfType<SettingsManager>() == null)
        {
            GameObject globalSettings = new GameObject("Global Settings");
            globalSettings.AddComponent<FPS>();
            globalSettings.AddComponent<SystemScale>();
            globalSettings.AddComponent<SettingsManager>();
            globalSettings.AddComponent<UnityMainThreadDispatcher>();

            GameObject globalManagers = new GameObject("Global Managers");
            globalManagers.AddComponent<EntityManager>();
            globalManagers.AddComponent<ComponentManager>();
            globalManagers.AddComponent<SystemManager>();
            globalManagers.AddComponent<HistoryManager>();
            globalManagers.AddComponent<AudioManager>();
            globalManagers.AddComponent<WebSocketManager>();
            globalManagers.AddComponent<SceneTransitionManager>();

            GameObject globalHandlers = new GameObject("Global Handlers");
            globalHandlers.AddComponent<TextMessageHandler>();
            globalHandlers.AddComponent<AudioMessageHandler>();
            globalHandlers.AddComponent<HistoryMessageHandler>();
            globalHandlers.AddComponent<ConfigMessageHandler>();

            // 设置为 DontDestroyOnLoad
            DontDestroyOnLoad(globalSettings);
            DontDestroyOnLoad(globalManagers);
            DontDestroyOnLoad(globalHandlers);
        }
        else
        {
            Debug.LogWarning("Global Settings already exists. Skipping instantiation.");
        }

        // 初始化完成后销毁 InitLoader 对象
        Destroy(gameObject);
    }
}