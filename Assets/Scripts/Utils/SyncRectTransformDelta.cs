using System.Collections.Generic;
using UnityEngine;

public class SyncRectTransformDelta : MonoBehaviour
{
    public RectTransform sourceRectTransform;
    public List<RectTransform> targetRectTransforms;
    public float maxDeltaHeight = 200f;
    public float minHeightThreshold = 300f;

    private Dictionary<RectTransform, float> initialHeights;

    void Start()
    {
        initialHeights = new Dictionary<RectTransform, float>();

        // 记录初始高度
        initialHeights[sourceRectTransform] = sourceRectTransform.rect.height;
        foreach (var target in targetRectTransforms)
        {
            initialHeights[target] = target.rect.height;
        }
    }

    void Update()
    {
        float currentSourceHeight = sourceRectTransform.rect.height;
        float deltaHeight = Mathf.Clamp(currentSourceHeight - minHeightThreshold, 0f, maxDeltaHeight);
        if (deltaHeight.Equals(0f))
        {
            return;
        }
        // 调整目标组件的高度
        foreach (var target in targetRectTransforms)
        {
            float newHeight = initialHeights[target] + deltaHeight;
            SetRectTransformHeight(target, newHeight);
        }
    }

    private void SetRectTransformHeight(RectTransform rectTransform, float newHeight)
    {
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, newHeight);
    }
}