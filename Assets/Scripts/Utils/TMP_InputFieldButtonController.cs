using UnityEngine;
using UnityEngine.UI;
using TMPro; // 引入 TextMeshPro 命名空间

public class TMP_InputFieldButtonController : MonoBehaviour
{
    public TMP_InputField inputField; // 绑定的 TMP_InputField
    public Button button; // 绑定的 Button

    private void Start()
    {
        // 初始化按钮状态
        UpdateButtonState();

        // 添加监听事件
        inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
    }

    private void OnInputFieldValueChanged(string value)
    {
        // 每当 InputField 的值发生变化时，更新按钮状态
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        // 检查 InputField 是否有内容
        bool hasText = !string.IsNullOrEmpty(inputField.text);

        // 设置按钮的交互状态
        button.interactable = hasText;
    }

    private void OnDestroy()
    {
        // 移除监听事件，避免内存泄漏
        inputField.onValueChanged.RemoveListener(OnInputFieldValueChanged);
    }
}