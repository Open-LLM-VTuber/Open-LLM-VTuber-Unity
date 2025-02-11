using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class UIElementOnlyWidth
{
    public Transform element; // UI 元素
    public float originalScreenWidth = 1080; // 原始屏幕宽度
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

public class UIElementMobileCompat : MonoBehaviour
{
    public List<UIElementOnlyWidth> onlyWidth = new List<UIElementOnlyWidth>(); // onlyWidth, Inspector 可配置
    public List<UIElementFullScreenWidth> fullScreenWidth = new List<UIElementFullScreenWidth>(); // fullScreenWidth, Inspector 可配置
    public List<UIElementSpaceBetween> spaceBetween = new List<UIElementSpaceBetween>(); // Inspector 可配置

    private float worldScreenWidth;

    void Start()
    {
        if (Camera.main == null)
        {
            Debug.LogError("Main Camera not found! Ensure the Canvas is in World Space mode.");
            return;
        }

        ComputeWorldScreenWidth();
        AdjustOnlyWidthElements();
        AdjustFullScreenWidthElements();
        AdjustSpaceBetweenElements();
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
}