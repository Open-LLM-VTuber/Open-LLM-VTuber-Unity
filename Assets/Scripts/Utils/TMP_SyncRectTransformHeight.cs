using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TMP_SyncRectTransformHeight : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_InputField inputField; // 输入框组件
    public RectTransform targetRectTransform; // 需要同步高度的组件
    public float maxHeight = 200f;

    private float initialHeight = 72f; // 初始高度
    private float varHeight = 72f; // 变化的高度

    void Start()
    {
        if (inputField == null || targetRectTransform == null)
        {
            Debug.LogError("InputField or Target RectTransform is not assigned!");
            return;
        }

        // 记录初始高度
        varHeight = initialHeight = inputField.GetComponent<RectTransform>().rect.height;

        // 监听输入框的文本变化事件
        inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
    }

    void OnInputFieldValueChanged(string text)
    {
        // 获取输入框的当前高度
        float currentHeight = Mathf.Clamp(inputField.textComponent.preferredHeight, initialHeight, maxHeight);
        
        // 计算高度变化量
        float heightChange = currentHeight - varHeight;
        //Debug.Log("heightChange: " + heightChange);
        // 同步高度变化量到目标组件
        targetRectTransform.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            targetRectTransform.rect.height + heightChange
        );

        // 更新初始高度
        varHeight = currentHeight;
    }

}
