using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Handlers/TextMessageHandler.cs
public class TextMessageHandler : InitOnceSingleton<TextMessageHandler>
{
    [SerializeField] private TMP_Text _displayText;

    public void Initialize(WebSocketManager wsManager, TMP_Text displayText)
    {
        InitOnce(() =>
        {
            wsManager.RegisterHandler("full-text", HandleFullText);
            wsManager.RegisterHandler("control", HandleControlText);
        });
        
        if (displayText != null)
        {
            _displayText = displayText;
        }
    }

    private void HandleFullText(WebSocketMessage message)
    {
        var textMsg = message as TextMessage;
        if (_displayText != null)
        {
            _displayText.text = $"\nAI: {textMsg.text}";
        }
        ScrollToBottom();
    }

    private void HandleControlText(WebSocketMessage message)
    {
        // 处理控制文本消息
        var textMsg = message as TextMessage;
        Debug.Log($"Control message received: {textMsg.text}");
    }

    private void ScrollToBottom()
    {
        // 实现滚动到底部逻辑
    }
}