using UnityEngine;
using UnityEngine.UI;

public class SyncRectTransformHeight : MonoBehaviour
{
    public InputField inputField; // 输入框组件
    public RectTransform targetRectTransform; // 需要同步高度的组件
    public float maxHeight = 200f; // 最大高度

    private float initialHeight; // 初始高度
    private float varHeight; // 变化的高度
    private Font font; // 字体
    private int fontSize; // 字体大小
    private float lineHeight; // 行高
    private float inputFieldWidth; // 输入框的宽度

    void Start()
    {
        if (inputField == null || targetRectTransform == null)
        {
            Debug.LogError("InputField or Target RectTransform is not assigned!");
            return;
        }

        // 获取字体和字体大小
        font = inputField.textComponent.font;
        fontSize = inputField.textComponent.fontSize;

        // 计算行高（假设为字体大小的 1.2 倍）
        lineHeight = fontSize * 1.2f;

        // 获取输入框的宽度
        inputFieldWidth = inputField.GetComponent<RectTransform>().rect.width;

        // 记录初始高度
        varHeight = initialHeight = inputField.GetComponent<RectTransform>().rect.height;

        // 监听输入框的文本变化事件
        inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
    }

    void OnInputFieldValueChanged(string text)
    {
        // 计算文本高度
        float currentHeight = CalculateTextHeight(text);

        // 限制高度在 initialHeight 和 maxHeight 之间
        currentHeight = Mathf.Clamp(currentHeight, initialHeight, maxHeight);

        // 计算高度变化量
        float heightChange = currentHeight - varHeight;

        // 同步高度变化量到目标组件
        targetRectTransform.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            targetRectTransform.rect.height + heightChange
        );

        // 更新初始高度
        varHeight = currentHeight;
    }

    float CalculateTextHeight(string text)
    {
        // 计算文本的行数
        int lineCount = CalculateLineCount(text);

        // 计算文本的总高度
        float totalHeight = lineCount * lineHeight;

        // 返回总高度，确保不小于初始高度
        return Mathf.Max(totalHeight, initialHeight);
    }

    int CalculateLineCount(string text)
    {
        string[] lines = text.Split('\n');
        int lineCount = lines.Length;

        foreach (string line in lines)
        {
            float lineWidth = 0f;

            foreach (char c in line)
            {
                // 获取字符的宽度
                float charWidth = GetCharacterWidth(c);

                // 如果当前行宽度超过输入框宽度，增加行数
                if (lineWidth + charWidth > inputFieldWidth)
                {
                    lineCount++;
                    lineWidth = 0f;
                }

                lineWidth += charWidth;
            }
        }

        return lineCount;
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
}