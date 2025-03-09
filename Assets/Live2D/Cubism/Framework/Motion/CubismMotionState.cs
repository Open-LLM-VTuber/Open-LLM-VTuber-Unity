/**
 * Copyright(c) Live2D Inc. All rights reserved.
 *
 * Use of this source code is governed by the Live2D Open Software license
 * that can be found at https://www.live2d.com/eula/live2d-open-software-license-agreement_en.html.
 */

using UnityEngine;

namespace Live2D.Cubism.Framework.Motion
{
    public class CubismMotionState
    {
        public AnimationClip Clip { get; private set; }
        public bool IsLoop { get; private set; }
        public float Speed { get; private set; }

        public static CubismMotionState CreateCubismMotionState(AnimationClip clip, bool isLoop = true, float speed = 1.0f)
        {
            return new CubismMotionState
            {
                Clip = clip,
                IsLoop = isLoop,
                Speed = speed
            };
        }
    }
}
