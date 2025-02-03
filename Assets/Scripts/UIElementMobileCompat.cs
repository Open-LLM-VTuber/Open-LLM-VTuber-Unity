using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UIElementMobileCompat : MonoBehaviour
{
    public Transform[] uiElements; // List of UI elements to adapt

    void Start()
    {
        if (Camera.main == null)
        {
            Debug.LogError("Main Camera not found! Ensure the Canvas is in World Space mode.");
            return;
        }

        // Get screen pixel width and height
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Compute screen dimensions in world units
        float worldScreenHeight = Camera.main.orthographicSize * 2.0f;
        float worldScreenWidth = worldScreenHeight * (screenWidth / screenHeight);

        //找到这些元素，就初始化成元素对象，否则初始化成空的
        RectTransform chatPanel = uiElements.FirstOrDefault(e => e.name == "ChatPanel")?.GetComponent<RectTransform>();
        RectTransform input = uiElements.FirstOrDefault(e => e.name == "Input")?.GetComponent<RectTransform>();
        RectTransform others = uiElements.FirstOrDefault(e => e.name == "Others")?.GetComponent<RectTransform>();
        RectTransform inputField = uiElements.FirstOrDefault(e => e.name == "InputField (TMP)")?.GetComponent<RectTransform>();

        if (chatPanel == null || input == null || others == null || inputField == null)
        {
            Debug.LogError("One or more required UI elements are missing!");
            return;
        }

        float inputMargin = chatPanel.rect.width - input.rect.width; // 90 的来源(chatpanel的宽度-input的宽度,输入框行的左右边距)
        float othersMargin = chatPanel.rect.width - others.rect.width; // 105 的来源(chatpanel的宽度-others的宽度，others行的左右边距)
        // Debug.Log($"inputMargin: {inputMargin}, othersMargin: {othersMargin}");

        // 计算原始组件宽度
        float inputOriginalWidth = input.rect.width;
        float inputFieldOriginalWidth = inputField.rect.width;

        // 计算 InputField 需要减去的边距
        float inputFieldMargin = inputOriginalWidth - inputFieldOriginalWidth;

        float totalButtonWidth = 0f;//计算按钮宽度总和
        int buttonCount = 0;//计算按钮数量

        foreach (Transform uiElement in uiElements)
        {
            if (uiElement == null) continue;
            
            RectTransform rt = uiElement.GetComponent<RectTransform>();

            if (rt != null)
            {
                if (uiElement.name == "Input")
                {
                    rt.sizeDelta = new Vector2(worldScreenWidth - inputMargin, rt.sizeDelta.y);
                }
                else if (uiElement.name == "InputField (TMP)")
                {
                    float inputFieldMinus = inputFieldMargin + inputMargin; 
                    rt.sizeDelta = new Vector2(worldScreenWidth - inputFieldMinus, rt.sizeDelta.y);//调整输入框的宽度
                }
                else if (uiElement.name == "Others")
                {
                    rt.sizeDelta = new Vector2(worldScreenWidth - othersMargin, rt.sizeDelta.y);
                }
                else
                {
                    rt.sizeDelta = new Vector2(worldScreenWidth, rt.sizeDelta.y);
                }

                if (uiElement.name == "Others")
                {
                    HorizontalLayoutGroup layoutGroup = uiElement.GetComponent<HorizontalLayoutGroup>();
                    if (layoutGroup != null)
                    {
                        Transform[] buttons = uiElement.Cast<Transform>().ToArray();//获取子对象：所有按钮

                        foreach (Transform button in buttons)
                        {
                            RectTransform buttonRT = button.GetComponent<RectTransform>();
                            if (buttonRT != null)
                            {
                                totalButtonWidth += buttonRT.rect.width;
                                buttonCount++;
                            }
                        }

                        if (buttonCount > 1)
                        {
                            layoutGroup.spacing = (worldScreenWidth - othersMargin - totalButtonWidth) / (buttonCount - 1);
                        }
                    }
                    else
                    {
                        Debug.LogError("Others 组件上没有 HorizontalLayoutGroup!");
                    }
                }

                if (uiElement.name == "Live2D Canvas" || uiElement.name == "BackgroundPanel" || uiElement.name == "Background Image" || uiElement.name == "ChatPanel" || uiElement.name == "SettingsTopPanel" || uiElement.name == "TopPanel" || uiElement.name == "UICanvas")
                {
                    //居中
                    rt.localPosition = new Vector3(0, rt.localPosition.y, rt.localPosition.z);
                }
            }
            else
            {
                Debug.LogError($"{uiElement.name} does not have a RectTransform component.");
            }
        }
    }
}
