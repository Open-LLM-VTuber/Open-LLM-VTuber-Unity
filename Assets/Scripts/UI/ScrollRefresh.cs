using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System;

public class ScrollRefresh : MonoBehaviour
{
    // Start is called before the first frame update
    private ScrollRectFix scrollRectFix;

    [SerializeField]
    private UnityEvent onRefresh;

    [SerializeField]
    private UnityEvent postRefresh;

    private bool isConditionMet = false;

    private void Start()
    {
        scrollRectFix = GetComponent<ScrollRectFix>();
        scrollRectFix.refreshAction = RefreshAction;
        scrollRectFix.postRefreshAction = PostRefresh;

        // 订阅更新
        HistoryManager.Instance.OnHistoryDataUpdated += SetConditionMet;
        HistoryManager.Instance.OnHistoryListUpdated += SetConditionMet;
    }

    private void OnDestroy()
    {
        // 取消订阅
        HistoryManager.Instance.OnHistoryDataUpdated -= SetConditionMet;
        HistoryManager.Instance.OnHistoryListUpdated -= SetConditionMet;
    }

    public void ResetCondition()
    {
        isConditionMet = false;
    }

    public void SetConditionMet()
    {
        isConditionMet = true;
    }

    public IEnumerator RefreshAction()
    {
        ResetCondition();
        onRefresh?.Invoke();
        while (!isConditionMet)
        {
            yield return null;
        }
    }

    public void PostRefresh() 
    {
        postRefresh?.Invoke();
    }

    // 调用方法
    public void TriggerRefresh()
    {
        StartCoroutine(RefreshAction());
    }
}
