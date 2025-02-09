using UnityEngine;

public class HistoryManager : Singleton<HistoryManager>
{
    private string historyUid;
    private HistoryListMessage historyList;
    private HistoryDataMessage historyData;
    
    // 历史记录更新事件
    public delegate void HistoryUpdatedHandler(HistoryDataMessage historyData);
    public event HistoryUpdatedHandler OnHistoryUpdated;
    public bool initialized = false;

    public void SetHistoryUid(string uid) => historyUid = uid;

    public void SetHistoryList(HistoryListMessage msg) => historyList = msg;

    public void SetHistoryData(HistoryDataMessage msg)
    {
        historyData = msg;
        OnHistoryUpdated?.Invoke(historyData); // 触发事件
    }

    public string GetHistoryUid() => historyUid;

    public HistoryListMessage GetHistoryList() => historyList;

    public HistoryDataMessage GetHistoryData() => historyData;

    private void OnDestroy()
    {
        Debug.LogWarning("OnDestroy historyUid: " + historyUid);
    }
}