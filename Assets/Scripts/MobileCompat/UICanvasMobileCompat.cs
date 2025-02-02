using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICanvasMobileCompat : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // 获取 Canvas 组件
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
        {
            RectTransform rt = canvas.GetComponent<RectTransform>();

            // 获取屏幕宽度并转换为世界单位
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            // 计算屏幕的世界宽度（基于摄像机视口大小）
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                float worldScreenHeight = mainCamera.orthographicSize * 2.0f;
                float worldScreenWidth = worldScreenHeight * (screenWidth / screenHeight);

                // 设置 Canvas 的宽度为屏幕宽度
                rt.sizeDelta = new Vector2(worldScreenWidth, rt.sizeDelta.y);
            }
            else
            {
                Debug.LogError("Main Camera not found!");
            }
        }
        else
        {
            Debug.LogError("Canvas not found or not in World Space mode!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
