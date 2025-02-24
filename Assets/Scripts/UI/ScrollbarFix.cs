using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ScrollRectFix : ScrollRect
{
    [SerializeField]
    private GameObject refreshIndicatorPrefab;
    private GameObject currentRefreshIndicator;

    [SerializeField]
    private float pixelThreshold = 120f;
    [SerializeField]
    private float timeout = 5f;

    private bool refreshReady = false;

    // 新增的委托用于自定义操作
    public Func<IEnumerator> refreshAction;
    public Action postRefreshAction;

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);

        float scrollableHeight = content.rect.height - viewport.rect.height;
        float normalizedThreshold = pixelThreshold / scrollableHeight;

        if (verticalNormalizedPosition > 1f + normalizedThreshold)
        {
            refreshReady = true;
            if (currentRefreshIndicator == null && refreshIndicatorPrefab != null)
            {
                ShowRefreshIndicator();
            }
        }
        else
        {
            refreshReady = false;
            if (currentRefreshIndicator != null)
            {
                DestroyRefreshIndicator();
            }
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        if (refreshReady)
        {
            Refresh();
            refreshReady = false;
        }
        else
        {
            DestroyRefreshIndicator();
        }
    }

    public void Refresh()
    {
        StartCoroutine(RefreshContent());
    }

    private IEnumerator RefreshContent()
    {
        ShowRefreshIndicator();
        bool refreshCompleted = false;

        if (refreshAction != null)
        {
            // 启动刷新操作协程
            var runningCoroutine = StartCoroutine(RunRefreshOperation(() => {
                refreshCompleted = true;
            })) ?? null;

            float elapsed = 0f;

            while (elapsed < timeout && !refreshCompleted)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // 处理超时
            if (!refreshCompleted)
            {
                if (runningCoroutine != null)
                {
                    StopCoroutine(runningCoroutine);
                }
                Debug.LogWarning("刷新操作超时");
            }
        }
        // 执行后续操作
        postRefreshAction?.Invoke();
        DestroyRefreshIndicator();
    }

    private IEnumerator RunRefreshOperation(Action onComplete)
    {
        yield return refreshAction?.Invoke();
        onComplete?.Invoke();
    }

    private void ShowRefreshIndicator()
    {
        if (refreshIndicatorPrefab != null && currentRefreshIndicator == null)
        {
            currentRefreshIndicator = Instantiate(refreshIndicatorPrefab, content);
            currentRefreshIndicator.transform.SetAsFirstSibling();
            var rotateAnimator = currentRefreshIndicator.GetComponent<RotateAnimation>();
            if (rotateAnimator != null)
            {
                rotateAnimator.StartInfiniteRotation();
            }
        }
    }

    private void DestroyRefreshIndicator()
    {
        if (currentRefreshIndicator != null)
        {
            var rotateAnimator = currentRefreshIndicator.GetComponent<RotateAnimation>();
            if (rotateAnimator != null)
            {
                rotateAnimator.StopInfiniteRotation();
            }
            Destroy(currentRefreshIndicator);
            currentRefreshIndicator = null;
        }
    }

    override protected void LateUpdate()
    {
        base.LateUpdate();
        HandleScrollbarVisibility();
    }

    override public void Rebuild(CanvasUpdate executing)
    {
        base.Rebuild(executing);
        HandleScrollbarVisibility();
    }

    private void HandleScrollbarVisibility()
    {
        if (horizontalScrollbar) horizontalScrollbar.size = 0;
        if (verticalScrollbar) verticalScrollbar.size = 0;
    }
}
