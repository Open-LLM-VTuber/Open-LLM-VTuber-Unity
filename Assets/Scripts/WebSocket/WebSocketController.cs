using TMPro;
using UnityEngine;

public class WebSocketController : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text displayText;

    private WebSocketManager wsManager;
    private HistoryMessageHandler historyHandler;
    private ConfigMessageHandler configHandler;
    private TextMessageHandler textHandler;
    private AudioMessageHandler audioHandler;

    private void Start()
    {
        // 初始 Handlers + UI 组件
        InitializeHandlers();
    }

    private void InitializeHandlers()
    {
        wsManager = WebSocketManager.Instance;
        wsManager.Initialize();

        historyHandler = HistoryMessageHandler.Instance;
        historyHandler.Initialize(wsManager);

        configHandler = ConfigMessageHandler.Instance;
        configHandler.Initialize(wsManager);

        if (displayText != null)
        {
            textHandler = TextMessageHandler.Instance;
            audioHandler = AudioMessageHandler.Instance;
            textHandler.Initialize(wsManager, displayText);
            audioHandler.Initialize(wsManager, displayText);
        }
        
    }

    public void OnRefreshButtonClicked()
    {
        var historyUid = HistoryManager.Instance.GetHistoryUid();
        Debug.LogWarning("historyUid: " + historyUid);
        wsManager.Send(new HistoryCreatedMessage { type = "fetch-and-set-history", history_uid = historyUid });
    }

    public static void Interrupt()
    {
        var wsManager = WebSocketManager.Instance;
        // 假设每次都打断
        wsManager.Send(new TextMessage
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
            wsManager.Send(new TextMessage
            {
                type = "text-input",
                text = inputField.text
            });
            inputField.text = "";
        }
    }
}