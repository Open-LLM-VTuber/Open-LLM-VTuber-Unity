using DG.Tweening;
using UnityEngine;

public class RotateAnimation : MonoBehaviour
{
    public Transform rotatingObject; // 需要旋转的物件
    public float rotationAngle = 90f; // 旋转角度
    public float duration = 0.5f; // 动画时长
    public Ease easeType = Ease.OutQuad; // 缓动类型

    private bool _isRotated; // 当前状态

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
}