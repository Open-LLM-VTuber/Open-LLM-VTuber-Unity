using Newtonsoft.Json;
using System;
using WebSocketSharp;
using UnityEngine;
using System.Collections;

// WebSocketManager.cs
public sealed class WebSocketManager : InitOnceSingleton<WebSocketManager>
{
    public string Url { get; private set; }
    private WebSocket ws;
    private MessageDispatcher dispatcher;

    private Coroutine checkRoutine; // 用于保存协程引用，以便停止
    private const float CHECK_INTERVAL = 3f; // 检查间隔 3 秒
    private const int MAX_FALSE_COUNT = 3; // 最大 false 次数

    enum Status {
        Success = 0,
        Error = 1,
        Retry = 2,
        Closed = 3
    }
    public void Initialize()
    {
        InitOnce(() =>
        {
            Url = SettingsManager.Instance.GetSetting("General.WebSocketUrl");
            dispatcher = new MessageDispatcher();
            ws = new WebSocket(Url);
            ConfigureEventHandlers();
            ws.Connect();
        });
    }

    public bool CheckConnectionStatus()
    {
        if (ws == null)
        {
            return false;
        }
        if (ws.IsAlive)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // 开始定时检查连接状态
    public void StartCheckingConnection()
    {
        if (checkRoutine != null)
        {
            StopCoroutine(checkRoutine); // 如果已有检查在运行，先停止
        }
        checkRoutine = StartCoroutine(CheckConnectionRoutine());
    }

    // 停止定时检查
    public void StopCheckingConnection()
    {
        if (checkRoutine != null)
        {
            StopCoroutine(checkRoutine);
            checkRoutine = null;
            DebugWrapper.Instance.Log("Connection checking stopped.", Color.cyan);
        }
    }

    // 协程：每隔 CHECK_INTERVAL 秒检查一次连接状态
    private IEnumerator CheckConnectionRoutine()
    {
        int falseCount = 0;

        DebugWrapper.Instance.Log("Checking Websocket Connecion...", Color.black);

        while (true)
        {
            yield return new WaitForSeconds(CHECK_INTERVAL);

            bool isConnected = CheckConnectionStatus();

            if (isConnected)
            {
                // 连接成功，立即停止检查
                Log(Status.Success);
                checkRoutine = null;
                yield break; // 退出协程
            }
            else
            {
                falseCount++;
                
                Log(Status.Retry, falseCount);
                if (falseCount >= MAX_FALSE_COUNT)
                {
                    // 达到最大 false 次数，停止检查
                    Log(Status.Error);
                    checkRoutine = null;
                    yield break;
                }
            }
            
        }
    }

    void Log(Status status, int falseCount = 0) {
        switch (status)
        {
            case Status.Success:
                DebugWrapper.Instance.Log("[b]WebSocket Connection established![/b]", Color.green);
                break;
            case Status.Retry:
                DebugWrapper.Instance.Log($"[b]Failed WebSocket connection attempt {falseCount}/{MAX_FALSE_COUNT}[/b]", Color.red);
                break;
            case Status.Error:
                DebugWrapper.Instance.Log("[b]Max failed attempts reached, stopping checks.[/b]", Color.red);
                break;
            case Status.Closed:
                DebugWrapper.Instance.Log("[b]WebSocket Connection Closed.[/b]", Color.gray);
                break;
            default:
                return;
        }
        
    }

    private void ConfigureEventHandlers()
    {
        ws.OnOpen += (s, e) => {
            Debug.Log("Connected");
            Log(Status.Success);
        };
        ws.OnMessage += (s, e) => dispatcher.Dispatch(e.Data);
        ws.OnError += (s, e) => {
            Debug.LogError($"Error: {e.Message}");
            Log(Status.Error);
        };
        ws.OnClose += (s, e) => {
            Debug.Log($"Closed: {e.Reason}");
        };
    }

    public void Send<T>(T message) where T : WebSocketMessage
    {
        if (ws?.IsAlive == true)
        {
            Debug.LogWarning("Sent Type: " + message.type);
            string json = JsonConvert.SerializeObject(message);
            ws.Send(json);
        }
    }

    public void RegisterHandler(string messageType, Action<WebSocketMessage> handler)
    {
        dispatcher.RegisterHandler(messageType, handler);
    }

    private void OnDestroy()
    {
        if (ws != null && ws.IsAlive)
            ws.Close();
    }
}