using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;


public class ButtonPressEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float pressScale = 0.9f; // 按钮按下时的缩放比例
    public float pressDuration = 0.1f; // 按下动画持续时间
    public float releaseDuration = 0.1f; // 松开动画持续时间
    public float maxDisplacement = 20f; // 最大位移值

    [Header("销毁设置")]
    public bool destroyOnPress = false; // 是否在按下后销毁对象
    public GameObject targetToDestroy; // 要销毁的目标对象

    [Header("生成设置")]
    public bool spawnOnPress = false; // 是否在按下后生成一个对象
    public GameObject targetToSpawn; // 预制体
    public Transform parentObject; // 父对象
    public Vector3 positionOffset; // 位置偏移

    [Header("淡入淡出设置")]
    public float fadeInDuration = 0.3f; // 淡入持续时间
    public float fadeOutDuration = 0.3f; // 淡出持续时间

    public Action<GameObject> OnSpawnComplete; // 实例化完成回调

    private Vector3 originalScale; // 按钮的原始缩放
    private Vector3 originalLocalPosition; // 按钮的原始本地位置
    private bool isPressed = false; // 是否正在按下
    private Sequence pressSequence; // 用于存储按下动画序列

    private void Start()
    {
        // 记录按钮的原始缩放和本地位置
        originalScale = transform.localScale;
        originalLocalPosition = transform.localPosition;
    }

    // 当按钮被按下时调用
    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;

        // 停止之前的动画
        if (pressSequence != null && pressSequence.IsActive())
        {
            pressSequence.Kill();
        }

        // 创建按下动画序列
        pressSequence = DOTween.Sequence();

        // 同时缩小和位移
        Vector3 targetLocalPosition = originalLocalPosition + Vector3.down * maxDisplacement;
        pressSequence.Append(transform.DOScale(originalScale * pressScale, pressDuration).SetEase(Ease.OutQuad));
        pressSequence.Join(transform.DOLocalMove(targetLocalPosition, pressDuration).SetEase(Ease.OutQuad));
        // 播放动画
        pressSequence.Play();
    }

    // 当按钮被释放时调用
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;

        // 停止之前的动画
        if (pressSequence != null && pressSequence.IsActive())
        {
            pressSequence.Kill();
        }

        // 创建弹跳恢复效果
        Sequence bounceSequence = DOTween.Sequence();
        bounceSequence.Append(transform.DOLocalMove(originalLocalPosition, releaseDuration).SetEase(Ease.OutBack)); // 恢复到原始本地位置
        bounceSequence.Join(transform.DOScale(originalScale, releaseDuration).SetEase(Ease.OutBack)); // 恢复到原始大小

        bounceSequence.OnComplete(() => {
            if (spawnOnPress)
            {
                SpawnTarget();
            }

            if (destroyOnPress)
            {
                DestroyTarget();
            }
        });
    
        // 播放动画
        bounceSequence.Play();
    }

    private void SpawnTarget()
    {
        if (targetToSpawn == null || parentObject == null)
        {
            Debug.LogError("Prefab or Parent Object is not assigned!");
            return;
        }

        // 销毁自身+生成新窗口，需要将parentObject设为挂载此脚本的parentObject
        if (destroyOnPress && spawnOnPress && targetToDestroy != null)
        {
            Debug.Log("销毁自身+生成新窗口，需要将parentObject设为挂载此脚本的parentObject");
            parentObject = parentObject.parent != null ? parentObject.parent.gameObject.GetComponent<Transform>() : null;
            Debug.Log("parentObject: " + parentObject);
        }

        Debug.Log("SpawnTarget");
        // 实例化预制体
        GameObject instance = Instantiate(targetToSpawn, parentObject);
        // 设置位置
        instance.transform.localPosition = positionOffset;
        // 实例化后可回调，设置基本信息
        OnSpawnComplete?.Invoke(instance);
        // 添加淡入效果
        FadeIn(instance);

    }

    // 淡入效果
    private void FadeIn(GameObject target)
    {
        // 确保目标对象有CanvasGroup组件
        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = target.AddComponent<CanvasGroup>();
        }

        // 设置初始透明度为0
        canvasGroup.alpha = 0f;

        // 使用DOTween进行淡入, 播放完动画后触发回调
        canvasGroup.DOFade(1f, fadeInDuration).SetEase(Ease.InQuad);
    }

    // 淡出效果
    private void FadeOut(GameObject target)
    {
        // 确保目标对象有CanvasGroup组件
        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = target.AddComponent<CanvasGroup>();
        }

        // 使用DOTween进行淡出
        canvasGroup.DOFade(0f, fadeOutDuration).SetEase(Ease.OutQuad).OnComplete(() => Destroy(target));
    }

    // 销毁目标对象
    private void DestroyTarget()
    {
        if (targetToDestroy == null)
        {
            Debug.LogError("Target is not assigned!");
            return;
        }
        Debug.Log("DestroyTarget");

        // 添加淡出效果
        FadeOut(targetToDestroy);
    }
}