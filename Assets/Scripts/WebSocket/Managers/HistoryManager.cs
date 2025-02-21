using System;
using System.Text;
using UnityEngine;

public class HistoryManager : Singleton<HistoryManager>
{
    private string historyUid;
    private HistoryListMessage historyList;
    private HistoryDataMessage historyData;

    // 历史记录数据更新事件
    public event Action<HistoryDataMessage> OnHistoryDataUpdated;

    // 历史记录条目更新事件
    public event Action<HistoryListMessage> OnHistoryListUpdated;

    public HistoryDataItem assistantLastMessage = new HistoryDataItem();

    public void SetHistoryUid(string uid) => historyUid = uid;

    public void SetHistoryList(HistoryListMessage msg) 
    {
        historyList = msg;
        UpdateHistoryList();
    }

    public void SetHistoryData(HistoryDataMessage msg)
    {
        historyData = msg;
        UpdateHistoryData();
    }

    public void UpdateHistoryList()
    {
        OnHistoryListUpdated?.Invoke(historyList);
    }

    public void UpdateHistoryData()
    {
        OnHistoryDataUpdated?.Invoke(historyData);
    }

    public string GetHistoryUid() => historyUid;

    public HistoryListMessage GetHistoryList() => historyList;

    public HistoryDataMessage GetHistoryData() => historyData;

    private void OnDestroy()
    {
        Debug.LogWarning("OnDestroy historyUid: " + historyUid);
    }
}