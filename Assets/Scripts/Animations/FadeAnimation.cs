using DG.Tweening;
using UnityEngine;

public class FadeAnimation : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.3f;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    // 淡入效果
    public void FadeIn()
    {
        gameObject.SetActive(true);
        // 设置初始透明度为0
        canvasGroup.alpha = 0f;

        // 使用DOTween进行淡入, 播放完动画后触发回调
        canvasGroup.DOFade(1f, fadeInDuration).SetEase(Ease.InQuad);
    }

    // 淡出效果
    public void FadeOut()
    {
        // 使用DOTween进行淡出
        canvasGroup.DOFade(0f, fadeOutDuration).SetEase(Ease.OutQuad).OnComplete(() => gameObject.SetActive(false));
    }

}
