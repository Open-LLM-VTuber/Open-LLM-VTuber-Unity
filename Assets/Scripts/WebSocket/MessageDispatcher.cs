// MessageDispatcher.cs
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using UnityEngine;
public class MessageDispatcher
{
    private Dictionary<string, Action<WebSocketMessage>> handlers = new Dictionary<string, Action<WebSocketMessage>>();

    public void RegisterHandler(string messageType, Action<WebSocketMessage> handler)
    {
        handlers[messageType] = handler;
    }

    public void Dispatch(string json)
    {
        try
        {
            var baseMessage = JsonConvert.DeserializeObject<WebSocketMessage>(json);
            if (handlers.TryGetValue(baseMessage.type, out var handler))
            {
                var concreteType = GetMessageType(baseMessage.type);
                var message = JsonConvert.DeserializeObject(json, concreteType) as WebSocketMessage;
                UnityMainThreadDispatcher.Instance.Enqueue(() => handler(message));
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Dispatch failed: {ex.Message}");
        }
    }

    private Type GetMessageType(string type)
    {
        return type switch
        {
            "full-text" => typeof(TextMessage),
            "control" => typeof(TextMessage),
            "audio" => typeof(AudioMessage),
            "history-list" => typeof(HistoryListMessage),
            "history-data" => typeof(HistoryDataMessage),
            "new-history-created" => typeof(HistoryCreatedMessage),
            "set-model-and-conf" => typeof(ModelConfigMessage),
            "config-files" => typeof(ConfigFilesMessage),
            "background-files" => typeof(BackgroundFilesMessage),
            // 添加其他类型映射...
            _ => typeof(WebSocketMessage)
        };
    }
}