using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class FadeInOutAnimation : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Ease easeType = Ease.OutQuad; // 缓动类型
    private bool isFaded = false;

    void Awake()
    {
        // 初始化时设置 CanvasGroup 的 alpha 值
        canvasGroup.alpha = isFaded ? 0 : 1;
    }

    public void ToggleFade()
    {
        // 根据当前状态决定是淡入还是淡出
        if (isFaded)
        {
            // 淡入
            canvasGroup.DOFade(1, 0.5f).SetEase(easeType).OnComplete(() => isFaded = false);
        }
        else
        {
            // 淡出
            canvasGroup.DOFade(0, 0.5f).SetEase(easeType).OnComplete(() => isFaded = true);
        }
    }
}