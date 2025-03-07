using UnityEngine;

public class FPS : MonoBehaviour
{
    /// <summary>
    /// 上一次更新帧率的时间
    /// </summary>
    private float m_lastUpdateShowTime = 0f;

    /// <summary>
    /// 更新显示帧率的时间间隔
    /// </summary>
    private readonly float m_updateTime = 0.05f;

    /// <summary>
    /// 帧数
    /// </summary>
    private int m_frames = 0;

    /// <summary>
    /// 帧间间隔
    /// </summary>
    private float m_frameDeltaTime = 0;

    /// <summary>
    /// 当前帧率
    /// </summary>
    private float m_FPS = 0;

    /// <summary>
    /// 显示FPS的区域
    /// </summary>
    private Rect m_fps, m_dtime;

    /// <summary>
    /// GUI样式
    /// </summary>
    private GUIStyle m_style = new GUIStyle();

    void Awake()
    {
        // 关闭 VSync
        QualitySettings.vSyncCount = 0; 
        // 根据平台设置目标帧率
        SetFrameRateBasedOnPlatform();
    }

    void Start()
    {
        m_lastUpdateShowTime = Time.realtimeSinceStartup;
        m_fps = new Rect(0, 0, 200, 50); // 调整显示区域大小
        m_dtime = new Rect(0, 50, 200, 50);
        m_style.fontSize = 30; // 调整字体大小
        m_style.normal.textColor = Color.blue;
    }

    void Update()
    {
        // 计算帧率
        m_frames++;
        if (Time.realtimeSinceStartup - m_lastUpdateShowTime >= m_updateTime)
        {
            m_FPS = m_frames / (Time.realtimeSinceStartup - m_lastUpdateShowTime);
            m_frameDeltaTime = (Time.realtimeSinceStartup - m_lastUpdateShowTime) / m_frames;
            m_frames = 0;
            m_lastUpdateShowTime = Time.realtimeSinceStartup;
        }
    }

    void OnGUI()
    {
        // 显示FPS和帧间间隔
        GUI.Label(m_fps, "FPS: " + m_FPS.ToString("F2"), m_style); // 格式化显示两位小数
        GUI.Label(m_dtime, "间隔: " + m_frameDeltaTime.ToString("F4") + "s", m_style);
    }

    /// <summary>
    /// 根据平台设置目标帧率
    /// </summary>
    private void SetFrameRateBasedOnPlatform()
    {
        
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
            case RuntimePlatform.IPhonePlayer:
                Application.targetFrameRate = 60; // 移动设备设置为 30 FPS
                Debug.Log("移动设备设置为 60 FPS");
                break;
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.LinuxPlayer:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.LinuxEditor:
            case RuntimePlatform.OSXEditor:
                Application.targetFrameRate = 120; // PC/Mac设置为 60 FPS
                Debug.Log("桌面运行设备设置为 120 FPS");
                break;

            default:
                Application.targetFrameRate = -1; // 其他平台不限制帧率
                Debug.Log("其他设备不限制帧率");
                break;
        }
    }
}