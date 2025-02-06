using System.Collections.Generic;
// Models/AudioMessage.cs
[System.Serializable]
public class AudioMessage : WebSocketMessage
{
    public string audio; // base64 编码的音频数据
    public float[] volumes; // 音量数据
    public int slice_length; // 音频切片长度（毫秒）
    public string text; // 显示的文本
    public Dictionary<string, List<string>> actions; // 动作配置
}
