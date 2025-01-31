
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_InputField))]
public class TMP_AutoResizeInputField : MonoBehaviour
{
    public TMP_InputField inputField; // 多行输入的 InputField
    public Scrollbar scrollbar;       // 控制滚动的 Scrollbar
    public float maxHeight = 200f;    // 输入框的最大高度

    private float minHeight;          // 输入框的最小高度
    private RectTransform inputFieldRectTransform; // InputField 的 RectTransform
    private RectTransform scrollbarRectTransform;  // Scrollbar 的 RectTransform
    private float contentHeight;      // 文本内容的总高度
    private float viewportHeight;     // 输入框的可见高度

    void Start()
    {
        if (inputField == null)
        {
            inputField = GetComponent<TMP_InputField>();
        }

        // 获取 InputField 的 RectTransform
        inputFieldRectTransform = inputField.GetComponent<RectTransform>();

        // 获取 Scrollbar 的 RectTransform
        scrollbarRectTransform = scrollbar.GetComponent<RectTransform>();

        // 初始化最小高度和可见高度
        minHeight = viewportHeight = inputFieldRectTransform.rect.height;

        // 监听 InputField 的文本变化事件
        inputField.onValueChanged.AddListener(OnTextChanged);

        // 监听 Scrollbar 的值变化事件
        scrollbar.onValueChanged.AddListener(OnScrollbarValueChanged);

        // 初始化时隐藏滚动条
        scrollbar.gameObject.SetActive(false);
    }

    void OnTextChanged(string text)
    {
        // 计算文本内容的总高度
        contentHeight = inputField.textComponent.preferredHeight;

        // 保证新高度在minHeight和maxHeight之间
        float newHeight = Mathf.Clamp(contentHeight, minHeight, maxHeight);
        // Debug.Log("minHeight: " + minHeight + ", maxHeight: " + maxHeight + ", " + "newHeight: " + newHeight);
        // 动态调整 InputField 的高度
        inputFieldRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
        // 同步调整 Scrollbar 的高度
        scrollbarRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);

        // 更新 Scrollbar 的状态
        UpdateScrollbar();

        // 更新文本显示位置
        UpdateTextPosition(scrollbar.value);
    }

    void OnScrollbarValueChanged(float value)
    {
        // 当 Scrollbar 值变化时，更新 InputField 的文本显示位置
        UpdateTextPosition(value);
    }

    void UpdateScrollbar()
    {
        if (contentHeight > viewportHeight)
        {
            // 如果文本内容的总高度超过 InputField 的可见高度，显示 Scrollbar
            scrollbar.gameObject.SetActive(true);

            // 设置 Scrollbar 的 size（滑块大小）
            scrollbar.size = viewportHeight / contentHeight;

            // 设置 Scrollbar 的 value（滑块位置）
            scrollbar.value = 0.5f; // 默认滚动到底部
        }
        else
        {
            // 如果文本内容的总高度不超过 InputField 的可见高度，隐藏 Scrollbar
            scrollbar.gameObject.SetActive(false);
        }
    }

    void UpdateTextPosition(float scrollValue)
    {
        // 获取文本的总高度和显示区域的高度
        float textHeight = inputField.textComponent.preferredHeight;
        float viewportHeight = inputFieldRectTransform.rect.height;

        // 如果文本高度小于显示区域高度，则不需要滚动
        if (textHeight <= viewportHeight)
        {
            return;
        }

        // 根据 Scrollbar 的值计算文本的偏移量
        float offsetY = scrollValue * (textHeight - viewportHeight);
        inputField.textComponent.rectTransform.anchoredPosition = new Vector2(
            inputField.textComponent.rectTransform.anchoredPosition.x,
            offsetY
        );
    }
}
