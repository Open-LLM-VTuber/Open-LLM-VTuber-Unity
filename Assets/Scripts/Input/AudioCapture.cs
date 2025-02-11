using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;

public class AudioCapture : MonoBehaviour
{
    private AudioClip microphoneInput;
    private bool isRecording = false;
    private string selectedDevice;

    // 采样率和块大小
    private int sampleRate = 16000; // 16000Hz
    private float blockSize = 0.032f; // Default sampleRate = 16000, frameSamples = 512, buffer window = 32ms
    private int samplesPerBlock; // 每个块的采样点数

    // UI 按钮
    public Button recordButton;

    // 按钮颜色
    public Color activeColor = new Color(0f, 0.5f, 1f); // 淡蓝色
    public Color inactiveColor = new Color(0f, 0f, 0f); // 黑色

    // WebSocket 管理器
    private WebSocketManager wsManager;

    void Start()
    {
        // 计算每个块的采样点数
        samplesPerBlock = (int)(sampleRate * blockSize);

        // 初始化按钮颜色
        if (recordButton != null)
        {
            recordButton.image.color = inactiveColor;
        }
        else
        {
            Debug.LogError("UI elements not assigned!");
        }

        // 获取 WebSocketManager 实例
        wsManager = WebSocketManager.Instance;
    }

    public void ToggleRecording()
    {
        if (isRecording)
        {
            // 停止录制
            StopRecording();
            recordButton.image.color = inactiveColor; // 切换为非激活颜色
        }
        else
        {
            // 开始录制
            StartCoroutine(StartRecording());
            recordButton.image.color = activeColor; // 切换为激活颜色
        }
    }

    IEnumerator StartRecording()
    {
        if (Microphone.devices.Length > 0)
        {
            selectedDevice = Microphone.devices[0];
            microphoneInput = Microphone.Start(selectedDevice, true, 1, sampleRate); // 1秒缓冲区
            isRecording = true;
            Debug.Log("Recording started with device: " + selectedDevice);
            WebSocketController.Interrupt();

            // 通知服务器开启VAD
            yield return StartCoroutine(StartAudioData());
            // 启动协程处理音频块
            yield return StartCoroutine(ProcessAudioBlocks());
        }
        else
        {
            Debug.LogError("No microphone found!");
        }
    }

    void StopRecording(bool forceStop = false)
    {
        if (isRecording)
        {
            Microphone.End(selectedDevice);
            isRecording = false;
            Debug.Log("Recording stopped.");

            if (!forceStop)
            {
                // 发送音频结束信号
                StartCoroutine(EndAudioData());
            }
        }
    }

    IEnumerator ProcessAudioBlocks()
    {
        while (isRecording)
        {
            // 等待 0.032 秒
            yield return new WaitForSeconds(blockSize);

            // 获取当前麦克风的位置
            int micPosition = Microphone.GetPosition(selectedDevice);

            // 如果缓冲区中有足够的数据（至少一个块）
            if (micPosition >= samplesPerBlock)
            {
                // 提取一个块的音频数据
                float[] audioData = new float[samplesPerBlock];
                microphoneInput.GetData(audioData, micPosition - samplesPerBlock);

                // 发送音频数据到后端
                SendAudioData(audioData);
            }
        }


    }

    IEnumerator StartAudioData()
    {
        wsManager.Send(new AudioDataMessage
        {
            type = "unity-audio-data",
            action = "start",
            audio = new float[1]
        });

        int missCount = 0;
        while (false == TextMessageHandler.Instance.State.AllowUnityAudio)
        {
            if (missCount >= 100)
            {
                Debug.LogWarning("Cannot activate VAD on server side.");
                break;
            }
            yield return new WaitForSeconds(0.1f);
            missCount++;
        }
        Debug.Log("StartAudioData");

    }

    void SendAudioData(float[] audioData)
    {
        // 发送音频数据到后端
        wsManager.Send(new AudioDataMessage
        {
            type = "unity-audio-data",
            action = "data",
            audio = audioData
        });
    }

    IEnumerator EndAudioData()
    {
        wsManager.Send(new AudioDataMessage
        {
            type = "unity-audio-data",
            action = "end",
            audio = new float[1]
        });
        int missCount = 0;
        while (true == TextMessageHandler.Instance.State.AllowUnityAudio)
        {
            if (missCount >= 100)
            {
                Debug.LogWarning("Cannot deactivate VAD on server side.");
                break;
            }
            yield return new WaitForSeconds(0.1f);
            missCount++;
        }
        Debug.Log("EndAudioData");
    }


    void OnDestroy()
    {
        // 停止录制
        if (isRecording)
        {
            StopRecording(forceStop: true);
        }
    }
}

// 定义音频数据消息类
public class AudioDataMessage : WebSocketMessage
{
    public float[] audio;
    public string action;
}
