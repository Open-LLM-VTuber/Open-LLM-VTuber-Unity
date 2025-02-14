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

[System.Serializable]
public class UIElementAdjustSize
{
    public Transform element; // 父组件，包含所有子对象
}

public class UIElementMobileCompat : MonoBehaviour
{
    public List<UIElementOnlyWidth> onlyWidth = new List<UIElementOnlyWidth>(); // onlyWidth, Inspector 可配置
    public List<UIElementFullScreenWidth> fullScreenWidth = new List<UIElementFullScreenWidth>(); // fullScreenWidth, Inspector 可配置
    public List<UIElementSpaceBetween> spaceBetween = new List<UIElementSpaceBetween>(); // Inspector 可配置
    // public List<UIElementFontSize> fontSize = new List<UIElementFontSize>(); // 字号, Inspector 可配置
    // public List<UIElementIconSize> iconSize = new List<UIElementIconSize>(); // 图片, Inspector 可配置
    public List<UIElementAdjustSize> adjustSize = new List<UIElementAdjustSize>(); // 图片, Inspector 可配置

    private float worldScreenWidth;
    private float referenceScreenWidth = 1080f;
    private float referenceScreenHeight = 2408f;

    void Start()
    {
        if (Camera.main == null)
        {
            Debug.LogError("Main Camera not found! Ensure the Canvas is in World Space mode.");
            return;
        }

        ComputeWorldScreenWidth();
        AdjustFullScreenWidthElements();
        AdjustUIElementsSize();
        // AdjustIconSizeElements();
        // AdjustFontSizeElements();
        AdjustOnlyWidthElements();
        AdjustSpaceBetweenElements();
    }

    private void ComputeWorldScreenWidth()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        Debug.Log("screenWidth is "+ screenWidth + "screenHeight is " + screenHeight);
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
    /// 统一调整 UI 组件的大小（自动遍历 `adjustSize`）
    /// </summary>
    private void AdjustUIElementsSize()
    {
        if (adjustSize == null || adjustSize.Count == 0)
        {
            Debug.LogWarning("No UI elements assigned in adjustSize!");
            return;
        }

        float fontScaleFactor = 0.3f; // 文字缩放因子
        float iconScaleFactor = 0.2f; // 图标缩放因子

        foreach (var elementData in adjustSize)
        {
            if (elementData.element != null)
            {
                AdjustUIElementRecursive(elementData.element, fontScaleFactor, iconScaleFactor);
            }
        }
    }

    #region 
    private void AdjustUIElementRecursive(Transform element, float fontScaleFactor, float iconScaleFactor)
    {
        if (element == null) return;

        Debug.Log($"Adjusting: {element.name}");

        // **特判：如果是滑动条（Scrollbar 或其子组件，或者背景图片），则不调整大小**
        if (IsExcludedElement(element))
        {
            Debug.Log($"Skipping resizing for {element.name}");
            return;
        }

        // 计算缩放因子
        float textScale = CalculateScaleFactor(fontScaleFactor);
        float iconScale = CalculateScaleFactor(iconScaleFactor);

        // TODO:处理 Live2D 模型
        if (IsLive2DModel(element))
        {
            AdjustLive2DModelSizeAndPosition(element, iconScale);
        }
        else
        {
            // 调整字体大小
            AdjustTextSize(element, textScale);

            // 调整 UI 元素大小 & 位置
            AdjustRectTransform(element, iconScale);

            // 调整布局（LayoutGroup 组件）
            AdjustLayoutComponent(element, iconScale);
        }

        // 递归处理所有子对象
        foreach (Transform child in element)
        {
            AdjustUIElementRecursive(child, fontScaleFactor, iconScaleFactor);
        }
    }

    /// <summary>
    /// 判断是否需要跳过该 UI 元素（如 Scrollbar 及其子组件）
    /// </summary>
    private bool IsExcludedElement(Transform element)
    {
        string name = element.name.ToLower();
        return name.Contains("scrollbar") || name.Contains("sliding area") || name.Contains("handle");
    }

    /// <summary>
    /// 计算缩放因子
    /// </summary>
    private float CalculateScaleFactor(float scaleFactorMultiplier = 0.5f)
    {
        float widthFactor = Screen.width / referenceScreenWidth;
        float heightFactor = Screen.height / referenceScreenHeight;

        return 1 + (widthFactor * heightFactor - 1) * scaleFactorMultiplier;
    }

    /// <summary>
    /// 调整字体大小
    /// </summary>
    private void AdjustTextSize(Transform element, float textScale)
    {
        Text textComponent = element.GetComponent<Text>();
        if (textComponent != null)
        {
            textComponent.fontSize = Mathf.RoundToInt(textComponent.fontSize * textScale);
        }

        TMP_Text tmpTextComponent = element.GetComponent<TMP_Text>();
        if (tmpTextComponent != null)
        {
            tmpTextComponent.fontSize *= textScale;
        }
    }

    /// <summary>
    /// 调整 RectTransform 的大小和位置
    /// </summary>
    private void AdjustRectTransform(Transform element, float iconScale)
    {
        RectTransform rt = element.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = new Vector2(rt.sizeDelta.x * iconScale, rt.sizeDelta.y * iconScale);
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x * iconScale, rt.anchoredPosition.y * iconScale);
        }
    }

    /// <summary>
    /// 处理 Layout 组件的大小和间距
    /// </summary>
    private void AdjustLayoutComponent(Transform element, float iconScale)
    {
        HorizontalLayoutGroup horizontalLayout = element.GetComponent<HorizontalLayoutGroup>();
        if (horizontalLayout != null)
        {
            horizontalLayout.spacing *= iconScale;
            horizontalLayout.padding.left = Mathf.RoundToInt(horizontalLayout.padding.left * iconScale);
            horizontalLayout.padding.right = Mathf.RoundToInt(horizontalLayout.padding.right * iconScale);
            horizontalLayout.padding.top = Mathf.RoundToInt(horizontalLayout.padding.top * iconScale);
            horizontalLayout.padding.bottom = Mathf.RoundToInt(horizontalLayout.padding.bottom * iconScale);
        }

        VerticalLayoutGroup verticalLayout = element.GetComponent<VerticalLayoutGroup>();
        if (verticalLayout != null)
        {
            verticalLayout.spacing *= iconScale;
            verticalLayout.padding.left = Mathf.RoundToInt(verticalLayout.padding.left * iconScale);
            verticalLayout.padding.right = Mathf.RoundToInt(verticalLayout.padding.right * iconScale);
            verticalLayout.padding.top = Mathf.RoundToInt(verticalLayout.padding.top * iconScale);
            verticalLayout.padding.bottom = Mathf.RoundToInt(verticalLayout.padding.bottom * iconScale);
        }

        GridLayoutGroup gridLayout = element.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            gridLayout.cellSize = new Vector2(gridLayout.cellSize.x * iconScale, gridLayout.cellSize.y * iconScale);
            gridLayout.spacing = new Vector2(gridLayout.spacing.x * iconScale, gridLayout.spacing.y * iconScale);
        }
    }

    //TODO:Live2D位置与大小

    /// <summary>
    /// 判断是否是 Live2D 模型（根据 GameObject 名称或组件）
    /// </summary>
    private bool IsLive2DModel(Transform element)
    {
        string name = element.name.ToLower();
        return name.Contains("live2d") || name.Contains("model");
    }

    /// <summary>
    /// 调整 Live2D 模型的大小和位置
    /// </summary>
    private void AdjustLive2DModelSizeAndPosition(Transform model, float iconScale)
    {
        // **调整 Live2D 的缩放**
        model.localScale *= iconScale;
    }
    #endregion
}