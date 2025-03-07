using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
public class MessageBoxController : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText; // 如果用 Text 组件
    [SerializeField] private TMP_Text queueCountText; // 显示队列计数
    [SerializeField] private Button skipButton; // 跳过按钮

    public Action OnSkip; // 跳过事件

    private void Awake()
    {
        if (skipButton != null)
            skipButton.onClick.AddListener(() => OnSkip?.Invoke());
    }

    private void OnDestroy()
    {
        if (skipButton != null)
            skipButton.onClick.RemoveAllListeners();
    }

    public void SetMessage(string message, int queueCount)
    {
        if (messageText != null)
        {
            string formattedMessage = FormatMessage(message);
            messageText.text = formattedMessage;
        }
        UpdateQueueCount(queueCount);
    }

    public void UpdateQueueCount(int queueCount)
    {
        if (queueCountText != null)
            queueCountText.text = $"left: {queueCount}";
    }

    private string FormatMessage(string message)
    {
        if (!message.Contains("<color"))
            message = $"<color=white>{message}</color>";
        if (message.Contains("[b]"))
            message = message.Replace("[b]", "<b>").Replace("[/b]", "</b>");
        if (message.Contains("[i]"))
            message = message.Replace("[i]", "<i>").Replace("[/i]", "</i>");
        if (message.Contains("[size]"))
            message = message.Replace("[size]", "<size=20>").Replace("[/size]", "</size>");
        return message;
    }
}