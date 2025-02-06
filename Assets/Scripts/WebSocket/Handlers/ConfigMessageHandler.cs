
using UnityEngine;

// Handlers/ConfigMessageHandler.cs
public class ConfigMessageHandler : MonoBehaviour
{
    public void Initialize(WebSocketManager wsManager)
    {
        wsManager.RegisterHandler("set-model-and-conf", HandleModelConfig);
        wsManager.RegisterHandler("config-files", HandleConfigFiles);
        wsManager.RegisterHandler("background-files", HandleBackgroundFiles);
    }

    private void HandleModelConfig(WebSocketMessage message)
    {
        var configMsg = message as ModelConfigMessage;
        Debug.Log($"Model config updated: {configMsg.model_info}, {configMsg.conf_name}");
        //ConfigManager.Instance.UpdateModelConfig(configMsg);
    }

    private void HandleConfigFiles(WebSocketMessage message)
    {
        var configFiles = message as ConfigFilesMessage;
        Debug.Log($"Available configs: {string.Join(", ", configFiles.configs)}");
        //ConfigManager.Instance.UpdateAvailableConfigs(configFiles.configs);
    }

    private void HandleBackgroundFiles(WebSocketMessage message)
    {
        var bgFiles = message as BackgroundFilesMessage;
        Debug.Log($"Background files: {string.Join(", ", bgFiles.files)}");
        //ConfigManager.Instance.UpdateBackgroundFiles(bgFiles.files);
    }
}