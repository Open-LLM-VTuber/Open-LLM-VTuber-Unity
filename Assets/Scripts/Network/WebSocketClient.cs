using System;
using WebSocketSharp;
using UnityEngine;
using TMPro;
using Newtonsoft.Json;
using System.Collections.Generic;

public class WebSocketClient : MonoBehaviour
{
    // region 自定义数据结构
    [Serializable]
    public class WebSocketMessage
    {
        public string type;
    }

    [Serializable]
    public class TextMessage : WebSocketMessage
    {
        public string text;
    }

    [Serializable]
    public class HistoryListMessage : WebSocketMessage
    {
        public string[] histories;
    }

    [Serializable]
    public class HistoryDataMessage : WebSocketMessage
    {
        public string[] messages;
    }

    [Serializable]
    public class HistoryCreatedMessage : WebSocketMessage
    {
        public string history_uid;
    }

    [Serializable]
    public class HistoryDeletedMessage : WebSocketMessage
    {
        public bool success;
        public string history_uid;
    }

    [Serializable]
    public class ModelConfigMessage : WebSocketMessage
    {
        public string model_info;
        public string conf_name;
        public string conf_uid;
    }

    [Serializable]
    public class ConfigFilesMessage : WebSocketMessage
    {
        public string[] configs;
    }

    [Serializable]
    public class BackgroundFilesMessage : WebSocketMessage
    {
        public string[] files;
    }
    // endregion

    private WebSocket ws;
    public TMP_InputField inputField;
    public TMP_Text displayText;
    private string serverUrl;

    void Start()
    {
        serverUrl = SettingsManager.Instance.GetSetting("WebSocketUrl");
        InitializeWebSocket();
    }

    private void InitializeWebSocket()
    {
        ws = new WebSocket(serverUrl);

        // 处理SSL证书验证（测试环境使用）
        ws.SslConfiguration.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

        ws.OnOpen += (sender, e) => {
            Debug.Log("Connected to WebSocket");
            SendWebSocketMessage(new WebSocketMessage { type = "fetch-history-list" });
        };

        ws.OnMessage += (sender, e) => {
            Debug.Log($"Raw message: {e.Data}");
            HandleReceivedMessage(e.Data);
        };

        ws.OnError += (sender, e) => {
            Debug.LogError($"WebSocket error: {e.Message}");
        };

        ws.OnClose += (sender, e) => {
            Debug.Log($"Connection closed: {e.Reason}");
        };

        ws.Connect();
    }

    private void HandleReceivedMessage(string json)
    {
        try
        {
            // 第一步：解析基础消息类型
            var baseMessage = JsonConvert.DeserializeObject<WebSocketMessage>(json);

            // 第二步：根据类型进行完整解析
            switch (baseMessage.type)
            {
                case "full-text":
                    var textMsg = JsonConvert.DeserializeObject<TextMessage>(json);
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        HandleFullTextMessage(textMsg));
                    break;

                case "history-list":
                    var historyListMsg = JsonConvert.DeserializeObject<HistoryListMessage>(json);
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        HandleHistoryListMessage(historyListMsg));
                    break;

                case "history-data":
                    var historyDataMsg = JsonConvert.DeserializeObject<HistoryDataMessage>(json);
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        HandleHistoryDataMessage(historyDataMsg));
                    break;

                case "new-history-created":
                    var historyCreatedMsg = JsonConvert.DeserializeObject<HistoryCreatedMessage>(json);
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        HandleNewHistoryCreatedMessage(historyCreatedMsg));
                    break;

                case "history-deleted":
                    var historyDeletedMsg = JsonConvert.DeserializeObject<HistoryDeletedMessage>(json);
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        HandleHistoryDeletedMessage(historyDeletedMsg));
                    break;

                case "set-model-and-conf":
                    var modelConfigMsg = JsonConvert.DeserializeObject<ModelConfigMessage>(json);
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        HandleSetModelAndConfMessage(modelConfigMsg));
                    break;

                case "config-files":
                    var configFilesMsg = JsonConvert.DeserializeObject<ConfigFilesMessage>(json);
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        HandleConfigFilesMessage(configFilesMsg));
                    break;

                case "background-files":
                    var bgFilesMsg = JsonConvert.DeserializeObject<BackgroundFilesMessage>(json);
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        HandleBackgroundFilesMessage(bgFilesMsg));
                    break;

                default:
                    Debug.LogWarning($"Unknown message type: {baseMessage.type}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Message parsing failed: {ex.Message}\n{ex.StackTrace}");
        }
    }

    // region 消息处理方法
    private void HandleFullTextMessage(TextMessage msg)
    {
        displayText.text += $"\nAI: {msg.text}";
        ScrollToBottom(); // 假设有自动滚动到底部的方法
    }

    private void HandleHistoryListMessage(HistoryListMessage msg)
    {
        Debug.Log($"Received {msg.histories.Length} history items");
        // 更新历史记录列表UI
    }

    private void HandleHistoryDataMessage(HistoryDataMessage msg)
    {
        Debug.Log($"Received {msg.messages.Length} history messages");
        // 加载历史消息到对话界面
    }

    private void HandleNewHistoryCreatedMessage(HistoryCreatedMessage msg)
    {
        Debug.Log($"New history created: {msg.history_uid}");
        // 刷新历史记录列表
    }

    private void HandleHistoryDeletedMessage(HistoryDeletedMessage msg)
    {
        Debug.Log($"History delete status: {msg.success}, UID: {msg.history_uid}");
        // 更新UI状态
    }

    private void HandleSetModelAndConfMessage(ModelConfigMessage msg)
    {
        Debug.Log($"Model config updated: {msg.model_info}, {msg.conf_name}");
        // 更新模型配置显示
    }

    private void HandleConfigFilesMessage(ConfigFilesMessage msg)
    {
        Debug.Log($"Available configs: {string.Join(", ", msg.configs)}");
        // 更新配置下拉菜单
    }

    private void HandleBackgroundFilesMessage(BackgroundFilesMessage msg)
    {
        Debug.Log($"Background files: {string.Join(", ", msg.files)}");
        // 更新背景选择面板
    }
    // endregion

    public void SendWebSocketMessage(WebSocketMessage message)
    {
        if (ws?.IsAlive != true)
        {
            Debug.LogWarning("Cannot send message - WebSocket not connected");
            return;
        }

        try
        {
            string json = JsonConvert.SerializeObject(message);
            ws.Send(json);
            Debug.Log($"Sent message: {json}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Send failed: {ex.Message}");
        }
    }

    public void OnSendButtonClicked()
    {
        if (!string.IsNullOrEmpty(inputField.text))
        {
            SendWebSocketMessage(new TextMessage
            {
                type = "text-input",
                text = inputField.text
            });
            inputField.text = "";
        }
    }

    void OnDestroy()
    {
        if (ws != null)
        {
            if (ws.IsAlive) ws.Close();
            ws = null;
        }
    }

    // 其他辅助方法
    private void ScrollToBottom()
    {
        // 实现滚动视图自动到底部逻辑
    }
}