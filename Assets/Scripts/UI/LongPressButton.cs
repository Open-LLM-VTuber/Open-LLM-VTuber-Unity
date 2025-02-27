using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LongPressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IScrollHandler
{
    private bool isPressed = false;
    private float pressTime = 0f;
    [SerializeField] private float longPressDuration = 1f;
    [SerializeField] private float maxClickDistance = 10f; // 最大允许移动距离（像素）

    private Vector2 initialPressPosition; // 按下时的初始位置
    private ScrollRect scrollRect; // 父级的 ScrollRect 组件

    public UnityEvent onLongPress; // 长按事件
    public UnityEvent onShortPress; // 短按事件

    void Start()
    {
        // 自动查找父级中的 ScrollRect
        scrollRect = GetComponentInParent<ScrollRect>();
    }

    void Update()
    {
        if (isPressed)
        {
            pressTime += Time.deltaTime;
            if (pressTime >= longPressDuration)
            {
                onLongPress?.Invoke();
                isPressed = false;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        pressTime = 0f;
        initialPressPosition = eventData.position; // 记录按下时的位置
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isPressed)
        {
            isPressed = false;
            float distanceMoved = Vector2.Distance(eventData.position, initialPressPosition);

            if (pressTime < longPressDuration && distanceMoved <= maxClickDistance)
            {
                onShortPress?.Invoke(); // 短按事件
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (scrollRect != null)
        {
            // 将拖拽事件传递给 ScrollRect
            scrollRect.OnBeginDrag(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        float distanceMoved = Vector2.Distance(eventData.position, initialPressPosition);
        if (distanceMoved > maxClickDistance)
        {
            isPressed = false; // 移动距离过大，取消长按检测
        }

        if (scrollRect != null)
        {
            // 将拖拽事件传递给 ScrollRect
            scrollRect.OnDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (scrollRect != null)
        {
            // 将拖拽事件传递给 ScrollRect
            scrollRect.OnEndDrag(eventData);
        }
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (scrollRect != null)
        {
            // 将拖拽事件传递给 ScrollRect
            scrollRect.OnScroll(eventData);
        }
    }

}