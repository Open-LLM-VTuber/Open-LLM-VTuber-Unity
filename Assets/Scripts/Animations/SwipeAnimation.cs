using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class SwipeAnimation : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private RectTransform rectTransform; // 按钮的 RectTransform
    private Vector2 startPos;           // 触摸初始位置
    private Vector2 buttonStartPos;     // 按钮初始位置
    private bool isSwipingToMax;        // 标记是否正在滑向最大距离

    [Header("滑动设置")]
    [Tooltip("选择允许的滑动方向")]
    public SwipeDirection allowedDirection = SwipeDirection.Left; // 默认向左滑动
    public float maxSlideDistance = 200f;    // 最大滑动距离（单位：像素）
    public float moveDuration = 0.3f;        // 平滑移动动画时长（秒）
    public float minSwipeDistance = 20f;     // 最小滑动距离，判断有效滑动

    // 滑动方向枚举
    public enum SwipeDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        buttonStartPos = rectTransform.anchoredPosition; // 记录初始位置
        isSwipingToMax = false; // 初始化状态
    }

    // 按下时触发
    public void OnPointerDown(PointerEventData eventData)
    {
        startPos = eventData.position; // 记录触摸初始位置
        rectTransform.DOKill();        // 停止之前的 DOTween 动画
    }

    // 拖动时触发
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 currentPos = eventData.position;
        Vector2 delta = currentPos - startPos;

        // 判断是否达到最小滑动距离
        if (delta.magnitude > minSwipeDistance)
        {
            Vector2 targetPos = rectTransform.anchoredPosition;

            switch (allowedDirection)
            {
                case SwipeDirection.Left:
                    if (delta.x < 0 && !isSwipingToMax) // 向左滑动
                    {
                        targetPos.x = buttonStartPos.x - maxSlideDistance;
                        rectTransform.DOAnchorPos(targetPos, moveDuration).SetEase(Ease.OutQuad);
                        isSwipingToMax = true;
                    }
                    else if (delta.x > 0 && isSwipingToMax) // 向右划回来
                    {
                        targetPos.x = buttonStartPos.x;
                        rectTransform.DOAnchorPos(targetPos, moveDuration).SetEase(Ease.OutQuad);
                        isSwipingToMax = false;
                    }
                    break;

                case SwipeDirection.Right:
                    if (delta.x > 0 && !isSwipingToMax) // 向右滑动
                    {
                        targetPos.x = buttonStartPos.x + maxSlideDistance;
                        rectTransform.DOAnchorPos(targetPos, moveDuration).SetEase(Ease.OutQuad);
                        isSwipingToMax = true;
                    }
                    else if (delta.x < 0 && isSwipingToMax) // 向左划回来
                    {
                        targetPos.x = buttonStartPos.x;
                        rectTransform.DOAnchorPos(targetPos, moveDuration).SetEase(Ease.OutQuad);
                        isSwipingToMax = false;
                    }
                    break;

                case SwipeDirection.Up:
                    if (delta.y > 0 && !isSwipingToMax) // 向上滑动
                    {
                        targetPos.y = buttonStartPos.y + maxSlideDistance;
                        rectTransform.DOAnchorPos(targetPos, moveDuration).SetEase(Ease.OutQuad);
                        isSwipingToMax = true;
                    }
                    else if (delta.y < 0 && isSwipingToMax) // 向下滑回来
                    {
                        targetPos.y = buttonStartPos.y;
                        rectTransform.DOAnchorPos(targetPos, moveDuration).SetEase(Ease.OutQuad);
                        isSwipingToMax = false;
                    }
                    break;

                case SwipeDirection.Down:
                    if (delta.y < 0 && !isSwipingToMax) // 向下滑动
                    {
                        targetPos.y = buttonStartPos.y - maxSlideDistance;
                        rectTransform.DOAnchorPos(targetPos, moveDuration).SetEase(Ease.OutQuad);
                        isSwipingToMax = true;
                    }
                    else if (delta.y > 0 && isSwipingToMax) // 向上划回来
                    {
                        targetPos.y = buttonStartPos.y;
                        rectTransform.DOAnchorPos(targetPos, moveDuration).SetEase(Ease.OutQuad);
                        isSwipingToMax = false;
                    }
                    break;
            }
        }

        startPos = currentPos; // 更新起始位置
    }

    // 松开时触发
    public void OnPointerUp(PointerEventData eventData)
    {
        // 可选：松开后保持当前位置，或强制回到初始位置
        // rectTransform.DOAnchorPos(buttonStartPos, moveDuration).SetEase(Ease.OutQuad);
        // isSwipingToMax = false;
    }
}