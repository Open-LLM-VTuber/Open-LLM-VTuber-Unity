using DG.Tweening;
using UnityEngine;

public class RotateAnimation : MonoBehaviour
{
    public Transform rotatingObject; // 需要旋转的物件
    public float rotationAngle = 90f; // 旋转角度
    public float duration = 0.5f; // 动画时长
    public Ease easeType = Ease.OutQuad; // 缓动类型

    private bool _isRotated; // 当前状态
    private Tween _infiniteRotationTween; // 存储无限旋转的 Tween

    public void ToggleRotation()
    {
        // 根据当前状态选择旋转角度
        float targetRotation = _isRotated ? -rotationAngle : rotationAngle;

        // 执行旋转动画
        rotatingObject.DORotate(new Vector3(0, 0, targetRotation), duration, RotateMode.LocalAxisAdd)
            .SetEase(easeType)
            .OnComplete(() => _isRotated = !_isRotated); // 切换状态
    }

    public bool IsRotated()
    {
        return _isRotated;
    }

    public void StartInfiniteRotation()
    {
        StopInfiniteRotation();
        _infiniteRotationTween = rotatingObject.DORotate(new Vector3(0, 0, 360), duration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1);
    }

    public void StopInfiniteRotation()
    {
        if (_infiniteRotationTween != null && _infiniteRotationTween.IsActive())
        {
            _infiniteRotationTween.Kill();
            _infiniteRotationTween = null;
        }
    }

    private void OnDestroy()
    {
        if (_infiniteRotationTween != null)
        {
            _infiniteRotationTween.Kill();
        }
    }
}