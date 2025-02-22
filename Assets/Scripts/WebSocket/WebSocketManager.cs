using Newtonsoft.Json;
using System;
using WebSocketSharp;
using UnityEngine;

// WebSocketManager.cs
public sealed class WebSocketManager : InitOnceSingleton<WebSocketManager>
{
    public string Url { get; private set; }
    private WebSocket ws;
    private MessageDispatcher dispatcher;
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

    private void ConfigureEventHandlers()
    {
        ws.OnOpen += (s, e) => Debug.Log("Connected");
        ws.OnMessage += (s, e) => dispatcher.Dispatch(e.Data);
        ws.OnError += (s, e) => Debug.LogError($"Error: {e.Message}");
        ws.OnClose += (s, e) => Debug.Log($"Closed: {e.Reason}");
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