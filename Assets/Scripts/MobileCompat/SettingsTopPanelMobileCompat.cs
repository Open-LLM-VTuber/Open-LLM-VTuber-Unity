using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsTopPanelMobileCompat : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RectTransform rt = GetComponent<RectTransform>();

        if (rt != null && Camera.main != null)
        {
            // 获取屏幕的像素宽度
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            // 计算屏幕的世界宽度 (基于相机视口)
            float worldScreenHeight = Camera.main.orthographicSize * 2.0f;
            float worldScreenWidth = worldScreenHeight * (screenWidth / screenHeight);

            // 设置组件的宽度等于屏幕宽度
            rt.sizeDelta = new Vector2(worldScreenWidth, rt.sizeDelta.y);

            // 保持顶部位置 (调整Y轴位置不变)
            // rt.position = new Vector3(0, rt.position.y, rt.position.z);

            // Debug.Log($"Updated Width: {worldScreenWidth}, Screen Width: {screenWidth}");
        }
        else
        {
            Debug.LogError("RectTransform or Main Camera not found! Ensure the Canvas is in World Space mode.");
        }
    }
}
