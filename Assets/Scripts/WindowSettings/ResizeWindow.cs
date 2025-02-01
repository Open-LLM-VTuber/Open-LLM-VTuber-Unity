using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class ResizeWindow : MonoBehaviour
{
    // Windows API 函数
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    // 常量
    private const uint SWP_NOZORDER = 0x0004;

    void Start()
    {
        // 获取当前窗口句柄
        IntPtr hWnd = GetActiveWindow();

        // 设置窗口大小为 350x800
        SetWindowPos(hWnd, 0, 0, 0, 350, 800, SWP_NOZORDER);
    }
}