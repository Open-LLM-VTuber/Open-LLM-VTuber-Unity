
using UnityEngine;

// Handlers/ConfigMessageHandler.cs
public class ConfigMessageHandler : InitOnceSingleton<ConfigMessageHandler>
{
    public void Initialize(WebSocketManager wsManager)
    {
        InitOnce(()=>
        {
            wsManager.RegisterHandler("set-model-and-conf", HandleModelConfig);
            wsManager.RegisterHandler("config-files", HandleConfigFiles);
            wsManager.RegisterHandler("background-files", HandleBackgroundFiles);
            RequestInitialData();
        });
        
    }

    private void RequestInitialData()
    {
        var wsManager = WebSocketManager.Instance;
        wsManager.Send(new WebSocketMessage { type = "fetch-configs" });
        wsManager.Send(new WebSocketMessage { type = "fetch-backgrounds" });
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
        Debug.Log($"Available configs: {configFiles.configs}");
        //ConfigManager.Instance.UpdateAvailableConfigs(configFiles.configs);
    }

    private void HandleBackgroundFiles(WebSocketMessage message)
    {
        var bgFiles = message as BackgroundFilesMessage;
        Debug.Log($"Background files: {string.Join(", ", bgFiles.files)}");
        //ConfigManager.Instance.UpdateBackgroundFiles(bgFiles.files);
    }
}