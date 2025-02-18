using UnityEngine;
using DG.Tweening; // 引入DOTween命名空间
using UnityEngine.UI; // 用于按钮交互

public class CameraMoveAnimation : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private Transform targetPosition; // 目标位置
    [SerializeField] private float moveDuration = 1.5f; // 移动持续时间
    [SerializeField] private Ease easeType = Ease.InOutQuad; // 缓动类型

    private Vector3 originalPosition; // 记录原始位置
    private Quaternion originalRotation; // 记录原始旋转

    /// <summary>
    /// 将摄像机移动到目标位置
    /// </summary>
    public void MoveCameraToTarget()
    {
        originalPosition = Camera.main.transform.position;
        originalRotation = Camera.main.transform.rotation;

        // 确保有目标位置
        if (targetPosition == null)
        {
            Debug.LogError("目标位置未设置！");
            return;
        }

        // 使用DOTween进行移动和旋转
        Camera.main.transform.DOMove(
            new Vector3(
                targetPosition.position.x, 
                targetPosition.position.y, 
                originalPosition.z
            ), moveDuration)
            .SetEase(easeType)
            .OnStart(() => Debug.Log("开始移动相机"))
            .OnComplete(() => Debug.Log("移动完成"));

        Camera.main.transform.DORotate(
            new Vector3(
                targetPosition.eulerAngles.x,
                targetPosition.eulerAngles.y,
                originalRotation.eulerAngles.z
            ), moveDuration)
            .SetEase(easeType);
    }

    /// <summary>
    /// 重置摄像机到初始位置
    /// </summary>
    public void ResetCameraPosition()
    {
        Camera.main.transform.DOMove(originalPosition, moveDuration)
            .SetEase(easeType);

        Camera.main.transform.DORotate(originalRotation.eulerAngles, moveDuration)
            .SetEase(easeType);
    }
}