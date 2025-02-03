using System;
using WebSocketSharp;
using UnityEngine;
using TMPro;
using Newtonsoft.Json;
using System.Collections.Generic;

public class WebSocketClient : MonoBehaviour
{
    #region 自定义数据结构
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
    public class AudioMessage : WebSocketMessage
    {
        public string audio; // base64 编码的音频数据
        public float[] volumes; // 音量数据
        public int slice_length; // 音频切片长度（毫秒）
        public string text; // 显示的文本
        public Dictionary<string, List<string>> actions; // 动作配置
    }

    [Serializable]
    public class HistoryListMessage : WebSocketMessage
    {
        public List<HistoryItem> histories;
    }

    [Serializable]
    public class HistoryItem
    {
        public string uid;
        public string latestMessage;
        [JsonProperty("timestamp")]
        public DateTime? timestamp;
    }

    [Serializable]
    public class HistoryDataMessage : WebSocketMessage
    {
        public List<string> messages;
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
        public ModelInfo model_info;
        public string conf_name;
        public string conf_uid;
    }

    [Serializable]
    public class ModelInfo
    {
        public string name;
        public string description;
        public string url;
        public float kScale;
        public int initialXshift;
        public int initialYshift;
        public int kXOffset;
        public string idleMotionGroupName;
        public Dictionary<string, int> emotionMap;
        public Dictionary<string, Dictionary<string, int>> tapMotions;
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
    #endregion

    private WebSocket ws;
    public TMP_InputField inputField;
    public TMP_Text displayText;
    private string serverUrl;

    void Start()
    {
        serverUrl = SettingsManager.Instance.GetSetting("WebSocketUrl");
        InitializeDispatcher();
        InitializeWebSocket();
    }

    private void InitializeDispatcher()
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() => { });
    }

    private void InitializeWebSocket()
    {
        ws = new WebSocket(serverUrl);

        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("Connected to WebSocket");
            SendWebSocketMessage(new WebSocketMessage { type = "fetch-history-list" });
            SendWebSocketMessage(new WebSocketMessage { type = "create-new-history" });
        };

        ws.OnMessage += (sender, e) =>
        {
            Debug.Log($"Raw message: {e.Data}");
            HandleReceivedMessage(e.Data);
        };

        ws.OnError += (sender, e) =>
        {
            Debug.LogError($"WebSocket error: {e.Message}");
        };

        ws.OnClose += (sender, e) =>
        {
            Debug.Log($"Connection closed: {e.Reason}");
        };

        ws.Connect();
    }

    private void HandleReceivedMessage(string json)
    {
        try
        {
            var baseMessage = JsonConvert.DeserializeObject<WebSocketMessage>(json);

            // 使用字典映射消息类型到对应的处理逻辑
            var messageHandlers = new Dictionary<string, Action<string>>
            {
                { "full-text", HandleMessage<TextMessage>(HandleFullTextMessage) },
                { "control", HandleMessage<TextMessage>(HandleFullTextMessage) },
                { "audio", HandleMessage<AudioMessage>(HandleAudioMessage) },
                { "history-list", HandleMessage<HistoryListMessage>(HandleHistoryListMessage) },
                { "history-data", HandleMessage<HistoryDataMessage>(HandleHistoryDataMessage) },
                { "new-history-created", HandleMessage<HistoryCreatedMessage>(HandleNewHistoryCreatedMessage) },
                { "history-deleted", HandleMessage<HistoryDeletedMessage>(HandleHistoryDeletedMessage) },
                { "set-model-and-conf", HandleMessage<ModelConfigMessage>(HandleSetModelAndConfMessage) },
                { "config-files", HandleMessage<ConfigFilesMessage>(HandleConfigFilesMessage) },
                { "background-files", HandleMessage<BackgroundFilesMessage>(HandleBackgroundFilesMessage) }
            };

            if (messageHandlers.TryGetValue(baseMessage.type, out var handler))
            {
                handler(json);
            }
            else
            {
                Debug.LogWarning($"Unknown message type: {baseMessage.type}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Message parsing failed: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private Action<string> HandleMessage<T>(Action<T> handler) where T : WebSocketMessage
    {
        return json =>
        {
            var message = JsonConvert.DeserializeObject<T>(json);
            UnityMainThreadDispatcher.Instance.Enqueue(() => handler(message));
        };
    }

    #region 消息处理方法
    private void HandleFullTextMessage(TextMessage msg)
    {
        displayText.text = $"\nAI: {msg.text}";
        ScrollToBottom();
    }

    private void HandleAudioMessage(AudioMessage msg)
    {
        Debug.Log($"Received audio message with text: {msg.text}");
        displayText.text = $"\nAI: {msg.text}";
        Debug.LogWarning($"Audio is to play!");
        int voiceEntity = AudioManager.Instance.CreateAudioEntityFromBase64(msg.audio);
        AudioManager.Instance.PlayAudio(voiceEntity);
        Debug.LogWarning($"Audio is end!");
        ScrollToBottom();
    }

    private void HandleHistoryListMessage(HistoryListMessage msg)
    {
        Debug.Log($"Received {msg.histories.Count} history items");
    }

    private void HandleHistoryDataMessage(HistoryDataMessage msg)
    {
        Debug.Log($"Received {msg.messages.Count} history messages");
    }

    private void HandleNewHistoryCreatedMessage(HistoryCreatedMessage msg)
    {
        Debug.Log($"New history created: {msg.history_uid}");
    }

    private void HandleHistoryDeletedMessage(HistoryDeletedMessage msg)
    {
        Debug.Log($"History delete status: {msg.success}, UID: {msg.history_uid}");
    }

    private void HandleSetModelAndConfMessage(ModelConfigMessage msg)
    {
        Debug.Log($"Model config updated: {msg.model_info}, {msg.conf_name}");
    }

    private void HandleConfigFilesMessage(ConfigFilesMessage msg)
    {
        Debug.Log($"Available configs: {string.Join(", ", msg.configs)}");
    }

    private void HandleBackgroundFilesMessage(BackgroundFilesMessage msg)
    {
        Debug.Log($"Background files: {string.Join(", ", msg.files)}");
    }
    #endregion

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

    private void ScrollToBottom()
    {
        // 实现滚动视图自动到底部逻辑
    }
}