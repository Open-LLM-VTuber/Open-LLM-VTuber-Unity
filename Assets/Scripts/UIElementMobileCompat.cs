using UnityEngine;
using UnityEngine.UI;

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

        foreach (Transform uiElement in uiElements)
        {
            if (uiElement == null) continue;
            RectTransform rt = uiElement.GetComponent<RectTransform>();

            if (rt != null)
            {
                if(uiElement.name == "Input")
                {
                    rt.sizeDelta = new Vector2(worldScreenWidth-90f, rt.sizeDelta.y);
                }
                else if(uiElement.name == "InputField (TMP)")
                {
                    rt.sizeDelta = new Vector2(worldScreenWidth-280f, rt.sizeDelta.y);
                }
                else if(uiElement.name == "Others")
                {
                    rt.sizeDelta = new Vector2(worldScreenWidth-105f, rt.sizeDelta.y);
                }
                else
                {
                    rt.sizeDelta = new Vector2(worldScreenWidth, rt.sizeDelta.y);

                }

                if (uiElement.name == "ChatPanel" || uiElement.name == "SettingsTopPanel" ||uiElement.name =="TopPanel" || uiElement.name == "UICanvas")
                {
                    //位置居中
                    rt.localPosition = new Vector3(0, rt.localPosition.y, rt.localPosition.z);
                }
                
                //Debug.Log($"Updated {uiElement.name}: Width = {worldScreenWidth}, PosX = {rt.localPosition.x}");
            }
            else
            {
                Debug.LogError($"{uiElement.name} does not have a RectTransform component.");
            }

            // ✅ 正确调整间距
            if (uiElement.name == "Others")
            {
                HorizontalLayoutGroup layoutGroup = uiElement.GetComponent<HorizontalLayoutGroup>(); // ✅ 获取 `Others` 的 `HorizontalLayoutGroup`
                if (layoutGroup != null)
                {
                    layoutGroup.spacing = (worldScreenWidth-90f-285f)/3.0f; // ✅ 这里修改间距（可以调整数值）       
                    //Debug.Log("调整间距成功: " + layoutGroup.spacing);
                }
                else
                {
                    Debug.LogError("Others 组件上没有 HorizontalLayoutGroup!");
                }
            }
        }
    }
}
