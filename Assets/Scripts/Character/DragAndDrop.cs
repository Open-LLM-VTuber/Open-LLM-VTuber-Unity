
using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDrop : MonoBehaviour, 
    IPointerDownHandler, IDragHandler
{

    private Vector3 offset;
    private bool isDragging = false;

    void Start()
    {
        //if (GetComponent<Collider>() == null)
        //{
        //    gameObject.AddComponent<BoxCollider>();
        //}
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        offset = transform.position - GetWorldPosition(eventData);
        isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            transform.position = GetWorldPosition(eventData) + offset;
        }
    }

    private Vector3 GetWorldPosition(PointerEventData eventData)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, Camera.main.WorldToScreenPoint(transform.position).z));
        return worldPosition;
    }
}
