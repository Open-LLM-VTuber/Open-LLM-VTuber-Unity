using UnityEngine;
using System;

public static class Base64AudioClipConverter
{
    public static AudioClip ConvertBase64ToAudioClip(string base64String, bool norm = false)
    {
        // 将 Base64 字符串转换为字节数组
        byte[] audioBytes = Convert.FromBase64String(base64String);

        // 解析 WAV 文件头部
        int channels = BitConverter.ToInt16(audioBytes, 22); // 声道数
        int fileSampleRate = BitConverter.ToInt32(audioBytes, 24); // 采样率
        int bitsPerSample = BitConverter.ToInt16(audioBytes, 34); // 每个采样的位数

        // Debug.Log($"WAV Header: Channels={channels}, SampleRate={fileSampleRate}, BitsPerSample={bitsPerSample}");

        // 检查是否为 PCM 16-bit 格式
        if (bitsPerSample != 16)
        {
            throw new NotSupportedException("Only 16-bit PCM WAV files are supported.");
        }

        // 找到数据块的起始位置
        int dataChunkIndex = 12;
        while (dataChunkIndex < audioBytes.Length - 8 &&
               !(audioBytes[dataChunkIndex] == 'd' &&
                 audioBytes[dataChunkIndex + 1] == 'a' &&
                 audioBytes[dataChunkIndex + 2] == 't' &&
                 audioBytes[dataChunkIndex + 3] == 'a'))
        {
            dataChunkIndex += 4;
            int chunkSize = BitConverter.ToInt32(audioBytes, dataChunkIndex);
            dataChunkIndex += 4 + chunkSize;
        }

        if (dataChunkIndex >= audioBytes.Length - 8)
        {
            throw new FormatException("Invalid WAV file: 'data' chunk not found.");
        }

        // 数据块的大小
        int dataSize = BitConverter.ToInt32(audioBytes, dataChunkIndex + 4);
        int dataStartIndex = dataChunkIndex + 8;

        // 将 PCM 16-bit 数据转换为 float 数组
        float[] audioData = ConvertPcm16ToFloat(audioBytes, dataStartIndex, dataSize, norm);

        // 创建 AudioClip
        AudioClip audioClip = AudioClip.Create("Base64Audio", audioData.Length / channels, channels, fileSampleRate, false);
        audioClip.SetData(audioData, 0);
        return audioClip;
    }

    private static float[] ConvertPcm16ToFloat(byte[] bytes, int startIndex, int dataSize, bool norm)
    {
        int sampleCount = dataSize / 2; // 每个样本 2 字节
        float[] floatArr = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            // 读取 16-bit PCM 数据（小端）
            short pcm16 = BitConverter.ToInt16(bytes, startIndex + i * 2);
            floatArr[i] = pcm16 / 32768f; // 将 16-bit 数据归一化到 [-1, 1]
        }

        if (norm)
        {
            return Normalize(floatArr);
        }
        return floatArr;
    }

    private static float[] Normalize(float[] data)
    {
        float max = float.MinValue;
        for (int i = 0; i < data.Length; i++)
        {
            if (Math.Abs(data[i]) > max) max = Math.Abs(data[i]);
        }
        for (int i = 0; i < data.Length; i++) data[i] = data[i] / max;
        return data;
    }
}