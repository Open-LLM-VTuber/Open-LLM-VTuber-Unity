//using System;
//using WebSocketSharp;
//using UnityEngine;
//using TMPro;
//using Newtonsoft.Json;
//using System.Collections.Generic;


//public class WebSocketClient : MonoBehaviour
//{
//    public TMP_InputField inputField;
//    public TMP_Text displayText;

//    // Websocket 
//    private WebSocket ws;
//    private string serverUrl;

//    // 对话历史
//    private string history_uid;

//    // 音频消息队列
//    private Queue<AudioMessage> audioMessageQueue = new Queue<AudioMessage>();
//    private bool isAudioPlaying = false;


//    void Start()
//    {
//        serverUrl = SettingsManager.Instance.GetSetting("WebSocketUrl");
//        InitializeDispatcher();
//        InitializeWebSocket();
//        DontDestroyOnLoad(gameObject);
//    }

//    private void InitializeDispatcher()
//    {
//        UnityMainThreadDispatcher.Instance.Enqueue(() => { });
//    }

//    private void InitializeWebSocket()
//    {
//        ws = new WebSocket(serverUrl);

//        ws.OnOpen += (sender, e) =>
//        {
//            Debug.Log("Connected to WebSocket");
//            SendWebSocketMessage(new WebSocketMessage { type = "fetch-history-list" });
//            SendWebSocketMessage(new WebSocketMessage { type = "create-new-history" });
//        };

//        ws.OnMessage += (sender, e) =>
//        {
//            Debug.Log($"Raw message: {e.Data}");
//            HandleReceivedMessage(e.Data);
//        };

//        ws.OnError += (sender, e) =>
//        {
//            Debug.LogError($"WebSocket error: {e.Message}");
//        };

//        ws.OnClose += (sender, e) =>
//        {
//            Debug.Log($"Connection closed: {e.Reason}");
//        };

//        ws.Connect();
//    }

//    private void HandleReceivedMessage(string json)
//    {
//        try
//        {
//            var baseMessage = JsonConvert.DeserializeObject<WebSocketMessage>(json);

//            // 使用字典映射消息类型到对应的处理逻辑
//            var messageHandlers = new Dictionary<string, Action<string>>
//            {
//                { "full-text", HandleMessage<TextMessage>(HandleFullTextMessage) },
//                { "control", HandleMessage<TextMessage>(HandleControlTextMessage) },
//                { "audio", HandleMessage<AudioMessage>(HandleAudioMessage) },
//                { "history-list", HandleMessage<HistoryListMessage>(HandleHistoryListMessage) },
//                { "history-data", HandleMessage<HistoryDataMessage>(HandleHistoryDataMessage) },
//                { "new-history-created", HandleMessage<HistoryCreatedMessage>(HandleNewHistoryCreatedMessage) },
//                { "history-deleted", HandleMessage<HistoryDeletedMessage>(HandleHistoryDeletedMessage) },
//                { "set-model-and-conf", HandleMessage<ModelConfigMessage>(HandleSetModelAndConfMessage) },
//                { "config-files", HandleMessage<ConfigFilesMessage>(HandleConfigFilesMessage) },
//                { "background-files", HandleMessage<BackgroundFilesMessage>(HandleBackgroundFilesMessage) }
//            };


//            if (messageHandlers.TryGetValue(baseMessage.type, out var handler))
//            {
//                handler(json);
//            }
//            else
//            {
//                Debug.LogWarning($"Unknown message type: {baseMessage.type}");
//            }

//        }
//        catch (Exception ex)
//        {
//            Debug.LogError($"Message parsing failed: {ex.Message}\n{ex.StackTrace}");
//        }
//    }

//    private Action<string> HandleMessage<T>(Action<T> handler) where T : WebSocketMessage
//    {
//        return json =>
//        {
//            var message = JsonConvert.DeserializeObject<T>(json);
//            UnityMainThreadDispatcher.Instance.Enqueue(() => handler(message));
//        };
//    }

//    #region 消息处理方法
//    private void HandleFullTextMessage(TextMessage msg)
//    {
//        displayText.text = $"\nAI: {msg.text}";
//        ScrollToBottom();
//    }

//    private void HandleControlTextMessage(TextMessage msg)
//    {

//    }

//    private void HandleAudioMessage(AudioMessage msg)
//    {
//        Debug.Log($"Received audio message with text: {msg.text}");
//        // 将音频消息加入队列
//        audioMessageQueue.Enqueue(msg);
//        // 如果当前没有音频正在播放，则立即播放队列中的第一条消息
//        if (!isAudioPlaying)
//        {
//            PlayNextAudio();
//        }
//    }

//    private void PlayNextAudio()
//    {
//        if (audioMessageQueue.Count > 0)
//        {
//            isAudioPlaying = true;
//            AudioMessage msg = audioMessageQueue.Dequeue();
//            displayText.text = $"\nAI: {msg.text}"; // 先更新文本
//            // 创建时不要立刻播放音频
//            int voiceEntity = AudioManager.Instance.CreateAudioEntityFromBase64(msg.audio, playOnCreate: false);
//            // 用管理器播放音频，并在播放完成后处理下一条消息
//            AudioManager.Instance.PlayAudio(voiceEntity, () =>
//            {
//                isAudioPlaying = false;
//                AudioManager.Instance.RemoveAudio(voiceEntity);
//                PlayNextAudio();
//            });
//        }
//    }

//    private void HandleHistoryListMessage(HistoryListMessage msg)
//    {
//        Debug.Log($"Received {msg.histories.Count} history items");

//        HistoryManager.Instance.SetHistoryList(msg);
//        HistoryListMessage listMsg = HistoryManager.Instance.GetHistoryList();
//        // 遍历消息列表并打印每个历史记录的内容
//        for (int i = 0; i < listMsg.histories.Count; i++)
//        {
//            Debug.Log($"Message {i + 1}: timestamp: {listMsg.histories[i].timestamp}\n " +
//                $"latestMessage: role: {listMsg.histories[i].latest_message.role}" +
//                $" content: {listMsg.histories[i].latest_message.content}");
//        }
//    }

//    private void HandleHistoryDataMessage(HistoryDataMessage msg)
//    {
//        Debug.Log($"Received {msg.messages.Count} history messages");

//        HistoryManager.Instance.SetHistoryData(msg);
//        HistoryDataMessage dataMsg = HistoryManager.Instance.GetHistoryData();
//        // 遍历消息列表并打印每条消息的内容
//        for (int i = 0; i < dataMsg.messages.Count; i++)
//        {
//            Debug.Log($"Message {i + 1}: role: {dataMsg.messages[i].role}\n " +
//                $"content: {dataMsg.messages[i].content}");
//        }
//    }

//    private void HandleNewHistoryCreatedMessage(HistoryCreatedMessage msg)
//    {
//        Debug.Log($"New history created: {msg.history_uid}");
//        history_uid = msg.history_uid;

//    }

//    private void HandleHistoryDeletedMessage(HistoryDeletedMessage msg)
//    {
//        Debug.Log($"History delete status: {msg.success}, UID: {msg.history_uid}");
//    }

//    private void HandleSetModelAndConfMessage(ModelConfigMessage msg)
//    {
//        Debug.Log($"Model config updated: {msg.model_info}, {msg.conf_name}");
//    }

//    private void HandleConfigFilesMessage(ConfigFilesMessage msg)
//    {
//        Debug.Log($"Available configs: {string.Join(", ", msg.configs)}");
//    }

//    private void HandleBackgroundFilesMessage(BackgroundFilesMessage msg)
//    {
//        Debug.Log($"Background files: {string.Join(", ", msg.files)}");
//    }
//    #endregion

//    public void SendWebSocketMessage(WebSocketMessage message)
//    {
//        if (ws?.IsAlive != true)
//        {
//            Debug.LogWarning("Cannot send message - WebSocket not connected");
//            return;
//        }

//        try
//        {
//            string json = JsonConvert.SerializeObject(message);
//            ws.Send(json);
//            Debug.Log($"Sent message: {json}");
//        }
//        catch (Exception ex)
//        {
//            Debug.LogError($"Send failed: {ex.Message}");
//        }
//    }

//    public void FetchHistory()
//    {
//        SendWebSocketMessage(new HistoryCreatedMessage { type = "fetch-and-set-history", history_uid = history_uid });
//    }

//    public void OnSendButtonClicked()
//    {
//        if (!string.IsNullOrEmpty(inputField.text))
//        {
//            SendWebSocketMessage(new TextMessage
//            {
//                type = "text-input",
//                text = inputField.text
//            });
//            inputField.text = "";

//        }
//    }

//    void OnDestroy()
//    {
//        if (ws != null)
//        {
//            if (ws.IsAlive) ws.Close();
//            ws = null;
//        }
//    }

//    private void ScrollToBottom()
//    {
//        // 实现滚动视图自动到底部逻辑
//    }
//}