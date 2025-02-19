using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class UIElementOnlyWidth
{
    public Transform element;
    public LayoutOption layoutOption = LayoutOption.Vertical;

    [Tooltip("仅当 LayoutOption 为 Vertical 时有效")]
    public float OriginalItemWidth; // 仅当 Vertical 时使用

    [Tooltip("仅当 LayoutOption 为 Horizontal 时有效")]
    public float Spacing; // 仅当 Horizontal 时使用
}



public enum LayoutOption
{
    Vertical,
    Horizontal
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
   // public float originalScreenWidth = 1080; // 原始屏幕宽度
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
        AdjustUIElementsSize();
        AdjustFullScreenWidthElements();
        // AdjustIconSizeElements();
        // AdjustFontSizeElements();
        AdjustOnlyWidthElements();
        AdjustSpaceBetweenElements();
    }

    private void ComputeWorldScreenWidth()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        //Debug.Log("screenWidth is "+ screenWidth + "screenHeight is " + screenHeight);
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

            if (elementData.layoutOption == LayoutOption.Vertical)
            {
                rt.sizeDelta = new Vector2(worldScreenWidth - (referenceScreenWidth - elementData.OriginalItemWidth), rt.sizeDelta.y);               
            }
            else
            {
                RectTransform parentRt = rt.parent as RectTransform;
                if (parentRt == null)
                {
                    Debug.LogWarning($"{element.name} does not have a valid parent RectTransform.");
                    continue;
                }

                // 计算同级组件的总宽度（排除自身）
                float totalSiblingWidth = 0;
                foreach (Transform sibling in parentRt)
                {
                    if (sibling == rt.transform) continue; // ✅ 确保排除自身

                    RectTransform siblingRt = sibling.GetComponent<RectTransform>();
                    if (siblingRt != null)
                    {
                        totalSiblingWidth += siblingRt.sizeDelta.x; // ✅ 使用 sizeDelta 而不是 rect.width
                    }
                }

                // 计算剩余可用宽度
                float parentWidth = parentRt.rect.width; // ✅ 使用 rect.width 作为父级宽度
                float availableWidth = parentWidth - totalSiblingWidth - elementData.Spacing;

                if (availableWidth < 0) availableWidth = 0; // 避免负值

                // 设置新宽度
                rt.sizeDelta = new Vector2(availableWidth, rt.sizeDelta.y);
            }
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

            rt.sizeDelta = new Vector2(worldScreenWidth - (referenceScreenWidth - elementData.originalItemWidth), rt.sizeDelta.y);

            HorizontalLayoutGroup layoutGroup = element.GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup != null)
            {
                Transform[] buttons = element.Cast<Transform>().ToArray();
                float totalButtonWidth = buttons.Sum(button => button.GetComponent<RectTransform>()?.rect.width ?? 0);
                int buttonCount = buttons.Length;

                if (buttonCount > 1)
                {
                    layoutGroup.spacing = (worldScreenWidth - (referenceScreenWidth - elementData.originalItemWidth) - totalButtonWidth) / (buttonCount - 1);
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

        float fontScaleFactor = 0.2f; // 文字缩放因子
        float iconScaleFactor = 0.2f; // 图标缩放因子
        float live2dScaleFactor = 0.1f; // live2d缩放因子

        foreach (var elementData in adjustSize)
        {
            if (elementData.element != null)
            {
                AdjustUIElementRecursive(elementData.element, fontScaleFactor, iconScaleFactor, live2dScaleFactor);
            }
        }
    }

    #region 

    private void AdjustUIElementRecursive(Transform element, float fontScaleFactor, float iconScaleFactor, float live2dScaleFactor)
    {
        if (element == null) return;

        // 计算缩放因子
        float textScale = CalculateScaleFactor(fontScaleFactor);
        float iconScale = CalculateScaleFactor(iconScaleFactor);
        float live2dScale = CalculateScaleFactor(live2dScaleFactor);

        // 处理 Live2D 模型
        if (IsLive2DModel(element))
        {
            AdjustLive2DModelSizeAndPosition(element, live2dScale);
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
            AdjustUIElementRecursive(child, fontScaleFactor, iconScaleFactor,live2dScale);
        }
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
            //滚动条组件，特殊处理
            List<string> scrollParentNames = new List<string>
            {
                "Settings Side Panel"
            };

            if (scrollParentNames.Contains(element.name))
            {
                rt.sizeDelta = new Vector2(rt.sizeDelta.x * iconScale, rt.sizeDelta.y);
            }
            //普遍情况
            else{
                rt.sizeDelta = new Vector2(rt.sizeDelta.x * iconScale, rt.sizeDelta.y * iconScale);
            }
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


