using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpenMessageWindow : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button openButton;
    [SerializeField] private ButtonPressEffect buttonPressEffect;

    [Header("Single Configuration")]
    [SerializeField] private string titleString = "Select Option";
    [SerializeField] private string introString = "Please choose an option:";

    [Header("Multi Configuration")]
    [SerializeField] private TMP_Text[] Keys;
    [SerializeField] private TMP_Text[] Values;

    void Start()
    {

        if (buttonPressEffect == null)
        {
            Debug.LogError("ButtonPressEffect component not found on the button!");
            return;
        }

        // 监听实例化完成回调
        buttonPressEffect.OnSpawnComplete += OnOptionsWindowSpawned;

    }

    void OnDestroy()
    {
        buttonPressEffect.OnSpawnComplete -= OnOptionsWindowSpawned;
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

        if (Keys.Length != Values.Length) {
            Debug.LogWarning("Keys.Length != Values.Length");
        }
        
        var tempIntro = new string(introString);
        for (int i = 0; i < Math.Min(Keys.Length, Values.Length); i++) {
            tempIntro += $"\n{Keys[i].text}: \n{Values[i].text}";
        }

        popupWindowScript.titleString = titleString;
        popupWindowScript.introString = tempIntro;

        popupWindowScript.Setup();
    }

}

// JSON 数据结构