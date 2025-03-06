
using UnityEngine;
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework.Raycasting;
using System;

namespace Live2D
{
    public class RaycastHit : MonoBehaviour
    {
        private CubismRaycaster Raycaster { get; set; }
        private CubismRaycastHit[] Results { get; set; }
        public event Action OnRaycastHit;  // 保持原有事件
        public event Action<bool> OnRaycastStateChanged;  // 新增事件用于传递命中状态
        
        private bool isHit = false;  // 追踪是否命中

        private void Start()
        {
            Raycaster = GetComponent<CubismRaycaster>();
            Results = new CubismRaycastHit[4];
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }
            DoRaycast();
        }

        private void DoRaycast()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var hitCount = Raycaster.Raycast(ray, Results);

            // 更新命中状态并通知
            bool newHitState = hitCount > 0;
            if (newHitState != isHit)
            {
                isHit = newHitState;
                OnRaycastStateChanged?.Invoke(isHit);
            }

            if (hitCount == 0)
            {
                return;
            }
            
            OnRaycastHit?.Invoke();
        }
    }
}