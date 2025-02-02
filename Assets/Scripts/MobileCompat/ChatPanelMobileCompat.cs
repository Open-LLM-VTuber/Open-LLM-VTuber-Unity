using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatPanelMobileCompat : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        RectTransform rt = GetComponent<RectTransform>();

        if (rt != null && Camera.main != null)
        {
            // 获取屏幕的像素宽度和高度
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            // 计算世界单位下的屏幕尺寸
            float worldScreenHeight = Camera.main.orthographicSize * 2.0f;
            float worldScreenWidth = worldScreenHeight * (screenWidth / screenHeight);

            // **适配宽度**
            rt.sizeDelta = new Vector2(worldScreenWidth, rt.sizeDelta.y);

            // **修改 Pos X**
            rt.localPosition = new Vector3((worldScreenWidth - 1080f)/2.0f, rt.localPosition.y, rt.localPosition.z);
            
            //Debug.Log($"Chat Panel Updated: Width = {worldScreenWidth}, PosX = {rt.position.x}, screenWidth = {screenWidth}");
        }
        else
        {
            Debug.LogError("RectTransform 或 Main Camera 未找到！请确保 Canvas 处于 World Space 模式。");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
