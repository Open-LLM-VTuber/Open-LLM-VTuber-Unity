using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using static UnityEditor.Progress;

public class OpenMessageWindow : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button openButton;
    [SerializeField] private ButtonPressEffect buttonPressEffect;

    [Header("Configuration")]
    [SerializeField] private string titleString = "Select Option";
    [SerializeField] private string introString = "Please choose an option:";

    private void Start()
    {

        if (buttonPressEffect == null)
        {
            Debug.LogError("ButtonPressEffect component not found on the button!");
            return;
        }

        // 监听实例化完成回调
        buttonPressEffect.OnSpawnComplete += OnOptionsWindowSpawned;

    }

    private void OnOptionsWindowSpawned(GameObject optionsWindow)
    {
        // 初始化窗口信息
        InitWindow(optionsWindow);
    }

    private void InitWindow(GameObject optionsWindow)
    {
        PopupWindow popupWindowScript = optionsWindow.GetComponent<PopupWindow>();
        if (popupWindowScript == null)
        {
            Debug.LogError("PopupWindow component not found on the options window!");
            return;
        }
        popupWindowScript.titleString = titleString;
        popupWindowScript.introString = introString;
        popupWindowScript.Setup();
    }

}

// JSON 数据结构