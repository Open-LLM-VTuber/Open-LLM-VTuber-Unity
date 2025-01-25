/**
 * Copyright(c) Live2D Inc. All rights reserved.
 *
 * Use of this source code is governed by the Live2D Open Software license
 * that can be found at https://www.live2d.com/eula/live2d-open-software-license-agreement_en.html.
 */


using Live2D.Cubism.Framework.LookAt;
using UnityEngine;

namespace Live2D.Cubism.Samples.OriginalWorkflow.Demo
{
    public class CubismLookTarget : MonoBehaviour, ICubismLookTarget
    {

        /// <summary>
        /// Local center position.
        /// </summary>
        public Transform Center;

        /// <summary>
        /// Get mouse coordinates while dragging.
        /// </summary>
        /// <returns>Mouse coordinates.</returns>
        public Vector3 GetPosition()
        {
            if (!Input.GetMouseButton(0))
            {
                return new Vector3(0,0,0);
            }

            var mousePosition = Input.mousePosition;
            var viewportCenterPosition = Camera.main.ScreenToViewportPoint(Center.position);
            var viewportMousePosition = Camera.main.ScreenToViewportPoint(mousePosition);
            var viewportTargetPosition = viewportMousePosition - (viewportCenterPosition + new Vector3(0.5f, 0.5f, 0));

            return viewportTargetPosition;
        }

        /// <summary>
        /// Gets whether the target is active.
        /// </summary>
        /// <returns><see langword="true"/> if the target is active; <see langword="false"/> otherwise.</returns>
        public bool IsActive()
        {
            return true;
        }
    }
}
