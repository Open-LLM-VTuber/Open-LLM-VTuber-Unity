using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TMP_SyncRectTransformHeight : MonoBehaviour
{
    public TMP_InputField inputField; // 输入框组件
    public RectTransform[] targetRectTransforms; // 需要同步高度的目标组件数组
    public Scrollbar scrollbar; // 滚动条组件
    public float maxHeight = 200f;

    private float initialHeight = 72f; // 初始高度
    private float currentHeight = 72f; // 当前高度

    void Start()
    {
        if (inputField == null || targetRectTransforms == null)
        {
            Debug.LogError("InputField or Target RectTransforms are not assigned!");
            return;
        }

        if (scrollbar == null)
        {
            Debug.LogError("Scrollbar is not assigned!");
            return;
        }

        // 记录初始高度
        currentHeight = initialHeight = inputField.GetComponent<RectTransform>().rect.height;

        // 监听输入框的文本变化事件
        inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
    }

    void OnInputFieldValueChanged(string text)
    {
        // 获取输入框的当前高度
        float targetHeight = Mathf.Clamp(inputField.textComponent.preferredHeight, initialHeight, maxHeight);

        // 计算高度变化量
        float heightChange = targetHeight - currentHeight;

        // 记录滚动条的原始 value
        float storedScrollbarValue = scrollbar.value;

        // 同步高度变化量到所有目标组件
        foreach (RectTransform rectTransform in targetRectTransforms)
        {
            // 确保目标组件不为空
            if (rectTransform == null)
            {
                Debug.LogWarning("One of the Target RectTransforms is missing!");
                continue;
            }

            // 设置新高度
            rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                rectTransform.rect.height + heightChange
            );
        }

        // 更新当前高度
        currentHeight = targetHeight;

        // 恢复滚动条的原始 value
        scrollbar.value = storedScrollbarValue;
    }
}