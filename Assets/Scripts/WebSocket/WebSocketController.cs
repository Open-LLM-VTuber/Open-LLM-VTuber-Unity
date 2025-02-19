using TMPro;
using UnityEngine;

public class WebSocketController : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject dialogPanel;

    private void Start()
    {
        // 初始 Handlers + UI 组件
        InitializeHandlers();
    }

    private void InitializeHandlers()
    {
        var wsManager = WebSocketManager.Instance;
        wsManager.Initialize();

        HistoryMessageHandler.Instance.Initialize(wsManager);
        ConfigMessageHandler.Instance.Initialize(wsManager);

        if (dialogPanel != null)
        {
            TextMessageHandler.Instance.Initialize(wsManager, dialogPanel);
            AudioMessageHandler.Instance.Initialize(wsManager, dialogPanel);
        }
        
    }

    public static void OnRefreshButtonClicked()
    {
        var historyUid = HistoryManager.Instance.GetHistoryUid();
        Debug.LogWarning("historyUid: " + historyUid);
        WebSocketManager.Instance.Send(new HistoryCreatedMessage { type = "fetch-and-set-history", history_uid = historyUid });
    }

    public static void Interrupt()
    {
        // 假设每次都打断
        WebSocketManager.Instance.Send(new TextMessage
        {
            type = "interrupt-signal",
            text = HistoryManager.Instance.assistantLastMessage
        });

        // 停止所有AI音频播放
        AudioMessageHandler.Instance.ClearAudioQueue();
        AudioManager.Instance.RemoveAllAudioByType(ECS.AudioType.AssistantVoice);

        // 清空, 下一次继续接收
        HistoryManager.Instance.assistantLastMessage = string.Empty;
    }

    public void OnSendButtonClicked()
    {
        if (!string.IsNullOrEmpty(inputField.text))
        {
            Interrupt();
            WebSocketManager.Instance.Send(new TextMessage
            {
                type = "text-input",
                text = inputField.text
            });
            inputField.text = "";
        }
    }
}