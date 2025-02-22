using System;
using UnityEngine;

public class HistoryManager : Singleton<HistoryManager>
{
    private string historyUid;
    private HistoryListMessage historyList;
    private HistoryDataMessage historyData;

    public event Action OnHistoryDataUpdated;
    public event Action OnHistoryListUpdated;

    public HistoryDataItem assistantLastMessage = new HistoryDataItem();

    public string HistoryUid
    {
        get => historyUid;
        set => historyUid = value;
    }

    public HistoryListMessage HistoryList
    {
        get => historyList;
        set
        {
            historyList = value;
        }
    }

    public HistoryDataMessage HistoryData
    {
        get => historyData;
        set
        {
            historyData = value;
        }
    }

    public void UpdateHistoryList()
    {
        OnHistoryListUpdated?.Invoke();
    }
    
    public void UpdateHistoryData()
    {
        OnHistoryDataUpdated?.Invoke();
    }

    private void OnDestroy()
    {
        Debug.LogWarning($"OnDestroy historyUid: {historyUid}");
    }
}