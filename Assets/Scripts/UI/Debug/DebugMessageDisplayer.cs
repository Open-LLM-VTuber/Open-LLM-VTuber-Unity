using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class DebugMessageDisplayer : MonoBehaviour
{
    [SerializeField] private GameObject messageBoxPrefab;
    [SerializeField] private Transform messageContainer;
    [SerializeField] private int poolSize = 5; // 保留对象池的设计，留作滚动显示
    [SerializeField] private float displayTime = 3f; // 每条消息显示时间

    private Queue<GameObject> messagePool = new Queue<GameObject>(); // 对象池
    private Queue<(string message, string color)> messageQueue = new Queue<(string, string)>(); // 消息队列
    private GameObject currentMessageBox; // 当前显示的消息
    
    private float lastDisplayTime; // 上次切换时间

    public int QueueCount => messageQueue.Count;

    void Awake()
    {
        DebugWrapper.Instance.RegisterDisplayer(this);
        if (messageContainer == null)
            messageContainer = transform;

        InitializePool();
    }

    void Start()
    {
        if (!WebSocketManager.Instance.CheckConnectionStatus())
            WebSocketManager.Instance.StartCheckingConnection();
    }

    void OnDestroy()
    {
        // Unregister this displayer from DebugWrapper
        if (DebugWrapper.Instance != null)
        {
            DebugWrapper.Instance.UnregisterDisplayer(this);
        }

        // Clean up the current message box if it exists
        if (currentMessageBox != null)
        {
            var controller = currentMessageBox.GetComponent<MessageBoxController>();
            if (controller != null)
            {
                controller.OnSkip -= GoToNextMessage;
            }
        }

        // Clean up all pooled message boxes
        while (messagePool.Count > 0)
        {
            GameObject messageBox = messagePool.Dequeue();
            if (messageBox != null)
            {
                var controller = messageBox.GetComponent<MessageBoxController>();
                if (controller != null)
                {
                    controller.OnSkip -= GoToNextMessage;
                }
                Destroy(messageBox);
            }
        }

        // Clear the queues
        messagePool.Clear();
        messageQueue.Clear();
    }
    
    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject messageBox = Instantiate(messageBoxPrefab, messageContainer);
            messageBox.SetActive(false);
            messagePool.Enqueue(messageBox);
        }
    }

    public void EnqueueMessage(string message, string color)
    {
        messageQueue.Enqueue((message, color)); // 加入队列
        if (currentMessageBox == null) // 如果当前没有消息，立即显示
            ShowNextMessage();
        else
            UpdateCurrentMessageCount();
    }

    private void Update()
    {
        if (Time.time - lastDisplayTime >= displayTime)
        {
            GoToNextMessage();
        }
    }

    private void ShowNextMessage()
    {
        if (messageQueue.Count > 0)
        {
            var (message, color) = messageQueue.Dequeue();
            currentMessageBox = messagePool.Count > 0 ? messagePool.Dequeue() : Instantiate(messageBoxPrefab, messageContainer);
            
            // currentMessageBox.SetActive(true);
            var fader = currentMessageBox.GetComponent<FadeAnimation>();
            fader.FadeIn();
            
            var controller = currentMessageBox.GetComponent<MessageBoxController>();
            if (controller != null)
            {
                controller.SetMessage($"<color={color}>{message}</color>", QueueCount);
                controller.OnSkip += GoToNextMessage; // 注册跳过事件
            }
            
            lastDisplayTime = Time.time;
        }
    }

    private void GoToNextMessage()
    {
        if (currentMessageBox != null)
        {
            ReturnToPool(currentMessageBox);
            currentMessageBox = null;
            ShowNextMessage(); // 立即显示下一条
        }
    }

    private void UpdateCurrentMessageCount()
    {
        if (currentMessageBox != null)
        {
            var controller = currentMessageBox.GetComponent<MessageBoxController>();
            if (controller != null)
                controller.UpdateQueueCount(QueueCount);

        }
    }

    private void ReturnToPool(GameObject messageBox)
    {
        var controller = messageBox.GetComponent<MessageBoxController>();
        if (controller != null)
            controller.OnSkip -= GoToNextMessage;
        // currentMessageBox.SetActive(false);
        var fader = currentMessageBox.GetComponent<FadeAnimation>();
        fader.FadeOut();
        
        messagePool.Enqueue(messageBox);
    }
}