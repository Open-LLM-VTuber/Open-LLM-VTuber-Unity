using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AudioMessageHandler : MonoBehaviour
{
    [SerializeField] private TMP_Text _displayText;

    private Queue<AudioMessage> audioQueue = new Queue<AudioMessage>();
    private bool isPlaying;

    public void Initialize(WebSocketManager wsManager, TMP_Text displayText)
    {
        wsManager.RegisterHandler("audio", HandleAudioMessage);
        _displayText = displayText;
    }

    private void HandleAudioMessage(WebSocketMessage message)
    {
        var audioMsg = message as AudioMessage;
        audioQueue.Enqueue(audioMsg);
        TryPlayNext();
    }

    private void TryPlayNext()
    {
        if (!isPlaying && audioQueue.Count > 0)
        {
            isPlaying = true;
            var msg = audioQueue.Dequeue();

            if (_displayText != null)
            {
                _displayText.text = $"\nAI: {msg.text}";
            }
            // 创建时不要立刻播放音频
            int voiceEntity = AudioManager.Instance.CreateAudioEntityFromBase64(msg.audio, playOnCreate: false);
            // 用管理器播放音频，并在播放完成后处理下一条消息
            AudioManager.Instance.PlayAudio(voiceEntity, () =>
            {
                isPlaying = false;
                AudioManager.Instance.RemoveAudio(voiceEntity);
                TryPlayNext();
            });
        }
    }
}