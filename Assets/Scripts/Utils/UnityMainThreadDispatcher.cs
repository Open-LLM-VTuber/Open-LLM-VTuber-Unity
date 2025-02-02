using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private readonly object lockObject = new object();
    private readonly Queue<Action> actions = new Queue<Action>();

    // 使用 Start 而不是 Awake 避免不必要的初始化
    public static UnityMainThreadDispatcher Instance { get; private set; }

    private void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 确保场景切换时不被销毁
        }
        else
        {
            Destroy(gameObject); // 如果已存在实例，销毁新的实例
        }
    }

    private void Update()
    {
        lock (lockObject)
        {
            while (actions.Count > 0)
            {
                actions.Dequeue().Invoke();
            }
        }
    }

    public void Enqueue(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action), "Action cannot be null.");
        }

        lock (lockObject)
        {
            actions.Enqueue(action);
        }
    }
}