using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

[Serializable]
public class GeneralSettings
{
    public string ResolutionWidth { get; set; } = "1920";
    public string ResolutionHeight { get; set; } = "1080";
    public bool UseCameraBackground { get; set; } = false;
    public bool ShowSubtitle { get; set; } = true;
    public string BackgroundPath { get; set; } = "ceiling-window-room-night.jpeg";
    public string Live2DModelName { get; set; } = "shizuku";
    public string I18n { get; set; } = "简体中文";
    public string BackgroundUrl { get; set; } = string.Empty;
    public string WebSocketUrl { get; set; } = "ws://127.0.0.1:12393/client-ws";
    public string BaseUrl { get; set; } = "http://127.0.0.1:12393";
}

[Serializable]
public class Live2DSettings
{
    public bool PointerInteractive { get; set; } = false;
    public bool EnableScrollToResize { get; set; } = false;
}

[Serializable]
public class ASRSettings 
{
    public bool AutoStopMicWhenAIStartSpeaking { get; set; } = false;
    public bool AutoStopMicWhenConversationEnd { get; set; } = false;
    public bool AutoStopMicWhenAIInterrupted { get; set; } = false;
    public string SpeechProbThreshold { get; set; } = "97";
    public string NegativeSpeechThreshold { get; set; } = "15";
    public string RedemptionFrames { get; set; } = "15";
}


[Serializable]
public class AgentSettings
{
    public bool AISpeakActively { get; set; } = false;
    public bool TriggerAIToSpeak { get; set; } = false;
}


[Serializable]
public class AudioSettings
{
    public string Volume { get; set; } = "100";
    public bool Mute { get; set; } = false;

}

[Serializable]       
public class GameSettings
{
    public GeneralSettings General { get; set; } = new GeneralSettings();
    public Live2DSettings Live2D { get; set; } = new Live2DSettings();
    public ASRSettings ASR { get; set; } = new ASRSettings();
    public AudioSettings Audio { get; set; } = new AudioSettings();
    public AgentSettings Agent { get; set; } = new AgentSettings();
}

public class SettingsManager : MonoBehaviour
{
    private string savePath;
    private GameSettings currentSettings;

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
        if (currentSettings == null)
        {
            currentSettings = new GameSettings();

            SaveSettings();
        }

        Debug.Log("SettingsManager Initialized. Path: " + savePath);
    }

    public void SaveSettings()
    {
        string json = JsonConvert.SerializeObject(currentSettings, Formatting.Indented);
        File.WriteAllText(savePath, json);
        Debug.Log("Settings saved to: " + savePath);
    }

    public GameSettings LoadSettings()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            return JsonConvert.DeserializeObject<GameSettings>(json);
        }
        else
        {
            Debug.Log("No settings file found, using default settings.");
            return null;
        }
    }

    // 通过字符串路径获取设置
    public string GetSetting(string path)
    {
        string[] parts = path.Split('.');
        if (parts.Length == 0)
        {
            Debug.LogError("Invalid setting path: " + path);
            return null;
        }
        
        object currentObject = currentSettings;
        foreach (string part in parts)
        {
            
            var property = currentObject.GetType().GetProperty(part);
            if (property == null)
            {
                var type = currentObject.GetType();
                Debug.LogWarning($"type: {type}");
                // 获取所有公共和非公共的实例属性
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo prope in properties)
                {
                    Debug.LogWarning($"property: {prope.Name}");
                }
                Debug.LogError($"Setting '{part}' not found in path: {path}");  
                return null;
            }
            currentObject = property.GetValue(currentObject);
        }

        return currentObject?.ToString();
    }

    // 通过字符串路径更新设置
    public void UpdateSetting(string path, string newValue)
    {
        string[] parts = path.Split('.');
        if (parts.Length == 0)
        {
            Debug.LogError("Invalid setting path: " + path);
            return;
        }

        object currentObject = currentSettings;
        for (int i = 0; i < parts.Length - 1; i++)
        {
            var property = currentObject.GetType().GetProperty(parts[i]);
            if (property == null)
            {
                Debug.LogError($"Setting '{parts[i]}' not found in path: {path}");
                return;
            }
            currentObject = property.GetValue(currentObject);
        }

        var finalProperty = currentObject.GetType().GetProperty(parts[parts.Length - 1]);
        if (finalProperty == null)
        {
            Debug.LogError($"Setting '{parts[parts.Length - 1]}' not found in path: {path}");
            return;
        }

        // 根据属性类型设置值
        if (finalProperty.PropertyType == typeof(string))
        {
            finalProperty.SetValue(currentObject, newValue);
        }
        else if (finalProperty.PropertyType == typeof(int))
        {
            if (int.TryParse(newValue, out int intValue))
            {
                finalProperty.SetValue(currentObject, intValue);
            }
            else
            {
                Debug.LogError($"Failed to parse '{newValue}' as int for setting: {path}");
            }
        }
        else if (finalProperty.PropertyType == typeof(bool))
        {
            if (bool.TryParse(newValue, out bool boolValue))
            {
                finalProperty.SetValue(currentObject, boolValue);
            }
            else
            {
                Debug.LogError($"Failed to parse '{newValue}' as bool for setting: {path}");
            }
        }
        else
        {
            Debug.LogError($"Unsupported property type '{finalProperty.PropertyType}' for setting: {path}");
        }

        // 不在这里调用 SaveSettings，让调用者手动决定何时保存到硬盘
        // SaveSettings();
    }

}