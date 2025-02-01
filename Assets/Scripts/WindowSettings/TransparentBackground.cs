using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class TransparentBackground : MonoBehaviour
{
    // 导入 user32.dll 库以使用 Windows API 函数
    [DllImport("user32.dll")]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    // 定义一个结构来存储窗口边框的边距大小
    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    // 导入 user32.dll 以获取活动窗口句柄 (HWND)
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    // 导入 Dwmapi.dll 以将窗口边框扩展到客户区域
    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    // 导入 user32.dll 以修改窗口属性
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    // 导入 user32.dll 以设置窗口位置
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    // 导入 user32.dll 以设置分层窗口属性 (透明度)
    [DllImport("user32.dll")]
    static extern int SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);

    // 代码中使用的常量和变量
    const int GWL_EXSTYLE = -20;  // 修改窗口样式的索引
    const uint WS_EX_LAYERED = 0x00080000;  // 分层窗口的扩展样式
    const uint WS_EX_TRANSPARENT = 0x00000020;  // 透明窗口的扩展样式
    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);  // 窗口插入位置（始终置顶）
    const uint LWA_COLORKEY = 0x00000001;  // 设置颜色键的标志（用于透明度）
    private IntPtr hWnd;  // 活动窗口的句柄

    private void Start()
    {
        // 显示一个消息框（仅用于演示目的）
        // MessageBox(new IntPtr(0), "Hello world", "Hello Dialog", 0);

#if !UNITY_EDITOR
        // 获取活动窗口的句柄
        hWnd = GetActiveWindow();
 
        // 创建一个边距结构来定义边框大小
        MARGINS margins = new MARGINS { 
            cxLeftWidth = -1
        };
 
        // 将窗口边框扩展到客户区域（玻璃效果）
        DwmExtendFrameIntoClientArea(hWnd, ref margins);
 
        // 将窗口样式设置为分层和透明
        //SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);
 
        // 设置窗口颜色键（用于透明度）
        SetLayeredWindowAttributes(hWnd, 0, 0, LWA_COLORKEY);
 
        // 将窗口位置设置为始终置顶
        SetWindowPos(hWnd, HWND_TOPMOST, 100, 100, 40, 40, 0);
#endif

        // 允许应用在后台运行
        Application.runInBackground = true;
    }
}