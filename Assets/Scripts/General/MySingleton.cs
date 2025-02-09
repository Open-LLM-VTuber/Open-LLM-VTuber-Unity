using UnityEngine;
using System; // 需要引入 System 命名空间以使用 Lazy<T>

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
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