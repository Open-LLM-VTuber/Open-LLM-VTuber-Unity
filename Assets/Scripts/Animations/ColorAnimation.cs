using DG.Tweening;
using UnityEngine;
using UnityEngine.UI; // 如果需要操作 UI 元素的颜色

public class ColorAnimation : MonoBehaviour
{
    public Graphic targetGraphic; // 需要改变颜色的 UI 元素（如 Image 或 Text）
    public Color targetColor = Color.red; // 目标颜色
    public float duration = 0.5f; // 动画时长
    public Ease easeType = Ease.OutQuad; // 缓动类型

    private Color _originalColor; // 初始颜色
    private bool _isChanged; // 当前状态

    void Awake()
    {
        if (targetGraphic == null)
        {
            Debug.LogError("Target Graphic is not assigned!");
            return;
        }

        // 记录初始颜色
        _originalColor = targetGraphic.color;
    }

    public void ToggleColor()
    {
        if (targetGraphic == null)
        {
            Debug.LogError("Target Graphic is not assigned!");
            return;
        }

        // 根据当前状态选择目标颜色
        Color target = _isChanged ? _originalColor : targetColor;

        // 执行颜色变化动画
        targetGraphic.DOColor(target, duration)
            .SetEase(easeType)
            .OnComplete(() => _isChanged = !_isChanged); // 切换状态
    }

    public bool IsChanged()
    {
        return _isChanged;
    }
}