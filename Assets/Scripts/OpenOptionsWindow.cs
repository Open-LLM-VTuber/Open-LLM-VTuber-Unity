using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening.Plugins.Options;

public class OpenOptionsWindow : MonoBehaviour
{
    public Button openButton; // 打开选项窗口的按钮

    [Header("Network")]
    public bool isLocal = true;

    [Header("Options")]
    public List<string> options = new List<string>(); // 选项文本列表

    private ButtonPressEffect buttonPressEffect; // 引用 ButtonPressEffect 组件

    private void Start()
    {
        // 获取 ButtonPressEffect 组件
        buttonPressEffect = openButton.GetComponent<ButtonPressEffect>();
        if (buttonPressEffect == null)
        {
            Debug.LogError("ButtonPressEffect component not found on the button!");
            return;
        }

        // 监听实例化完成回调
        buttonPressEffect.OnSpawnComplete += OnOptionsWindowSpawned;
    }


    // 按钮点击事件
    private void GetOptions()
    {   if (isLocal)
        {
            LocallyShowOptions();
        }
        else
        {
            StartCoroutine(FetchDataAndShowOptions());
        }
    }

    private void OnDestroy()
    {
        // 取消监听
        if (buttonPressEffect != null)
        {
            buttonPressEffect.OnSpawnComplete -= OnOptionsWindowSpawned;
        }
    }

    private void LocallyShowOptions()
    {
        OptionsData data = new OptionsData
        {
            options = options,
            defaultIndex = 2
        };

        string json = JsonUtility.ToJson(data, prettyPrint: true); // 格式化 JSON
        Debug.Log(json);
        ProcessJsonData(json);
    }

    // 发起网络请求并显示选项窗口
    private IEnumerator FetchDataAndShowOptions()
    {
        // 模拟网络请求
        string url = "https://example.com/api/options"; // 替换为实际 API 地址
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                ProcessJsonData(json);
            }
            else
            {
                Debug.LogError("Network request failed: " + request.error);
            }
        }
    }

    // 解析 JSON 数据并显示选项窗口
    private void ProcessJsonData(string json)
    {
        // 解析 JSON
        OptionsData optionsData = JsonUtility.FromJson<OptionsData>(json);
        // 设置选项列表
        options = optionsData.options;
    }

    // 实例化完成回调
    private void OnOptionsWindowSpawned(GameObject optionsWindow)
    {
        // 得到选项
        GetOptions();
        // 获取选项窗口脚本
        ScrollViewSelection selectionScript = optionsWindow.GetComponent<ScrollViewSelection>();
        if (selectionScript == null)
        {
            Debug.LogError("ScrollViewSelection component not found on the options window!");
            return;
        }

        // 设置选项列表
        selectionScript.options = options;

        // 设置默认选项
        OptionsData optionsData = new OptionsData
        {
            options = options,
            defaultIndex = 0 // 假设默认选项是第 1 个
        };

        // 初始化选项窗口
        selectionScript.InitializeButtons();

        if (optionsData.defaultIndex >= 0 && optionsData.defaultIndex < optionsData.options.Count)
        {
            if (selectionScript.isMultiSelect)
            {
                selectionScript.ToggleMultiSelect(optionsData.defaultIndex);
            }
            else
            {
                selectionScript.SetSingleSelect(optionsData.defaultIndex);
            }
        }
    }
}

// JSON 数据结构
[System.Serializable]
public class OptionsData
{
    public List<string> options; // 选项列表
    public int defaultIndex; // 默认选项的索引
}