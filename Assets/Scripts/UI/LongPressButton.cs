using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class LongPressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool isPressed = false;
    private float pressTime = 0f;
    [SerializeField] private float longPressDuration = 1f;

    public UnityEvent onLongPress; // 长按事件
    public UnityEvent onShortPress; // 短按事件

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
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isPressed)
        {
            isPressed = false;
            if (pressTime < longPressDuration)
            {
                onShortPress?.Invoke();
            }
        }
    }

    public void Log()
    {
        Debug.Log("Long Press!");
    }
}