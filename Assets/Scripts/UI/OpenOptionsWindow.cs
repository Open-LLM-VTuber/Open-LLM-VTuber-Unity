using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;

public class OpenOptionsWindow : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button openButton;
    [SerializeField] private ButtonPressEffect buttonPressEffect;
    [SerializeField] private TMP_Text currentOption;

    [Header("Configuration")]
    [SerializeField] private bool isLocal = true;
    [SerializeField] private bool getFolderName = false;
    [SerializeField] private bool getExtension = false;
    [SerializeField] private string titleString = "Select Option";
    [SerializeField] private string introString = "Please choose an option:";
    [SerializeField] private string localSearchPath = "Data";
    [SerializeField] private List<string> fileSuffixes = new List<string> { "*.txt" };
    [SerializeField] private string remoteDataUrl = "https://api.example.com/options";
    [SerializeField] private string settingName = "BaseUrl";
    [SerializeField] private int defaultIndex = 0;

    // 要设置的选项集合
    private List<string> options;
    private void Start()
    {
        if (currentOption != null)
        {
            // 如果要显示当前设置的值
            string settingsValue = SettingsManager.Instance.GetSetting(settingName);
            currentOption.text = settingsValue;
        }
        

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
            LoadLocalOptions();
        }
        else
        {
            StartCoroutine(FetchRemoteOptions());
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

    private void LoadLocalOptions()
    {
        options = new List<string>();

        var fullPath = Path.Combine(Application.persistentDataPath, localSearchPath);

        if (!Directory.Exists(fullPath))
        {
            Debug.LogError($"Directory not found: {fullPath}");
            return;
        }

        var filePaths = new List<string>();

        foreach (var suffix in fileSuffixes)
        {
            var files = Directory.EnumerateFiles(fullPath, suffix, SearchOption.AllDirectories);

            if (getFolderName)
            {
                filePaths.AddRange(files.Select(Path.GetDirectoryName).Select(Path.GetFileName));
            }
            else
            {
                filePaths.AddRange(files.Select(file => getExtension ?
                    Path.GetFileName(file) : Path.GetFileNameWithoutExtension(file))
                );
            }
        }

        options = filePaths.Distinct().ToList();

        // 从 SettingsManager 中获取对应设置的值
        string settingsValue = SettingsManager.Instance.GetSetting(settingName);

        if (string.IsNullOrEmpty(settingsValue))
        {
            Debug.LogWarning($"Setting '{settingName}' not found. Using default index 0.");
            defaultIndex = 0;
        }
        else
        {
            // 在 options 中查找匹配的索引
            defaultIndex = options.IndexOf(settingsValue);

            if (defaultIndex == -1)
            {
                Debug.LogWarning($"Setting '{settingsValue}' not found in options. Using default index 0.");
                defaultIndex = 0;
            }
        }
    }

    // 发起网络请求并显示选项窗口
    private IEnumerator FetchRemoteOptions()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(remoteDataUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                OptionsData optionsData = JsonUtility.FromJson<OptionsData>(json);
                options = optionsData.options;
                defaultIndex = optionsData.defaultIndex;
            }
            else
            {
                Debug.LogError("Network request failed: " + request.error);
            }
        }
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
    // 实例化完成回调
    private void OnOptionsWindowSpawned(GameObject optionsWindow)
    {
        // 初始化窗口信息
        InitWindow(optionsWindow);
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
            defaultIndex = defaultIndex // 假设默认选项是第 1 个
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