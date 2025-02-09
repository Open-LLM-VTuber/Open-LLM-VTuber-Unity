using System;
using System.Collections.Generic;

[Serializable]
public class ModelConfigMessage : WebSocketMessage
{
    public ModelInfo model_info;
    public string conf_name;
    public string conf_uid;
}

[Serializable]
public class ModelInfo
{
    public string name;
    public string description;
    public string url;
    public float kScale;
    public int initialXshift;
    public int initialYshift;
    public int kXOffset;
    public string idleMotionGroupName;
    public Dictionary<string, int> emotionMap;
    public Dictionary<string, Dictionary<string, int>> tapMotions;
}

[Serializable]
public class ConfigFilesMessage : WebSocketMessage
{
    public ConfigFile[] configs;
}

[Serializable]
public class ConfigFile
{
    public string filename;
    public string name;
}

[Serializable]
public class BackgroundFilesMessage : WebSocketMessage
{
    public string[] files;
}