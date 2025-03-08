
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Live2D 
{
    public class DragController : MonoBehaviour, 
        IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        private Vector3 offset;
        private HitRaycaster hitRaycaster;
        private bool canDrag = false;  // 新增：只有raycast命中后才能拖拽

        void Start()
        {
            // 订阅RaycastHit的事件
            hitRaycaster = GetComponent<HitRaycaster>();
            if (hitRaycaster != null)
            {
                hitRaycaster.OnRaycastStateChanged += SetCanDrag;
            }
            CreateAuxImageChild();
        }

        void OnDestroy()
        {
            // 取消订阅
            if (hitRaycaster != null)
            {
                hitRaycaster.OnRaycastStateChanged -= SetCanDrag;
            }
        }

        void CreateAuxImageChild()
        {
            GameObject imageChild = new GameObject("AuxImage");
            imageChild.transform.SetParent(this.transform, false);
            Image imageComponent = imageChild.AddComponent<Image>();

            // 设置 Image 的属性
            imageComponent.color = new Color(1, 1, 1, 0); 
            imageComponent.raycastTarget = true;
            RectTransform rectTransform = imageChild.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0.5f, 1);
            rectTransform.anchoredPosition = Vector2.zero;
            Debug.Log("Created Aux Image Child For Live2D Model (Drag)");
        }

        // 更新拖拽权限
        private void SetCanDrag(bool state)
        {
            canDrag = state;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            offset = transform.position - GetWorldPosition(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            return;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (canDrag)  // 只有在canDrag为true时才允许拖拽
            {
                transform.position = GetWorldPosition(eventData) + offset;
            }
        }

        private Vector3 GetWorldPosition(PointerEventData eventData)
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(
                new Vector3(eventData.position.x, eventData.position.y, 
                Camera.main.WorldToScreenPoint(transform.position).z));
            return worldPosition;
        }
    }
}