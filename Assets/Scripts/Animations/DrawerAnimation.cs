using DG.Tweening;
using UnityEngine;

public class DrawerAnimation : MonoBehaviour
{
    public RectTransform drawer; // 需要移动的UI对象
    public float slideDistance = 200f; // 滑动距离
    public float duration = 0.5f; // 动画时长
    public Ease easeType = Ease.OutQuad; // 缓动类型

    private Vector2 _originalPos; // 初始位置
    private Vector2 _targetPos; // 目标位置
    private bool _isOpen; // 当前状态

    void Start()
    {
        // 记录初始位置
        _originalPos = drawer.anchoredPosition;
        // 计算目标位置
        _targetPos = _originalPos + Vector2.down * slideDistance;
    }

    public void ToggleDrawer()
    {
        // 根据当前状态选择目标位置
        Vector2 targetPos = _isOpen ? _originalPos : _targetPos;

        // 执行动画
        drawer.DOAnchorPos(targetPos, duration)
            .SetEase(easeType)
            .OnComplete(() => _isOpen = !_isOpen); // 切换状态
    }

    public bool IsOpen()
    {
        return _isOpen;
    }
}