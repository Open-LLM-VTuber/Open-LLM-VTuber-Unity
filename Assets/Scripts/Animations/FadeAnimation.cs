using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FadeAnimation : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.3f;
    public bool initActive = true;

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
        // 首次加载要先激活，然后再通过deactive隐藏起来，省资源，再显示也不容易出错
        gameObject.SetActive(initActive);
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
