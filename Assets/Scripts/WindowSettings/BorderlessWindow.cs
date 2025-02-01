using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class BorderlessWindow : MonoBehaviour
{
    // Windows API 函数
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll")]
    private static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    // 常量
    private const int GWL_STYLE = -16;
    private const uint WS_POPUP = 0x80000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint SWP_FRAMECHANGED = 0x0020;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;

    void Start()
    {
        // 获取当前窗口句柄
        IntPtr hWnd = GetActiveWindow();

        // 设置窗口样式为无边框
        SetWindowLong(hWnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);

        // 应用窗口样式更改
        SetWindowPos(hWnd, 0, 0, 0, 0, 0, SWP_FRAMECHANGED | SWP_NOMOVE | SWP_NOSIZE);
    }
}