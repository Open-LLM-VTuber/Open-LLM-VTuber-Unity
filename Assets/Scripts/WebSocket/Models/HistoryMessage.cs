using Newtonsoft.Json;
using System;
using System.Collections.Generic;


[Serializable]
public class HistoryListMessage : WebSocketMessage
{
    public List<HistoryListItem> histories;
}

[Serializable]
public class HistoryListItem
{
    public string uid;
    public HistoryDataItem latest_message;
    [JsonProperty("timestamp")]
    public DateTime? timestamp;
}

[Serializable]
public class HistoryDataMessage : WebSocketMessage
{
    public List<HistoryDataItem> messages;
}

[Serializable]
public class HistoryDataItem
{
    public string role;
    [JsonProperty("timestamp")]
    public DateTime? timestamp;
    public string content;
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