using DG.Tweening;
using UnityEngine;

public class FadeAnimation : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.3f;
    public bool initActive = true;

    private Tween fadeTween = null; // 保存当前的 Tween 对象，用于检查和中断

    private bool fadingIn = false;
    private bool fadingOut = false;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Start()
    {
        // 首次加载要先激活，然后再通过 deactive 隐藏起来，省资源，再显示也不容易出错
        gameObject.SetActive(initActive);
    }

    // 淡入效果
    public void FadeIn()
    {
        
        gameObject.SetActive(true);

        // 如果当前有动画在播放（比如 FadeOut），立即停止
        if (fadingOut && fadeTween != null && fadeTween.IsPlaying())
        {
            canvasGroup.alpha = 1f;
            Kill();
        }
        else
        {
            fadingIn = true;
            canvasGroup.alpha = 0f;
            fadeTween = canvasGroup.DOFade(1f, fadeInDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => {
                    fadingIn = false;
                });
            
        }
    }

    // 淡出效果
    public void FadeOut()
    {
        fadingOut = true;
        fadeTween = canvasGroup.DOFade(0f, fadeOutDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                fadeTween = null;
                fadingOut = false;
            });
    }

    void OnDestroy()
    {
        if (fadeTween != null)
        {
            Kill();
        }
    }

    void Kill() {
        fadeTween.Kill();
        fadeTween = null;
        fadingIn = fadingOut = false;
    }
}