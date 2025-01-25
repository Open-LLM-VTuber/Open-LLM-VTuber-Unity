using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AudioCapture : MonoBehaviour
{
    private AudioClip microphoneInput;
    private bool isRecording = false;
    private string selectedDevice;

    // 采样率和块大小
    private int sampleRate = 16000; // 16000Hz
    private float blockSize = 0.1f; // 0.1秒
    private int samplesPerBlock; // 每个块的采样点数

    // UI 按钮和图片
    public Button recordButton;
    public Sprite startRecordingSprite; // 开始录制图标
    public Sprite stopRecordingSprite;  // 停止录制图标

    void Start()
    {
        // 计算每个块的采样点数
        samplesPerBlock = (int)(sampleRate * blockSize);

        // 初始化按钮图标
        if (recordButton != null && startRecordingSprite != null)
        {
            recordButton.image.sprite = startRecordingSprite;
        }
        else
        {
            Debug.LogError("UI elements not assigned!");
        }
    }

    public void ToggleRecording()
    {
        if (isRecording)
        {
            // 停止录制
            StopRecording();
            recordButton.image.sprite = startRecordingSprite; // 切换为开始图标
        }
        else
        {
            // 开始录制
            StartRecording();
            recordButton.image.sprite = stopRecordingSprite; // 切换为停止图标
        }
    }

    void StartRecording()
    {
        if (Microphone.devices.Length > 0)
        {
            selectedDevice = Microphone.devices[0];
            microphoneInput = Microphone.Start(selectedDevice, true, 1, sampleRate); // 1秒缓冲区
            isRecording = true;
            Debug.Log("Recording started with device: " + selectedDevice);

            // 启动协程处理音频块
            StartCoroutine(ProcessAudioBlocks());
        }
        else
        {
            Debug.LogError("No microphone found!");
        }
    }

    void StopRecording()
    {
        if (isRecording)
        {
            Microphone.End(selectedDevice);
            isRecording = false;
            Debug.Log("Recording stopped.");
        }
    }

    IEnumerator ProcessAudioBlocks()
    {
        while (isRecording)
        {
            // 等待 0.1 秒
            yield return new WaitForSeconds(blockSize);

            // 获取当前麦克风的位置
            int micPosition = Microphone.GetPosition(selectedDevice);

            // 如果缓冲区中有足够的数据（至少一个块）
            if (micPosition >= samplesPerBlock)
            {
                // 提取一个块的音频数据
                float[] audioData = new float[samplesPerBlock];
                microphoneInput.GetData(audioData, micPosition - samplesPerBlock);

                // 计算RMS音量
                float volume = CalculateRMSVolume(audioData);
                Debug.Log("Audio block RMS volume: " + volume);
            }
        }
    }

    float CalculateRMSVolume(float[] audioData)
    {
        // 计算RMS
        float sumOfSquares = 0f;
        foreach (float sample in audioData)
        {
            sumOfSquares += sample * sample; // 平方和
        }
        float meanOfSquares = sumOfSquares / audioData.Length; // 平均值
        float rms = Mathf.Sqrt(meanOfSquares); // 平方根

        return rms;
    }

    void OnDestroy()
    {
        // 停止录制
        if (isRecording)
        {
            StopRecording();
        }
    }
}