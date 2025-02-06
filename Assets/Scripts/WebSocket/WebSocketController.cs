using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WebSocketController : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text displayText;

    private WebSocketManager wsManager;
    private AudioMessageHandler audioHandler;
    private TextMessageHandler textHandler;
    private HistoryMessageHandler historyHandler;
    private ConfigMessageHandler configHandler;

    private void Start()
    {
        var url = SettingsManager.Instance.GetSetting("WebSocketUrl");

        // 初始化WebSocket管理器
        wsManager = gameObject.AddComponent<WebSocketManager>();
        wsManager.Initialize(url);

        // 初始化各消息处理器
        InitializeHandlers();

        // 初始请求
        RequestInitialData();
    }

    private void InitializeHandlers()
    {
        // 文本消息处理器
        textHandler = gameObject.AddComponent<TextMessageHandler>();
        textHandler.Initialize(wsManager, displayText);

        // 音频消息处理器
        audioHandler = gameObject.AddComponent<AudioMessageHandler>();
        audioHandler.Initialize(wsManager, displayText);

        // 历史记录处理器
        historyHandler = gameObject.AddComponent<HistoryMessageHandler>();
        historyHandler.Initialize(wsManager);
        
        // 配置处理器
        configHandler = gameObject.AddComponent<ConfigMessageHandler>();
        configHandler.Initialize(wsManager);
    }

    private void RequestInitialData()
    {
        wsManager.Send(new WebSocketMessage { type = "fetch-history-list" });
        // 一开始初始化一次，之后不随场景切换而重新初始化
        if (!HistoryManager.Instance.initialized)
        {
            HistoryManager.Instance.initialized = true;
            wsManager.Send(new WebSocketMessage { type = "create-new-history" });
        }
        wsManager.Send(new WebSocketMessage { type = "get-config-files" });
        wsManager.Send(new WebSocketMessage { type = "get-background-files" });
        
    }

    public void OnRefreshButtonClicked()
    {
        var historyUid = HistoryManager.Instance.GetHistoryUid();
        Debug.LogWarning("historyUid: " + historyUid);
        wsManager.Send(new HistoryCreatedMessage { type = "fetch-and-set-history", history_uid = historyUid });
    }

    public void OnSendButtonClicked()
    {
        if (!string.IsNullOrEmpty(inputField.text))
        {
            wsManager.Send(new TextMessage
            {
                type = "text-input",
                text = inputField.text
            });
            inputField.text = "";
        }
    }
}