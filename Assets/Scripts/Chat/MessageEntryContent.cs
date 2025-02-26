using TMPro;
using System;
using UnityEngine;

public class MessageEntryContent : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText; // 名字
    [SerializeField] private TMP_Text timeText; // 时间
    [SerializeField] private TMP_Text contentText; // 气泡UI文本组件
    private string historyUid;
    public string HistoryUid
    {
        get => historyUid;
        set => historyUid = value;
    }

    public void SetName(string name)
    {
        if (nameText != null)
        {
            nameText.text = name;
        }
    }

    public void SetTime(DateTime time)
    {
        if (timeText != null)
        {
            timeText.text = time.ToString();
        }
    }

    public void SetContent(string content)
    {
        if (contentText != null)
        {
            contentText.text = content;

        }
    }


}
