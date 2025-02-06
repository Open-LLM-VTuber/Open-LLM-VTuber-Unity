
using UnityEngine;

public class HistoryManager : MonoBehaviour
{
    private string historyUid;
    private HistoryListMessage historyList;
    private HistoryDataMessage historyData;
    
    // 历史记录更新事件
    public delegate void HistoryUpdatedHandler(HistoryDataMessage historyData);
    public event HistoryUpdatedHandler OnHistoryUpdated;

    public static HistoryManager Instance { get; private set; }
    public bool initialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Duplicate HistoryManager instance detected. Destroying the new one.");
            Destroy(gameObject);
        }
    }

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