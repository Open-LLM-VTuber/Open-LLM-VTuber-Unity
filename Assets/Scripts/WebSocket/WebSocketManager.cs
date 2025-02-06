using Newtonsoft.Json;
using System;
using WebSocketSharp;
using UnityEngine;

// WebSocketManager.cs
public class WebSocketManager : MonoBehaviour
{
    private WebSocket ws;
    private MessageDispatcher dispatcher;

    private void Awake()
    {
        dispatcher = new MessageDispatcher();
    }

    public void Initialize(string url)
    {
        ws = new WebSocket(url);
        ConfigureEventHandlers();
        ws.Connect();
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