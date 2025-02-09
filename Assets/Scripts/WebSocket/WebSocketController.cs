using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;
using System.Linq;

public class WebSocketController : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text displayText;
    
    private void Start()
    {
        // 初始 Handlers + UI 组件
        InitializeHandlers();
    }

    private void InitializeHandlers()
    {
        var wsManager = WebSocketManager.Instance;
        wsManager.Initialize();

        var historyHandler = HistoryMessageHandler.Instance;
        var configHandler = ConfigMessageHandler.Instance;
        historyHandler.Initialize(wsManager);
        configHandler.Initialize(wsManager);

        if (displayText != null)
        {
            var textHandler = TextMessageHandler.Instance;
            var audioHandler = AudioMessageHandler.Instance;
            textHandler.Initialize(wsManager, displayText);
            audioHandler.Initialize(wsManager, displayText);
        }
        
    }

    public void OnRefreshButtonClicked()
    {
        var historyUid = HistoryManager.Instance.GetHistoryUid();
        Debug.LogWarning("historyUid: " + historyUid);
        var wsManager = WebSocketManager.Instance;
        wsManager.Send(new HistoryCreatedMessage { type = "fetch-and-set-history", history_uid = historyUid });
    }

    public void OnSendButtonClicked()
    {
        if (!string.IsNullOrEmpty(inputField.text))
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

            var audioHandler = AudioMessageHandler.Instance;
            
            // 清空, 下一次继续接收
            HistoryManager.Instance.assistantLastMessage = string.Empty;

            wsManager.Send(new TextMessage
            {
                type = "text-input",
                text = inputField.text
            });
            inputField.text = "";
        }
    }
}