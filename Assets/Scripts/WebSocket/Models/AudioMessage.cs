using System.Collections.Generic;
using System;
// Models/AudioMessage.cs
[Serializable]
public class AudioMessage : WebSocketMessage
{
    public string audio; // base64 编码的音频数据
    public float[] volumes; // 音量数据
    public int slice_length; // 音频切片长度（毫秒）
    public DisplayText display_text; // 显示的文本
    public Dictionary<string, List<string>> actions; // 动作配置
    public bool forwarded; // 转发
}

[Serializable]
public class DisplayText
{
    public string text;
    public string name;
    public string avatar;
}
