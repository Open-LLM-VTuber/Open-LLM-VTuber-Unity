using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
[System.Serializable]
public class Setting
{
    public string Key;
    public string Value;
}

[System.Serializable]
public class GameSettingsDict
{
    public List<Setting> Settings = new List<Setting>();
}

public class SettingsManager : MonoBehaviour
{
    private string savePath;
    private GameSettingsDict currentSettings;

    // 单例实例
    public static SettingsManager Instance { get; private set; }

    private void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 确保场景切换时不被销毁
            Initialize();
        }
        else
        {
            Destroy(gameObject); // 如果已存在实例，销毁新的实例
        }
    }

    private void Initialize()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }
#endif
        savePath = Path.Combine(Application.persistentDataPath, "settings.json");
        currentSettings = LoadSettings();

        // 如果配置文件不存在，则初始化默认设置
        if (currentSettings.Settings.Count == 0)
        {
            InitializeDefaultSettings();
            SaveSettings();
        }

        Debug.Log("SettingsManager Initialized. Path: " + savePath);
    }

    public void SaveSettings()
    {
        string json = JsonUtility.ToJson(currentSettings);
        File.WriteAllText(savePath, json);
        Debug.Log("Settings saved to: " + savePath);
    }

    public GameSettingsDict LoadSettings()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<GameSettingsDict>(json);
        }
        else
        {
            Debug.Log("No settings file found, using default settings.");
            return new GameSettingsDict();
        }
    }

    // 添加默认设置
    private void InitializeDefaultSettings()
    {
        currentSettings.Settings.Clear(); // 清空现有设置

        // 初始化默认设置
        AddSetting("ResolutionWidth", "1920");
        AddSetting("ResolutionHeight", "1080");
        AddSetting("BackgroundPath", "ceiling-window-room-night.jpeg");
        AddSetting("Live2DModelName", "shizuku.model.json");
        AddSetting("I18n", "简体中文");
        AddSetting("BackgroundUrl", string.Empty);
        AddSetting("WebSocketUrl", "ws://127.0.0.1:12393/client-ws");
        AddSetting("BaseUrl", "http://127.0.0.1:12393");
        AddSetting("SpeechProbThrehold", "97");
        AddSetting("NegativeSpeechThrehold", "15");
        AddSetting("RedemptionFrames", "15");
    }

    // 添加设置
    public void AddSetting(string key, string value)
    {
        if (currentSettings.Settings.Exists(s => s.Key == key))
        {
            Debug.LogError($"Setting with key '{key}' already exists.");
            return;
        }
        currentSettings.Settings.Add(new Setting { Key = key, Value = value });
        SaveSettings();
    }

    // 获取设置
    public string GetSetting(string key)
    {
        Setting setting = currentSettings.Settings.Find(s => s.Key == key);
        return setting != null ? setting.Value : null;
    }

    // 更新设置
    public void UpdateSetting(string key, string newValue)
    {
        Setting setting = currentSettings.Settings.Find(s => s.Key == key);
        if (setting != null)
        {
            setting.Value = newValue;
            SaveSettings();
        }
        else
        {
            Debug.LogError($"Setting with key '{key}' not found.");
        }
    }
}