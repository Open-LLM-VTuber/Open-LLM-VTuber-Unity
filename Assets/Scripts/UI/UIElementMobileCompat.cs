using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;


[System.Serializable]
public class UIElementOnlyWidth
{
    public Transform element; // UI 元素
    public float originalScreenWidth = 1080; // 原始屏幕宽度
    // public float originalTotalSpacingWidth = 0; // 原始间隙和
    public float originalItemWidth; // 原始组件宽度
}

[System.Serializable]
public class UIElementFullScreenWidth
{
    public Transform element; // UI 元素
}

[System.Serializable]
public class UIElementSpaceBetween
{
    public Transform element; // UI 元素
    public float originalScreenWidth = 1080; // 原始屏幕宽度
    public float originalItemWidth; // 原始组件的宽度
}

[System.Serializable]
public class UIElementFontSize
{
    public Transform element; // UI 元素
}

[System.Serializable]
public class UIElementIconSize
{
    public Transform element; // UI 元素
}

public class UIElementMobileCompat : MonoBehaviour
{
    public List<UIElementOnlyWidth> onlyWidth = new List<UIElementOnlyWidth>(); // onlyWidth, Inspector 可配置
    public List<UIElementFullScreenWidth> fullScreenWidth = new List<UIElementFullScreenWidth>(); // fullScreenWidth, Inspector 可配置
    public List<UIElementSpaceBetween> spaceBetween = new List<UIElementSpaceBetween>(); // Inspector 可配置
    public List<UIElementFontSize> fontSize = new List<UIElementFontSize>(); // 字号, Inspector 可配置
    public List<UIElementIconSize> iconSize = new List<UIElementIconSize>(); // 图片, Inspector 可配置

    private float worldScreenWidth;

    void Start()
    {
        if (Camera.main == null)
        {
            Debug.LogError("Main Camera not found! Ensure the Canvas is in World Space mode.");
            return;
        }

        ComputeWorldScreenWidth();
        AdjustIconSizeElements();
        AdjustFullScreenWidthElements();
        AdjustOnlyWidthElements();
        AdjustSpaceBetweenElements();
        AdjustFontSizeElements();
    }

    private void ComputeWorldScreenWidth()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float worldScreenHeight = Camera.main.orthographicSize * 2.0f;
        worldScreenWidth = worldScreenHeight * (screenWidth / screenHeight);
    }

    private void AdjustOnlyWidthElements()
    {
        foreach (var elementData in onlyWidth)
        {
            var element = elementData.element;
            if (element == null) continue;

            RectTransform rt = element.GetComponent<RectTransform>();
            if (rt == null)
            {
                Debug.LogWarning($"{element.name} does not have a RectTransform component.");
                continue;
            }

            rt.sizeDelta = new Vector2(worldScreenWidth - (elementData.originalScreenWidth - elementData.originalItemWidth), rt.sizeDelta.y);
        }
    }

    private void AdjustFullScreenWidthElements()
    {
        foreach (var elementData in fullScreenWidth)
        {
            var element = elementData.element;
            if (element == null) continue;

            RectTransform rt = element.GetComponent<RectTransform>();
            if (rt == null)
            {
                Debug.LogWarning($"{element.name} does not have a RectTransform component.");
                continue;
            }

            rt.sizeDelta = new Vector2(worldScreenWidth, rt.sizeDelta.y);
            rt.localPosition = new Vector3(0, rt.localPosition.y, rt.localPosition.z);
        }
    }

    private void AdjustSpaceBetweenElements()
    {
        foreach (var elementData in spaceBetween)
        {
            var element = elementData.element;
            if (element == null) continue;

            RectTransform rt = element.GetComponent<RectTransform>();
            if (rt == null)
            {
                Debug.LogWarning($"{element.name} does not have a RectTransform component.");
                continue;
            }

            rt.sizeDelta = new Vector2(worldScreenWidth - (elementData.originalScreenWidth - elementData.originalItemWidth), rt.sizeDelta.y);

            HorizontalLayoutGroup layoutGroup = element.GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup != null)
            {
                Transform[] buttons = element.Cast<Transform>().ToArray();
                float totalButtonWidth = buttons.Sum(button => button.GetComponent<RectTransform>()?.rect.width ?? 0);
                int buttonCount = buttons.Length;

                if (buttonCount > 1)
                {
                    layoutGroup.spacing = (worldScreenWidth - (elementData.originalScreenWidth - elementData.originalItemWidth) - totalButtonWidth) / (buttonCount - 1);
                }
                else
                {
                    Debug.LogError("Error: Icon count <= 1");
                }
            }
            else
            {
                Debug.LogError("Element does not have a HorizontalLayoutGroup component!");
            }
        }
    }

    /// <summary>
    /// 计算缩放因子
    /// </summary>
    private float CalculateScaleFactor(float scaleFactorMultiplier = 0.5f)
    {
        float referenceScreenWidth = 1080f;
        float referenceScreenHeight = 2408f;

        float widthFactor = Screen.width / referenceScreenWidth;
        float heightFactor = Screen.height / referenceScreenHeight;

        return 1 + (widthFactor * heightFactor - 1) * scaleFactorMultiplier;
    }

    /// <summary>
    /// 调整字体大小
    /// </summary>
    private void AdjustFontSizeElements()
    {
        float scaleFactor = CalculateScaleFactor(0.3f);

        foreach (var elementData in fontSize)
        {
            if (elementData.element == null) continue;

            Text textComponent = elementData.element.GetComponent<Text>();
            if (textComponent != null)
            {
                textComponent.fontSize = Mathf.RoundToInt(textComponent.fontSize * scaleFactor);
            }
            else
            {
                TMP_Text tmpTextComponent = elementData.element.GetComponent<TMP_Text>();
                if (tmpTextComponent != null)
                {
                    tmpTextComponent.fontSize = tmpTextComponent.fontSize * scaleFactor;
                }
                else
                {
                    Debug.LogWarning($"{elementData.element.name} does not have a Text or TMP_Text component.");
                }
            }
        }
    }

    /// <summary>
    /// 调整图标大小
    /// </summary>
    private void AdjustIconSizeElements()
    {
        foreach (var elementData in iconSize)
        {
            if (elementData.element == null) continue;
            float iconScaleFactor = 0.2f; //缩放因子
            float scaleFactor = CalculateScaleFactor(iconScaleFactor);

            RectTransform rt = elementData.element.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.sizeDelta = new Vector2(rt.sizeDelta.x * scaleFactor, rt.sizeDelta.y * scaleFactor);
            }
            else
            {
                Debug.LogWarning($"{elementData.element.name} does not have a RectTransform component.");
            }
        }
    }

}