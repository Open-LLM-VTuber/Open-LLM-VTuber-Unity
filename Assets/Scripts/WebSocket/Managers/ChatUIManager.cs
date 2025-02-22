using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

public class ChatUIManager : MonoBehaviour
{
    public GameObject chatBubbleRight; // 人类聊天预制体
    public GameObject chatBubbleLeft;    // AI聊天预制体
    public Transform parentObject;      // 父对象
    public RectTransform scrollViewport;

    void Start()
    {
        // 保证能实时更新
        if (HistoryManager.Instance != null)
        {
            HistoryManager.Instance.OnHistoryDataUpdated += UpdateChatBubbles;
        }
        ClearParentObjectChildren();
    }

    private void OnDestroy()
    {
        // 取消订阅事件，避免内存泄漏
        if (HistoryManager.Instance != null)
        {
            HistoryManager.Instance.OnHistoryDataUpdated -= UpdateChatBubbles;
        }
    }

    public static void RefreshHistoryData()
    {
        var historyUid = HistoryManager.Instance.HistoryUid;
        Debug.LogWarning("historyUid: " + historyUid);
        WebSocketManager.Instance.Send(new HistoryCreatedMessage
        { type = "fetch-and-set-history", history_uid = historyUid });
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
        var historyMessages = HistoryManager.Instance.HistoryData;
        var baseUrl = SettingsManager.Instance.GetSetting("General.BaseUrl");

        // 处理所有消息
        foreach (var message in historyMessages.messages.Concat(GetLastAssistantMessage(historyMessages)))
        {
            GameObject prefab = message.role == "human" ? chatBubbleRight : chatBubbleLeft;
            GameObject chatObject = Instantiate(prefab, parentObject);

            var charContent = chatObject.GetComponent<ChatContent>();
            charContent.SetName(message.name);
            charContent.SetTime(message.timestamp);
            charContent.SetContent(message.content);

            var avatarManager = chatObject.GetComponent<AvatarManager>();
            if (!string.IsNullOrEmpty(message.avatar))
            {
                string avatarUrl = new UriBuilder(baseUrl) { Path = $"avatars/{message.avatar}" }.ToString();
                AvatarManager.AddOrUpdateAvatarUrl(message.name, avatarUrl);
                avatarManager.SetAvatarByName(message.name);
            }
        }

        Canvas.ForceUpdateCanvases();
    }

    // 提取最后一个助手消息的逻辑
    private IEnumerable<HistoryDataItem> GetLastAssistantMessage(HistoryDataMessage historyMessages)
    {
        var lastMsg = HistoryManager.Instance.assistantLastMessage;
        if (historyMessages.messages.Count > 0 &&
            historyMessages.messages.Last().role == "human" &&
            !string.IsNullOrEmpty(lastMsg.content))
        {
            yield return new HistoryDataItem
            {
                role = "ai",
                timestamp = DateTime.Now,
                content = lastMsg.content,
                name = lastMsg.name,
                avatar = lastMsg.avatar
            };
        }
    }

}
