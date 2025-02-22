
using Newtonsoft.Json;
using UnityEngine;

// Handlers/HistoryMessageHandler.cs
public class HistoryMessageHandler : InitOnceSingleton<HistoryMessageHandler>
{
    public void Initialize(WebSocketManager wsManager)
    {

        InitOnce(() =>
        {
            wsManager.RegisterHandler("history-list", HandleHistoryList);
            wsManager.RegisterHandler("history-data", HandleHistoryData);
            wsManager.RegisterHandler("new-history-created", HandleNewHistory);
            wsManager.RegisterHandler("history-deleted", HandleHistoryDeleted);
            RequestInitialData();
        });
    }

    private void RequestInitialData()
    {
        var wsManager = WebSocketManager.Instance;
        wsManager.Send(new WebSocketMessage { type = "fetch-history-list" });
        wsManager.Send(new WebSocketMessage { type = "create-new-history" });
    }

    private void HandleHistoryList(WebSocketMessage message)
    {
        var historyMsg = message as HistoryListMessage;
        Debug.Log($"Received {historyMsg.histories.Count} history items");

        HistoryManager.Instance.HistoryList = historyMsg;
        HistoryManager.Instance.UpdateHistoryList();
    }

    private void HandleHistoryData(WebSocketMessage message)
    {
        var historyData = message as HistoryDataMessage;
        Debug.Log($"Received {historyData.messages.Count} history messages");
        HistoryManager.Instance.HistoryData = historyData;
        HistoryManager.Instance.UpdateHistoryData();
    }

    private void HandleNewHistory(WebSocketMessage message)
    {
        var newHistory = message as HistoryCreatedMessage;
        Debug.LogWarning($"New history historyUid:  { newHistory.history_uid}");
        HistoryManager.Instance.HistoryUid = newHistory.history_uid;
    }

    private void HandleHistoryDeleted(WebSocketMessage message)
    {
        var deletedHistory = message as HistoryDeletedMessage;
        Debug.Log($"History deleted: {deletedHistory.history_uid}, Success: {deletedHistory.success}");
    }
}