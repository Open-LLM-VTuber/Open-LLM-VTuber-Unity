using UnityEngine;

public class SystemScale : MonoBehaviour
{
    void Start()
    {
        // 获取主显示器的 DPI
        float dpi = Display.main.systemHeight / (Display.main.renderingHeight / Screen.dpi);

        // 如果 DPI 为 0，说明设备不支持 DPI 检测，使用默认值 96
        if (dpi == 0)
        {
            dpi = 96f;
            Debug.LogWarning("无法获取屏幕 DPI，使用默认值 96。");
        }

        // 计算系统缩放比例
        float systemScale = dpi / 96f; // 96 是标准 DPI
        Debug.Log("系统缩放比例: " + systemScale);
    }
}