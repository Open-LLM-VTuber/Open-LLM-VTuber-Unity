using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class WebSocketClient : MonoBehaviour
{
    private WebSocket ws;

    // UI元素
    public InputField serverAddressInput; // 用于输入服务器地址
    public Button connectButton; // 连接按钮
    public Text statusText; // 显示连接状态

    void Start()
    {
        // 绑定连接按钮的点击事件
        connectButton.onClick.AddListener(ConnectToServer);
    }

    void ConnectToServer()
    {
        // 获取用户输入的服务器地址
        string serverAddress = serverAddressInput.text;

        if (string.IsNullOrEmpty(serverAddress))
        {
            statusText.text = "Please enter a server address!";
            return;
        }

        // 连接到WebSocket服务器
        ws = new WebSocket(serverAddress);

        // 设置事件处理程序
        ws.OnOpen += OnOpen;
        ws.OnMessage += OnMessage;
        ws.OnError += OnError;
        ws.OnClose += OnClose;

        // 连接
        ws.Connect();
    }

    void OnOpen(object sender, System.EventArgs e)
    {
        Debug.Log("WebSocket connected!");
        statusText.text = "Connected to server!";

        // 发送初始消息
        ws.Send("{\"type\": \"fetch-conf-info\"}");
    }

    void OnMessage(object sender, MessageEventArgs e)
    {
        Debug.Log("Message received from server: " + e.Data);
        statusText.text = "Message received: " + e.Data;

        // 处理服务器响应
        HandleServerResponse(e.Data);
    }

    void OnError(object sender, ErrorEventArgs e)
    {
        Debug.LogError("WebSocket error: " + e.Message);
        statusText.text = "Error: " + e.Message;
    }

    void OnClose(object sender, CloseEventArgs e)
    {
        Debug.Log("WebSocket closed: " + e.Reason);
        statusText.text = "Connection closed: " + e.Reason;
    }

    void HandleServerResponse(string response)
    {
        // 解析JSON响应
        var jsonResponse = JsonUtility.FromJson<ServerResponse>(response);

        // 根据响应类型处理
        switch (jsonResponse.type)
        {
            case "config-info":
                Debug.Log("Config Info: " + jsonResponse.conf_name + ", " + jsonResponse.conf_uid);
                break;
            case "history-list":
                Debug.Log("History List: " + jsonResponse.histories);
                break;
            case "history-data":
                Debug.Log("History Data: " + jsonResponse.messages);
                break;
            case "new-history-created":
                Debug.Log("New History Created: " + jsonResponse.history_uid);
                break;
            case "history-deleted":
                Debug.Log("History Deleted: " + jsonResponse.history_uid);
                break;
            case "full-text":
                Debug.Log("Full Text: " + jsonResponse.text);
                break;
            case "set-model":
                Debug.Log("Model Info: " + jsonResponse.model_info);
                break;
            case "control":
                Debug.Log("Control: " + jsonResponse.text);
                break;
            default:
                Debug.Log("Unknown response type: " + jsonResponse.type);
                break;
        }
    }

    void OnDestroy()
    {
        if (ws != null)
        {
            ws.Close();
            ws = null;
        }
    }

    // 用于解析JSON响应的类
    [System.Serializable]
    private class ServerResponse
    {
        public string type;
        public string conf_name;
        public string conf_uid;
        public string[] histories;
        public string[] messages;
        public string history_uid;
        public string text;
        public string model_info;
    }
}