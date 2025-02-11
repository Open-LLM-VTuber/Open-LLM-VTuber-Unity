using UnityEngine;
using TMPro;

// Handlers/TextMessageHandler.cs
public class TextMessageHandler : InitOnceSingleton<TextMessageHandler>
{
    [SerializeField] private TMP_Text _displayText;

    // 状态实例
    private readonly ServerState _serverState = new ();

    // 提供对状态的只读访问
    public ServerState State => _serverState;

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
            // 记下最后的回复，用于interrupt-signal
            HistoryManager.Instance.assistantLastMessage += textMsg.text;
        }
        ScrollToBottom();
    }

    private void HandleControlText(WebSocketMessage message)
    {
        // 处理控制文本消息
        var textMsg = message as TextMessage;
        Debug.Log($"Control message received: {textMsg.text}");
        if (textMsg != null)
        {
            if (textMsg.text == "interrupt")
            {
                WebSocketController.Interrupt();
                _serverState.SetInterrupted(true);
            }
            else if (textMsg.text == "mic-audio-end")
            {
                WebSocketManager.Instance.Send(new WebSocketMessage
                {
                    type = "mic-audio-end"
                });
                _serverState.SetInterrupted(false);
            }
            else if (textMsg.text == "allow-unity-audio")
            {
                _serverState.SetAllowUnityAudio(true);
            }
            else if (textMsg.text == "reject-unity-audio")
            {
                _serverState.SetAllowUnityAudio(false);
            }
        }
    }

    private void ScrollToBottom()
    {
        // 实现滚动到底部逻辑
    }
}