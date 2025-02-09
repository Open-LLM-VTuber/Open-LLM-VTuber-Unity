using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatContent : MonoBehaviour
{
    [SerializeField] private TMP_Text contentText; // UI文本组件
    [SerializeField] private GameObject contentGameObject; // 需要调整的气泡Object
    [SerializeField] private float maxWidth = 740f; // 指定的最大宽度

    private ContentSizeFitter contentSizeFitter;

    private void Awake()
    {
        if (contentGameObject != null)
        {
            contentSizeFitter = contentGameObject.GetComponent<ContentSizeFitter>();
        }
    }

    public void SetContent(string content)
    {
        if (contentText != null)
        {
            contentText.text = content;

            float textWidth = contentText.preferredWidth;

            if (contentSizeFitter != null)
            {
                if (textWidth <= maxWidth)
                {
                    contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                }
                else
                {
                    contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                }
            }
        }
    }
}