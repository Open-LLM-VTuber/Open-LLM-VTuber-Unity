
using TMPro;
using UnityEngine;

public class ChatContent : MonoBehaviour
{
    [SerializeField] private TMP_Text contentText; // UI文本组件

    public void SetContent(string content)
    {
        if (contentText != null)
        {
            contentText.text = content;
        }
    }
}