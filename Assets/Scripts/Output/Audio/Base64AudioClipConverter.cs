using UnityEngine;
using System;

public static class Base64AudioClipConverter
{
    public static AudioClip ConvertBase64ToAudioClip(string base64String, int sampleRate = 24000, bool norm = false)
    {
        byte[] audioBytes = Convert.FromBase64String(base64String);
        float[] audioData = ConvertBytesToFloat(audioBytes, norm);
        AudioClip audioClip = AudioClip.Create("Base64Audio", audioData.Length, 1, sampleRate, false);
        audioClip.SetData(audioData, 0);
        return audioClip;
    }

    private static float[] ConvertBytesToFloat(byte[] bytes, bool norm)
    {
        float[] floatArr = new float[bytes.Length / 4];
        for (int i = 0; i < floatArr.Length; i++)
        {
            if (System.BitConverter.IsLittleEndian) Array.Reverse(bytes, i * 4, 4);
            floatArr[i] = System.BitConverter.ToSingle(bytes, i * 4);
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