
using UnityEngine;
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework;
using Live2D.Cubism.Framework.Raycasting;
using Live2D.Cubism.Framework.Expression;
using Live2D.Cubism.Framework.Motion;
using System.Collections.Generic;
using System;


namespace Live2D
{
    public class MotionController : MonoBehaviour
    {
        
        private DynamicFadeMotionSetup _motionSetup;
        private CubismMotionController _motionController;
        private CubismExpressionController _expressionController;
        private List<HitDrawableInfomation> _hasHitDrawables;

        private HitRaycaster _raycaster;
        private CubismRaycastHit[] _raycastResults;
        private enum HitArea
        {
            Head,
            Body
        }
        private struct HitDrawableInfomation
        {
            public CubismDrawable drawable;
            public HitArea hitArea;
        }

        public void Initialize(CubismModel model)
        {
            // Get components.
            _motionSetup = GetComponent<DynamicFadeMotionSetup>();
            _motionController = _motionSetup.MotionController;
            _expressionController = GetComponent<DynamicExpressionSetup>().ExpressionController;

            _raycaster = GetComponent<HitRaycaster>();
            _raycaster.OnRaycastHit += OnRaycastHit;

            PostInit(model);
        }

        void OnDestroy()
        {
            _raycaster.OnRaycastHit -= OnRaycastHit;
        }

        void PostInit(CubismModel model)
        {

            // Get up to 4 results of collision detection.
            _raycastResults = new CubismRaycastHit[4];

            // Cache the drawable in which the component is set.
            _hasHitDrawables = new List<HitDrawableInfomation>();

            var hitAreas = Enum.GetValues(typeof(HitArea));
            var drawables = model.Drawables;


            for (var i = 0; i < hitAreas.Length; i++)
            {
                for (var j = 0; j < drawables.Length; j++)
                {
                    var cubismHitDrawable = drawables[j].GetComponent<CubismHitDrawable>();

                    if (cubismHitDrawable)
                    {
                        if (cubismHitDrawable.Name == hitAreas.GetValue(i).ToString())
                        {
                            var hitDrawable = new HitDrawableInfomation();
                            hitDrawable.drawable = drawables[j];
                            hitDrawable.hitArea = (HitArea)i;

                            _hasHitDrawables.Add(hitDrawable);
                            break;
                        }
                    }
                }
            }
        }

        private void OnRaycastHit(int hitCount)
        {

            // Motion playback according to the hit location.
            for (var i = 0; i < hitCount; i++)
            {
                var hitDrawable = _raycastResults[i].Drawable;

                for (var j = 0; j < _hasHitDrawables.Count; j++)
                {
                    if (hitDrawable == _hasHitDrawables[j].drawable)
                    {
                        var hitArea = _hasHitDrawables[j].hitArea;

                        // Tap body.
                        if (hitArea == HitArea.Body)
                        {
                            Debug.Log("Tap body.");
                        }
                        // Tap head.
                        else if (hitArea == HitArea.Head)
                        {
                            Debug.Log("Tap head.");
                        }
                        PlayMotion();
                        SetExpression();

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 从除 "Idle" 外的所有组中随机选择一组并播放一个动画。
        /// </summary>
        public void PlayMotion()
        {
            var motionClipsByGroup = _motionSetup.motionClipsByGroup;
            
            if (motionClipsByGroup == null || motionClipsByGroup.Count == 0)
            {
                Debug.LogWarning("No motion groups available.");
                return;
            }

            // 筛选出除 "Idle" 外的组
            var validGroups = new List<string>();
            foreach (var group in motionClipsByGroup.Keys)
            {
                if (group != "Idle")
                {
                    validGroups.Add(group);
                }
            }

            if (validGroups.Count == 0)
            {
                return;
            }

            // 随机选择一组
            int randomGroupIndex = UnityEngine.Random.Range(0, validGroups.Count);
            string selectedGroup = validGroups[randomGroupIndex];
            var clips = motionClipsByGroup[selectedGroup];

            if (clips.Count == 0)
            {
                return;
            }

            // 随机选择一个动作
            int randomMotionIndex = UnityEngine.Random.Range(0, clips.Count);
            var clip = clips[randomMotionIndex];
            _motionController.PlayLegacyAnimation(clip, layerIndex: 0, priority: 2, isLoop: false);
            Debug.Log($"Randomly playing motion from group '{selectedGroup}' at index {randomMotionIndex}.");
        }

        /// <summary>
        /// 随机设置一个表情。
        /// </summary>
        public void SetExpression()
        {
            int randomMotionIndex = UnityEngine.Random.Range(0, _expressionController.ExpressionsList.CubismExpressionObjects.Length - 1);
            _expressionController.CurrentExpressionIndex = randomMotionIndex;
        }
    }
}