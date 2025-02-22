using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleFromPointAnimation : MonoBehaviour
{

    public Transform targetObject; // 需要动画的目标物体
    public Transform centerPoint; // 中心点位置
    public float duration = 0.2f; // 动画时长
    public Ease easeType = Ease.OutQuad; // 缓动类型

    private Vector3 _originalPos; // 初始位置
    private Vector3 _originalScale; // 初始缩放
    private bool _isExpanded; // 当前状态（是否展开）

    void Awake()
    {
        // 确保目标物体已赋值
        if (targetObject == null)
        {
            Debug.LogError("目标物体未赋值！");
            return;
        }

        // 记录初始位置和缩放
        _originalPos = targetObject.position;
        _originalScale = targetObject.localScale;
        targetObject.position = centerPoint.position;
        targetObject.localScale = Vector3.zero;
        _isExpanded = false;
    }

    public void ToggleProjection()
    {
        // 如果当前是展开状态，则目标是中心点和零缩放；否则目标是原始位置和缩放
        Vector3 targetPos = _isExpanded ? centerPoint.position : _originalPos;
        Vector3 targetScale = _isExpanded ? Vector3.zero : _originalScale;

        // 创建动画序列
        Sequence seq = DOTween.Sequence();
        seq.Append(targetObject.DOMove(targetPos, duration).SetEase(easeType));
        seq.Join(targetObject.DOScale(targetScale, duration).SetEase(easeType));
        seq.OnComplete(() => _isExpanded = !_isExpanded); // 切换状态
    }

    public bool IsExpanded()
    {
        return _isExpanded;
    }
}
