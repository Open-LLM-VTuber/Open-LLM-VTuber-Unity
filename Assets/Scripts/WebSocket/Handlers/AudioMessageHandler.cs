using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AudioMessageHandler : InitOnceSingleton<AudioMessageHandler>
{
    [SerializeField] private TMP_Text _displayText;

    private GameObject _dialogPanel;
    private Queue<AudioMessage> audioQueue = new Queue<AudioMessage>();
    private bool isPlaying;

    public void Initialize(WebSocketManager wsManager, GameObject dialogPanel)
    {
        InitOnce(() =>
        {
            wsManager.RegisterHandler("audio", HandleAudioMessage);
        });

        if (dialogPanel != null)
        {
            _displayText = dialogPanel.transform.Find("FrostedGlass/Content")?.GetComponent<TMP_Text>();
            _dialogPanel = dialogPanel;
            _dialogPanel.SetActive(false);
        }
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
            if (_dialogPanel != null)
            {
                _dialogPanel.SetActive(true);
            }
            
            isPlaying = true;
            var msg = audioQueue.Dequeue();

            if (_displayText != null)
            {
                _displayText.text = $"\nAI: {msg.display_text.text}";
            }

            // 记下最后的回复，用于interrupt-signal
            var lastMsg = HistoryManager.Instance.assistantLastMessage;
            lastMsg.content += msg.display_text.text;
            lastMsg.avatar = msg.display_text.avatar;
            lastMsg.name = msg.display_text.name;
            HistoryManager.Instance.UpdateHistoryData();

            if (!string.IsNullOrEmpty(msg.audio))
            {
                // 创建时不要立刻播放音频
                int voiceEntity = AudioManager.Instance.CreateAudioEntityFromBase64(msg.audio, playOnCreate: false);
                // 用管理器播放音频，并在播放完成后处理下一条消息
                AudioManager.Instance.PlayAudio(voiceEntity, () =>
                {
                    if (_dialogPanel != null)
                    {
                        _dialogPanel.SetActive(false);
                    }
                    isPlaying = false;
                    AudioManager.Instance.RemoveAudio(voiceEntity);
                    TryPlayNext();
                });
            } 
            else
            {
                // 表情等特殊符号，不读出来，但是也得继续
                isPlaying = false;
                TryPlayNext();
            }
            
        }
    }

    public void ClearAudioQueue()
    {
        audioQueue.Clear();
        isPlaying = false;
    }
}