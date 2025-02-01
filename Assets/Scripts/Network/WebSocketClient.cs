//using System;
//using WebSocketSharp;
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using Newtonsoft.Json;

//public class WebSocketClient : MonoBehaviour
//{
//    private WebSocket ws;
//    public TMP_InputField inputField; // 用户输入的文本框（使用 TMP_InputField）
//    public TMP_Text displayText; // 显示对话内容（使用 TMP_Text）
//    private string serverUrl; // 替换为你的 WebSocket 服务地址

//    void Start()
//    {
//        serverUrl = SettingsManager.Instance.GetSetting("WebSocketUrl");
//        ws = new WebSocket(serverUrl);
//        ws.OnOpen += OnOpen;
//        ws.OnMessage += OnMessage;
//        ws.OnError += OnError;
//        ws.OnClose += OnClose;
//        ws.Connect();
//    }

//    private void OnOpen(object sender, EventArgs e)
//    {
//        Debug.Log("WebSocket Connection Established");
//        // 发送初始化消息
//        SendWebSocketMessage(new { type = "fetch-history-list" });
//    }

//    private void OnMessage(object sender, MessageEventArgs e)
//    {
//        Debug.Log("Received: " + e.Data);

//        try
//        {
//            dynamic message = JsonConvert.DeserializeObject(e.Data);
//            string type = message.type;

//            switch (type)
//            {
//                case "full-text":
//                    HandleFullTextMessage(message.text);
//                    break;
//                case "history-list":
//                    HandleHistoryListMessage((string[])message.histories);
//                    break;
//                case "history-data":
//                    HandleHistoryDataMessage((string[])message.messages);
//                    break;
//                case "new-history-created":
//                    HandleNewHistoryCreatedMessage(message.history_uid);
//                    break;
//                case "history-deleted":
//                    HandleHistoryDeletedMessage(message.success, message.history_uid);
//                    break;
//                case "control":
//                    HandleControlMessage(message.text);
//                    break;
//                case "set-model-and-conf":
//                    HandleSetModelAndConfMessage(message.model_info, message.conf_name, message.conf_uid);
//                    break;
//                case "config-files":
//                    HandleConfigFilesMessage((string[])message.configs);
//                    break;
//                case "background-files":
//                    HandleBackgroundFilesMessage((string[])message.files);
//                    break;
//                default:
//                    Debug.LogWarning("Unknown message type received: " + type);
//                    break;
//            }
//        }
//        catch (Exception ex)
//        {
//            Debug.LogError("Error parsing WebSocket message: " + ex.Message);
//        }
//    }

//    private void HandleFullTextMessage(string text)
//    {
//        displayText.text += "\nAI: " + text;
//        Debug.Log("Full Text: " + text);
//    }

//    private void HandleHistoryListMessage(string[] histories)
//    {
//        Debug.Log("History List: " + string.Join(", ", histories));
//        // 处理历史记录列表
//    }

//    private void HandleHistoryDataMessage(string[] messages)
//    {
//        Debug.Log("History Data: " + string.Join(", ", messages));
//        // 处理历史记录数据
//    }

//    private void HandleNewHistoryCreatedMessage(string historyUid)
//    {
//        Debug.Log("New History Created: " + historyUid);
//        // 处理新创建的历史记录
//    }

//    private void HandleHistoryDeletedMessage(bool success, string historyUid)
//    {
//        Debug.Log("History Deleted: " + historyUid + " (Success: " + success + ")");
//        // 处理删除历史记录
//    }

//    private void HandleControlMessage(string text)
//    {
//        Debug.Log("Control Signal: " + text);
//        // 处理控制信号
//    }

//    private void HandleSetModelAndConfMessage(string modelInfo, string confName, string confUid)
//    {
//        Debug.Log("Model Info: " + modelInfo + ", Conf Name: " + confName + ", Conf UID: " + confUid);
//        // 处理模型和配置信息
//    }

//    private void HandleConfigFilesMessage(string[] configFiles)
//    {
//        Debug.Log("Config Files: " + string.Join(", ", configFiles));
//        // 处理配置文件列表
//    }

//    private void HandleBackgroundFilesMessage(string[] backgroundFiles)
//    {
//        Debug.Log("Background Files: " + string.Join(", ", backgroundFiles));
//        // 处理背景文件列表
//    }

//    private void OnError(object sender, ErrorEventArgs e)
//    {
//        Debug.LogError("WebSocket Error: " + e.Message);
//    }

//    private void OnClose(object sender, CloseEventArgs e)
//    {
//        Debug.Log("WebSocket Connection Closed");
//    }

//    public void SendWebSocketMessage(object message)
//    {
//        string jsonMessage = JsonConvert.SerializeObject(message);
//        ws.Send(jsonMessage);
//        Debug.Log("Sent: " + jsonMessage);
//    }

//    public void OnSendButtonClicked()
//    {
//        SendWebSocketMessage(new { type = "text-input", text = inputField.text });
//        inputField.text = ""; // 清空输入框
//    }

//    void OnDestroy()
//    {
//        ws.Close();
//    }
//}