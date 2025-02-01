using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class AutoResizeInputFieldWithScrollbar : MonoBehaviour
{
    public InputField inputField; // 多行输入的 InputField
    public Scrollbar scrollbar;   // 控制滚动的 Scrollbar
    public float maxHeight = 200f; // 输入框的最大高度

    private float minHeight;      // 输入框的最小高度
    private RectTransform inputFieldRectTransform; // InputField 的 RectTransform
    private RectTransform scrollbarRectTransform;  // Scrollbar 的 RectTransform
    private RectTransform textRectTransform;       // Text 的 RectTransform
    private float inputFieldWidth; // 输入框的宽度
    private Font font; // 字体
    private int fontSize; // 字体大小
    private float lineHeight; // 行高

    void Start()
    {
        if (inputField == null)
        {
            inputField = GetComponent<InputField>();
        }

        // 获取 InputField 的 RectTransform
        inputFieldRectTransform = inputField.GetComponent<RectTransform>();

        // 获取 Scrollbar 的 RectTransform
        scrollbarRectTransform = scrollbar.GetComponent<RectTransform>();

        // 获取 Text 的 RectTransform
        textRectTransform = inputField.textComponent.GetComponent<RectTransform>();

        // 初始化最小高度和可见高度
        minHeight = inputFieldRectTransform.rect.height;

        // 获取输入框的宽度
        inputFieldWidth = inputFieldRectTransform.rect.width;

        // 获取字体和字体大小
        font = inputField.textComponent.font;
        fontSize = inputField.textComponent.fontSize;

        // 计算行高（假设为字体大小的 1.2 倍）
        lineHeight = fontSize * 1.2f;

        // 监听 InputField 的文本变化事件
        inputField.onValueChanged.AddListener(OnTextChanged);

        // 初始化时隐藏滚动条
        scrollbar.gameObject.SetActive(false);
    }

    void OnTextChanged(string text)
    {
        // 处理文本换行
        string wrappedText = WrapText(text);

        // 如果文本被修改，更新 InputField 的文本
        if (wrappedText != text)
        {
            inputField.text = wrappedText;
        }

        // 计算新的高度
        float newHeight = CalculateTextHeight(wrappedText);

        // 动态调整 InputField 的高度
        inputFieldRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Min(newHeight, maxHeight));

        // 更新 Scrollbar 的状态
        UpdateScrollbar(newHeight);

        // 更新文本显示位置
        UpdateTextPosition(scrollbar.value);
    }

    string WrapText(string text)
    {
        string[] lines = text.Split('\n');
        string result = "";

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            string newLine = "";
            float lineWidth = 0f;

            foreach (char c in line)
            {
                // 获取字符的宽度
                float charWidth = GetCharacterWidth(c);

                // 如果当前行宽度超过输入框宽度，插入换行符
                if (lineWidth + charWidth > inputFieldWidth)
                {
                    newLine += '\n';
                    lineWidth = 0f;
                }

                newLine += c;
                lineWidth += charWidth;
            }

            result += newLine;
            //if (i < lines.Length - 1)
            //{
            //    result += '\n';
            //}
        }

        return result;
    }

    float GetCharacterWidth(char c)
    {
        // 获取字符的宽度（基于字体和字体大小）
        CharacterInfo characterInfo;
        if (font.GetCharacterInfo(c, out characterInfo, fontSize))
        {
            return characterInfo.advance;
        }
        return 0f;
    }

    float CalculateTextHeight(string text)
    {
        // 计算文本的行数
        int lineCount = text.Split('\n').Length;

        // 计算文本的总高度
        float totalHeight = lineCount * lineHeight;

        // 返回总高度，确保不小于初始高度
        return Mathf.Max(totalHeight, minHeight);
    }

    void UpdateScrollbar(float newHeight)
    {
        if (newHeight > maxHeight)
        {
            // 如果文本内容的总高度超过 InputField 的可见高度，显示 Scrollbar
            scrollbar.gameObject.SetActive(true);

            // 设置 Scrollbar 的 size（滑块大小）
            scrollbar.size = maxHeight / newHeight;

            // 设置 Scrollbar 的 value（滑块位置）
            scrollbar.value = 1; // 默认滚动到底部
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
        float textHeight = CalculateTextHeight(inputField.text);
        float viewportHeight = inputFieldRectTransform.rect.height;

        // 如果文本高度小于显示区域高度，则不需要滚动
        if (textHeight <= viewportHeight)
        {
            return;
        }

        // 根据 Scrollbar 的值计算文本的偏移量
        float offsetY = scrollValue * (textHeight - viewportHeight);
        textRectTransform.anchoredPosition = new Vector2(
            textRectTransform.anchoredPosition.x,
            offsetY
        );
    }
}