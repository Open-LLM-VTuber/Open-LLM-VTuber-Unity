using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;

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
            wsManager.Send(new TextMessage
            {
                type = "text-input",
                text = inputField.text
            });
            inputField.text = "";
        }
    }
}