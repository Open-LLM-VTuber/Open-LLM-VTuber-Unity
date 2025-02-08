using ECS;
using UnityEngine;

public class InitLoader : MonoBehaviour
{
    void Start()
    {
        // 检查是否已经存在 Global Settings 实例
        if (FindObjectOfType<SettingsManager>() == null)
        {
            // 创建 Global Settings GameObject
            GameObject globalSettings = new GameObject("Global Settings");

            // 挂载所有 Manager 组件
            globalSettings.AddComponent<FPS>();
            globalSettings.AddComponent<SystemScale>();
            globalSettings.AddComponent<SettingsManager>();
            globalSettings.AddComponent<UnityMainThreadDispatcher>();

            globalSettings.AddComponent<EntityManager>();
            globalSettings.AddComponent<ComponentManager>();
            globalSettings.AddComponent<SystemManager>();

            globalSettings.AddComponent<HistoryManager>();
            globalSettings.AddComponent<AudioManager>();

            // 添加其他 Manager...

            // 设置为 DontDestroyOnLoad
            DontDestroyOnLoad(globalSettings);
        }
        else
        {
            Debug.LogWarning("Global Settings already exists. Skipping instantiation.");
        }

        // 初始化完成后销毁 InitLoader 对象
        Destroy(gameObject);
    }
}