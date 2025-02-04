using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

[System.Serializable]
public class UIElementOnlyWidth
{
    public Transform element; // UI 元素
    public float originalScreenWidth; // 原始屏幕宽度,默认是1080
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
    public float originalScreenWidth; // 原始屏幕宽度,默认是1080
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

        // 动态调用方法
        InvokeAdjustMethods(onlyWidth, "AdjustOnlyWidth");
        InvokeAdjustMethods(fullScreenWidth, "AdjustFullScreenWidth");
        InvokeAdjustMethods(spaceBetween, "AdjustSpaceBetween");
    }

    private void ComputeWorldScreenWidth()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float worldScreenHeight = Camera.main.orthographicSize * 2.0f;
        worldScreenWidth = worldScreenHeight * (screenWidth / screenHeight);
    }

    private void InvokeAdjustMethods<T>(List<T> elements, string methodName)
    {
        foreach (var elementData in elements)
        {
            var element = elementData.GetType().GetField("element").GetValue(elementData) as Transform;
            if (element == null) continue;

            RectTransform rt = element.GetComponent<RectTransform>();
            if (rt == null)
            {
                Debug.LogWarning($"{element.name} does not have a RectTransform component.");
                continue;
            }

            System.Reflection.MethodInfo method = GetType().GetMethod(methodName, 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (method != null)
            {
                var parameters = method.GetParameters();
                var args = new List<object> { rt };

                foreach (var param in parameters.Skip(1)) // Skip the first parameter (RectTransform)
                {
                    var field = elementData.GetType().GetField(param.Name, 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        args.Add(field.GetValue(elementData));
                    }
                    else
                    {
                        Debug.LogError($"Field {param.Name} not found in {elementData.GetType().Name}");
                    }
                }

                method.Invoke(this, args.ToArray());
            }
            else
            {
                Debug.LogError($"Method {methodName} not found in {GetType().Name}");
            }
        }
    }



    // **可被动态调用的方法**
    private void AdjustOnlyWidth(RectTransform element, float originalScreenWidth, float originalItemWidth)
    {
        element.sizeDelta = new Vector2(worldScreenWidth - (originalScreenWidth - originalItemWidth), element.sizeDelta.y);
    }

    private void AdjustSpaceBetween(RectTransform element, float originalScreenWidth, float originalItemWidth)
    {
        float originalItemMargin = originalScreenWidth - originalItemWidth;
        element.sizeDelta = new Vector2(worldScreenWidth - originalItemMargin, element.sizeDelta.y);

        HorizontalLayoutGroup layoutGroup = element.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup != null)
        {
            Transform[] buttons = element.Cast<Transform>().ToArray();
            float totalButtonWidth = buttons.Sum(button => button.GetComponent<RectTransform>()?.rect.width ?? 0);
            int buttonCount = buttons.Length;

            if (buttonCount > 1)
            {
                layoutGroup.spacing = (worldScreenWidth - originalItemMargin - totalButtonWidth) / (buttonCount - 1);
            }
            else
            {
                Debug.LogError("错误:图标数量<=1");
            }
        }
        else
        {
            Debug.LogError("Element does not have a HorizontalLayoutGroup component!");
        }
    }


    private void AdjustFullScreenWidth(RectTransform rt)
    {
        rt.sizeDelta = new Vector2(worldScreenWidth, rt.sizeDelta.y);
        rt.localPosition = new Vector3(0, rt.localPosition.y, rt.localPosition.z);
    }
}
