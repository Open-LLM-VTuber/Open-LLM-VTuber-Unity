using UnityEngine;
using System; // 需要引入 System 命名空间以使用 Lazy<T>

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static readonly Lazy<T> _lazyInstance = new Lazy<T>(CreateSingleton);

    public static T Instance => _lazyInstance.Value;

    private static T CreateSingleton()
    {
        // 查找场景中是否已经存在实例
        var existingInstance = FindObjectOfType<T>();

        if (existingInstance != null)
        {
            return existingInstance;
        }
        Debug.LogWarning($"{typeof(T).Name}");
        // 如果不存在，则创建一个新的 GameObject 并附加组件
        var singletonObject = new GameObject(typeof(T).Name);
        var instance = singletonObject.AddComponent<T>();
        DontDestroyOnLoad(singletonObject); // 确保在场景切换时不销毁

        return instance;
    }

    protected virtual void Awake()
    {
        // 确保实例唯一
        if (_lazyInstance.Value != this as T)
        {
            Destroy(gameObject);
        }
    }
}


public abstract class InitOnceSingleton<T> : Singleton<T> where T : MonoBehaviour
{
    private bool _isInitialized = false; // 标志位，记录是否已经初始化

    /// <summary>
    /// 确保初始化逻辑只会执行一次
    /// </summary>
    protected void InitOnce(Action initAction)
    {
        if (_isInitialized)
        {
            //Debug.LogWarning($"{typeof(T).Name} has already been initialized.");
            return;
        }

        // 执行初始化逻辑
        initAction?.Invoke();

        // 标记为已初始化
        _isInitialized = true;
        Debug.Log($"{typeof(T).Name} initialized successfully.");
    }
}
