using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class HistoryUIManager : MonoBehaviour
{
    [Header("ScrollRect")]
    public GameObject messageEntry;
    public GameObject msgSplitLine;
    public ScrollRectFix chatHistoryScrollView;
    public RectTransform parentObject;      // 父对象
    public Scrollbar scrollBar;

    [Header("MessageEntry")]
    [SerializeField] private GameObject longPressInfo; 
    [SerializeField] private RectTransform infoParentObject; 

    void Start()
    {
        ClearParentObjectChildren();
        // 订阅历史记录更新事件
        if (HistoryManager.Instance != null)
        {
            HistoryManager.Instance.OnHistoryListUpdated += UpdateEntries;
        }
        RefreshHistoryList();
    }

    private void OnDestroy()
    {
        // 取消订阅事件，避免内存泄漏
        if (HistoryManager.Instance != null)
        {
            HistoryManager.Instance.OnHistoryListUpdated -= UpdateEntries;
        }
        ClearParentObjectChildren();
    }

    public void UpdateEntries()
    {
        ClearParentObjectChildren();
        DisplayMessageEntries();
    }

    private IEnumerator ScrollToBottomCoroutine()
    {
        yield return new WaitForEndOfFrame(); // 等待布局渲染完成
        scrollBar.value = 0f;
    }

    private void ClearParentObjectChildren()
    {
        if (parentObject != null)
        {
            foreach (Transform child in parentObject)
            {
                var button = child.GetComponent<LongPressButton>();
                if (button != null)
                {
                    button.onShortPress.RemoveAllListeners();
                    button.onLongPress.RemoveAllListeners();
                }
                Destroy(child.gameObject);
            }
        }
    }

    private void DisplayMessageEntries()
    {
        var historyList = HistoryManager.Instance.HistoryList;
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
            charContent.HistoryUid = history.uid;

            // 每个按钮绑定"进入时，更新uid后刷新记录"
            if (chatHistoryScrollView != null)
            {
                var button = entryObject.GetComponent<LongPressButton>();
                button.onShortPress.AddListener(() => 
                { 
                    HistoryManager.Instance.HistoryUid = charContent.HistoryUid;
                    HistoryManager.Instance.DeltaUpdate = true;
                    chatHistoryScrollView.Refresh();
                    
                });

                button.onLongPress.AddListener(() => { 
                    var infoObject = Instantiate(longPressInfo, infoParentObject);
                    var fadeAnimator = infoObject.GetComponent<FadeAnimation>();
                    fadeAnimator.FadeIn();
                    var entryOp = infoObject.GetComponent<MessageEntryOp>();
                    entryOp.messageEntry = entryObject; // 传递entry引用
                    entryOp.msgSplitLine = splitObject; // 传递splitLine引用
                });
            }
            var avatarManager = entryObject.GetComponent<AvatarManager>();
            if (!string.IsNullOrEmpty(message.avatar))
            {
                string avatarUrl = new UriBuilder(baseUrl) { Path = $"avatars/{message.avatar}" }.ToString();
                var name = Path.GetFileNameWithoutExtension(message.avatar);
                var absolutePath = Path.Combine(Application.temporaryCachePath, "live2d-models", name, message.avatar);
                avatarManager.SetAvatar(avatarUrl, absolutePath);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(parentObject);
    }

    public static void RefreshHistoryList()
    {
        WebSocketManager.Instance.Send(new WebSocketMessage
        { type = "fetch-history-list" });
    }

    public static void CreateNewHistory()
    {
        WebSocketManager.Instance.Send(new WebSocketMessage
        { type = "create-new-history" });
    }

    public static void DeleteHistory(string uid)
    {
        WebSocketManager.Instance.Send(new HistoryCreatedMessage
        { type = "delete-history", history_uid = uid});
    }

}
