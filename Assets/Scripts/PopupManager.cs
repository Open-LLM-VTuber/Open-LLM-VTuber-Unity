using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    // UI 元素
    public GameObject popupPanel; // 弹窗
    public GameObject backgroundMask; // 背景遮罩
    public Button closeButton; // 右上角的 X 按钮
    public Button confirmButton; // 弹窗中的确认按钮
    public Button cancelButton; // 弹窗中的取消按钮

    void Start()
    {
        // 绑定按钮事件
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
        }

        // 默认隐藏弹窗和背景
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }

        if (backgroundMask != null)
        {
            backgroundMask.SetActive(false);
        }
    }

    void OnCloseButtonClicked()
    {
        // 显示弹窗和背景
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }

        if (backgroundMask != null)
        {
            backgroundMask.SetActive(true);
        }
    }

    void OnConfirmButtonClicked()
    {
        // 确认退出逻辑
        Debug.Log("退出游戏");
        Application.Quit(); // 退出游戏
    }

    void OnCancelButtonClicked()
    {
        // 隐藏弹窗和背景
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }

        if (backgroundMask != null)
        {
            backgroundMask.SetActive(false);
        }
    }
}