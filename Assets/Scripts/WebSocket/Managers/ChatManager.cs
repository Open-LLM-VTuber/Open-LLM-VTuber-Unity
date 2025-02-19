using UnityEngine;
using Newtonsoft.Json;

public class ChatManager : MonoBehaviour
{
    public GameObject chatBubbleRight; // 人类聊天预制体
    public GameObject chatBubbleLeft;    // AI聊天预制体
    public Transform parentObject;      // 父对象
    public RectTransform scrollViewport;

    void Start()
    {
        // 订阅历史记录更新事件
        if (HistoryManager.Instance != null)
        {
            HistoryManager.Instance.OnHistoryUpdated += HandleHistoryUpdated;
        }
    }

    private void OnDestroy()
    {
        // 取消订阅事件，避免内存泄漏
        if (HistoryManager.Instance != null)
        {
            HistoryManager.Instance.OnHistoryUpdated -= HandleHistoryUpdated;
        }
    }

    // 事件处理函数
    private void HandleHistoryUpdated(HistoryDataMessage historyData)
    {
        UpdateChatBubbles();
    }

    public void UpdateChatBubbles()
    {
        ClearParentObjectChildren();
        DisplayChatMessages();
    }

    private void ClearParentObjectChildren()
    {
        if (parentObject != null)
        {
            foreach (Transform child in parentObject)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void DisplayChatMessages()
    {
        var historyMessages = HistoryManager.Instance.GetHistoryData();
        Debug.Log(JsonConvert.SerializeObject(historyMessages));
        foreach (var message in historyMessages.messages)
        {
            GameObject prefab = message.role == "human" ? chatBubbleRight : chatBubbleLeft;
            GameObject chatObject = Instantiate(prefab, parentObject);
            chatObject.GetComponent<ChatContent>().SetContent(message.content);
        }

        var count = historyMessages.messages.Count;
        if (count > 0)
        {
            var lastMsg = historyMessages.messages[count - 1];
            if (lastMsg.role == "human")
            {
                GameObject chatObject = Instantiate(chatBubbleLeft, parentObject);
                var content = HistoryManager.Instance.assistantLastMessage;
                chatObject.GetComponent<ChatContent>().SetContent(content);
            }
        }
        Canvas.ForceUpdateCanvases();
    }
}
