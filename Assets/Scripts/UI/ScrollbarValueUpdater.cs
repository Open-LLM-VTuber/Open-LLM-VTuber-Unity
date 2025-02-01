using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScrollbarTextUpdater : MonoBehaviour
{
    public Scrollbar scrollbar; // 引用Scrollbar组件
    public TMP_Text targetText; // 引用TMP_Text组件
    public float minValue = 0.0f; // 最小值
    public float maxValue = 1.0f; // 最大值
    public string settingName = string.Empty;
    public int precision = 0; // 精确到多少位小数
    public string statement = ""; // 自定义文本

    private void Start()
    {
        // 确保Scrollbar和TMP_Text组件都已赋值
        if (scrollbar == null || targetText == null)
        {
            Debug.LogError("Scrollbar or TMP_Text is not assigned!");
            return;
        }

        if (!string.IsNullOrEmpty(settingName))
        {
            string settingsValue = SettingsManager.Instance.GetSetting(settingName);
            Debug.Log(settingsValue + "  " + settingName);
            scrollbar.value = (float.Parse(settingsValue) - minValue) / (maxValue - minValue);
        }
        // 添加监听器，当Scrollbar的值变化时调用OnScrollbarValueChanged方法
        scrollbar.onValueChanged.AddListener(OnScrollbarValueChanged);

        // 初始化文本内容
        OnScrollbarValueChanged(scrollbar.value);
    }

    private void OnScrollbarValueChanged(float value)
    {
        // 将Scrollbar的值映射到[minValue, maxValue]范围
        float mappedValue = Mathf.Lerp(minValue, maxValue, value);

        // 根据precision的值格式化字符串
        string formatString = "F" + precision; // 例如，precision=2时，formatString="F2"

        // 更新TMP_Text的内容，包含自定义文本和映射后的值
        targetText.text = $"{statement}{mappedValue.ToString(formatString)}";
    }
}