using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class AlwaysOnTopWindow : MonoBehaviour
{
    // Windows API 函数
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    // 常量
    private const int HWND_TOPMOST = -1;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;

    void Start()
    {
        // 获取当前窗口句柄
        IntPtr hWnd = GetActiveWindow();

        // 设置窗口置顶
        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
    }
}