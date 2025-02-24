using UnityEngine;
using TMPro;
using UnityEngine.UI;

// Handlers/TextMessageHandler.cs
public class TextMessageHandler : InitOnceSingleton<TextMessageHandler>
{
    [SerializeField] private TMP_Text _displayText;

    // 状态实例
    private ServerState _serverState = new ();

    public ServerState State => _serverState;

    public void Initialize(WebSocketManager wsManager, GameObject dialogPanel)
    {
        InitOnce(() =>
        {
            wsManager.RegisterHandler("full-text", HandleFullText);
            wsManager.RegisterHandler("control", HandleControlText);
            wsManager.RegisterHandler("backend-synth-complete", HandleBackendSync);
            wsManager.RegisterHandler("force-new-message", HandleForceNewMessage);
        });
        
        _displayText = dialogPanel.transform.Find("FrostedGlass/Content")?.GetComponent<TMP_Text>();
        
    }

    private void HandleFullText(WebSocketMessage message)
    {
        var textMsg = message as TextMessage;
        if (_displayText != null)
        {
            _displayText.text = $"\nAI: {textMsg.text}";
            // 记下最后的回复，用于interrupt-signal
            //HistoryManager.Instance.assistantLastMessage.content += textMsg.text;
        }
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
                if (!State.IsFrontendSynced)
                {
                    WebSocketController.Interrupt();
                }
                State.IsInterrupted = true;

                State.IsFrontendSynced = true;
                State.IsBackendSynced = false;
                // 清空, 下一次继续接收
                HistoryManager.Instance.ClearLastMessage();

            }
            else if (textMsg.text == "mic-audio-end")
            {
                WebSocketManager.Instance.Send(new WebSocketMessage
                {
                    type = "mic-audio-end"
                });
                State.IsInterrupted = false;
            }
        }
    }

    private void HandleBackendSync(WebSocketMessage message)
    {
        State.IsBackendSynced = true;
    }

    private void HandleForceNewMessage(WebSocketMessage message)
    {

    }
}