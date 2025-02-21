using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HistoryUIManager : MonoBehaviour
{
    public GameObject messageEntry;
    public GameObject msgSplitLine;
    public Transform parentObject;      // 父对象

    void Start()
    {
        // 订阅历史记录更新事件
        if (HistoryManager.Instance != null)
        {
            HistoryManager.Instance.OnHistoryListUpdated += HandleHistoryListUpdated;
        }
        ClearParentObjectChildren();
    }

    private void OnDestroy()
    {
        // 取消订阅事件，避免内存泄漏
        if (HistoryManager.Instance != null)
        {
            HistoryManager.Instance.OnHistoryListUpdated -= HandleHistoryListUpdated;
        }
    }

    private void HandleHistoryListUpdated(HistoryListMessage historyList)
    {
        UpdateEntries();
    }

    public void UpdateEntries()
    {
        ClearParentObjectChildren();
        DisplayMessageEntries();
    }

    private void ClearParentObjectChildren()
    {
        if (parentObject != null)
        {
            foreach (Transform child in parentObject)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void DisplayMessageEntries()
    {
        var historyList = HistoryManager.Instance.GetHistoryList();
        var baseUrl = SettingsManager.Instance.GetSetting("General.BaseUrl");

        foreach (var history in historyList.histories)
        {
            GameObject entryObject = Instantiate(messageEntry, parentObject);
            GameObject splitObject = Instantiate(msgSplitLine, parentObject);

            var message = history.latest_message;

            var charContent = entryObject.GetComponent<MessageEntryContent>();
            charContent.SetName(message.name);
            charContent.SetTime(message.timestamp);
            charContent.SetContent(message.content);

            var avatarManager = entryObject.GetComponent<AvatarManager>();
            if (!string.IsNullOrEmpty(message.avatar))
            {
                string avatarUrl = new UriBuilder(baseUrl) { Path = $"avatars/{message.avatar}" }.ToString();
                AvatarManager.AddOrUpdateAvatarUrl(message.name, avatarUrl);
                avatarManager.SetAvatarByName(message.name);
            }
        }

        Canvas.ForceUpdateCanvases();
    }


}
