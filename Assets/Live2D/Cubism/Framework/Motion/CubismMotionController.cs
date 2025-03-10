using Live2D.Cubism.Framework.MotionFade;
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System.Collections;

namespace Live2D.Cubism.Framework.Motion
{
    [RequireComponent(typeof(CubismFadeController), typeof(Animator))]
    public class CubismMotionController : MonoBehaviour
    {
        #region Variables

        [SerializeField] private int layerCount = 1; // 图层数量，Inspector中可配置
        private CubismFadeMotionList fadeMotionList;
        private Animator parentAnimator;
        private Animator animator;
        private AnimatorOverrideController overrideController;
        private int[] motionPriorities;    // 每个层的动画优先级
        private bool[] isLayerPlaying;     // 每个层的播放状态
        private bool[] shouldLoop;         // 每个层的循环状态
        private bool isActive;

        // 事件委托
        public Action<int, AnimationClip> OnAnimationStart; // 动画开始时触发
        public Action<int, AnimationClip> OnAnimationEnd;   // 动画结束时触发

        private Animation[] layerAnimations;

        #endregion

        #region Public Methods

        /// <summary>
        /// 播放指定层的动画，支持优先级、循环和速度控制。
        /// </summary>
        public void PlayAnimation(AnimationClip clip, int layerIndex = 0, int priority = CubismMotionPriority.PriorityNormal, 
            bool isLoop = true, float speed = 1.0f, Action onComplete = null)
        {
            if (!CanPlayAnimation(clip, layerIndex, priority))
            {
                Debug.LogWarning($"Cannot start motion: {clip?.name} on Layer {layerIndex}");
                return;
            }

            SetupAnimation(layerIndex, clip, priority, isLoop, speed);
            StartCoroutine(PlayAndMonitorLoop(layerIndex, clip, onComplete));
        }

        /// <summary>
        /// 停止指定层的动画。
        /// </summary>
        public void StopAnimation(int layerIndex)
        {
            if (!IsValidLayer(layerIndex) || !isLayerPlaying[layerIndex]) return;

            ResetLayerState(layerIndex);
            Debug.Log($"Stopped animation on Layer {layerIndex}");
        }

        #endregion

        #region Private Methods

        private bool CanPlayAnimation(AnimationClip clip, int layerIndex, int priority)
        {
            return enabled && isActive && fadeMotionList != null && clip != null
                && IsValidLayer(layerIndex)
                && (priority == CubismMotionPriority.PriorityForce || motionPriorities[layerIndex] <= priority);
        }

        private bool IsValidLayer(int layerIndex)
        {
            return layerIndex >= 0 && layerIndex < layerCount;
        }

        private void SetupAnimation(int layerIndex, AnimationClip clip, int priority, bool loop, float speed)
        {
            motionPriorities[layerIndex] = priority;
            shouldLoop[layerIndex] = loop;
            isLayerPlaying[layerIndex] = true;

            string stateName = $"Layer{layerIndex}";
            overrideController[stateName] = clip;

            animator.SetLayerWeight(layerIndex, 1.0f);
            animator.speed = speed;
            animator.Play(stateName, layerIndex, 0f);

            OnAnimationStart?.Invoke(layerIndex, clip);
            Debug.Log($"Playing {clip.name} on Layer {layerIndex} | Priority: {priority} | Loop: {loop} | Speed: {speed}");
        }

        private IEnumerator PlayAndMonitorLoop(int layerIndex, AnimationClip clip, Action onComplete)
        {
            while (isLayerPlaying[layerIndex])
            {
                yield return new WaitForSeconds(clip.length / animator.speed);

                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
                if (stateInfo.IsName($"Layer{layerIndex}") && stateInfo.normalizedTime >= 0.99f)
                {
                    OnAnimationEnd?.Invoke(layerIndex, clip);
                    Debug.Log($"Cycle ended: {clip.name} on Layer {layerIndex}");

                    if (shouldLoop[layerIndex] && isLayerPlaying[layerIndex])
                    {
                        animator.Play($"Layer{layerIndex}", layerIndex, 0f);
                        OnAnimationStart?.Invoke(layerIndex, clip);
                        Debug.Log($"Loop restarted: {clip.name} on Layer {layerIndex}");
                    }
                    else
                    {
                        ResetLayerState(layerIndex);
                        onComplete?.Invoke();
                        Debug.Log($"Animation ended: {clip.name} on Layer {layerIndex}");
                        yield break;
                    }
                }
            }
        }

        private void ResetLayerState(int layerIndex)
        {
            animator.SetLayerWeight(layerIndex, 0f);
            isLayerPlaying[layerIndex] = false;
            shouldLoop[layerIndex] = false;
            motionPriorities[layerIndex] = CubismMotionPriority.PriorityNone;
        }

        #endregion

        #region Unity Event Handlers

        private void OnEnable()
        {
            InitializeComponents();
            if (fadeMotionList == null)
            {
                Debug.LogError("CubismFadeMotionList is not set in CubismFadeController.");
                return;
            }
            // SyncAnimators();
            // InitializeOverrideController();
            InitializeStateArrays();
            InitializeAnimationComponents();
            isActive = true;
        }

        private void InitializeAnimationComponents()
        {
            layerAnimations = new Animation[layerCount];
            for (int i = 0; i < layerCount; i++)
            {
                Animation anim = gameObject.GetComponent<Animation>();
                if (anim == null)
                {
                    anim = gameObject.AddComponent<Animation>();
                }
                layerAnimations[i] = anim;
            }
        }

        private void InitializeComponents()
        {
            fadeMotionList = GetComponent<CubismFadeController>().CubismFadeMotionList;
            parentAnimator = transform.parent?.GetComponentInParent<Animator>();
            animator = GetComponent<Animator>();
        }

        private void InitializeOverrideController()
        {
            overrideController = new AnimatorOverrideController
            {
                runtimeAnimatorController = animator.runtimeAnimatorController
            };
            animator.runtimeAnimatorController = overrideController;
        }

        private void InitializeStateArrays()
        {
            motionPriorities = new int[layerCount];
            isLayerPlaying = new bool[layerCount];
            shouldLoop = new bool[layerCount];
            for (int i = 0; i < layerCount; i++)
            {
                motionPriorities[i] = CubismMotionPriority.PriorityNone;
                isLayerPlaying[i] = false;
                shouldLoop[i] = false;
            }
        }

        private void SyncAnimators()
        {
            animator.runtimeAnimatorController = parentAnimator.runtimeAnimatorController;

            for (int i = 0; i < parentAnimator.layerCount; i++)
            {
                AnimatorStateInfo state = parentAnimator.GetCurrentAnimatorStateInfo(i);
                animator.Play(state.fullPathHash, i, state.normalizedTime);
                animator.SetLayerWeight(i, parentAnimator.GetLayerWeight(i));
            }

            animator.speed = parentAnimator.speed;
            SyncAnimatorParameters();
            Debug.Log("Animators synchronized successfully!");
        }

        private void SyncAnimatorParameters()
        {
            foreach (AnimatorControllerParameter param in parentAnimator.parameters)
            {
                switch (param.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        animator.SetBool(param.name, parentAnimator.GetBool(param.name));
                        break;
                    case AnimatorControllerParameterType.Float:
                        animator.SetFloat(param.name, parentAnimator.GetFloat(param.name));
                        break;
                    case AnimatorControllerParameterType.Int:
                        animator.SetInteger(param.name, parentAnimator.GetInteger(param.name));
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        if (parentAnimator.GetBool(param.name))
                            animator.SetTrigger(param.name);
                        break;
                }
            }
        }

        #endregion
    
        #region Animation Component Methods

        /// <summary>
        /// 使用 Animation 组件播放旧版动画剪辑，支持优先级和速度。
        /// </summary>
        public void PlayLegacyAnimation(AnimationClip legacyClip, int layerIndex = 0, int priority = CubismMotionPriority.PriorityNormal, 
            bool isLoop = true, float speed = 1.0f, Action onComplete = null)
        {
            if (!CanPlayLegacyAnimation(legacyClip, layerIndex, priority))
            {
                Debug.LogWarning($"Cannot play legacy animation: {legacyClip?.name} on Layer {layerIndex}");
                return;
            }

            SetupLegacyAnimation(legacyClip, layerIndex, priority, isLoop, speed);
            StartCoroutine(MonitorLegacyAnimation(layerIndex, legacyClip, onComplete));
        }

        /// <summary>
        /// 停止指定层的旧版动画。
        /// </summary>
        public void StopLegacyAnimation(int layerIndex)
        {
            if (!IsValidLayer(layerIndex) || !isLayerPlaying[layerIndex]) return;

            layerAnimations[layerIndex].Stop();
            ResetLayerState(layerIndex);
            Debug.Log($"Stopped legacy animation on Layer {layerIndex}");
        }

        private bool CanPlayLegacyAnimation(AnimationClip clip, int layerIndex, int priority)
        {
            return enabled && isActive && clip != null && clip.legacy && IsValidLayer(layerIndex)
                && (priority == CubismMotionPriority.PriorityForce || motionPriorities[layerIndex] <= priority);
        }

        private void SetupLegacyAnimation(AnimationClip legacyClip, int layerIndex, int priority, bool isLoop, float speed)
        {
            // 设置 Animation 组件状态
            Animation animation = layerAnimations[layerIndex];
            animation.playAutomatically = false;
            animation.Stop(); // 停止当前动画

            // 添加或替换 Clip
            string clipName = $"Layer{layerIndex}_Legacy_{legacyClip.name}";
            animation.AddClip(legacyClip, clipName);

            // 配置播放参数
            AnimationState state = animation[clipName];
            state.speed = speed;
            state.wrapMode = isLoop ? WrapMode.Loop : WrapMode.Once;

            // 更新层状态
            motionPriorities[layerIndex] = priority;
            shouldLoop[layerIndex] = isLoop;
            isLayerPlaying[layerIndex] = true;

            // 开始播放
            animation.Play(clipName);

            OnAnimationStart?.Invoke(layerIndex, legacyClip);
            Debug.Log($"Playing legacy animation {legacyClip.name} on Layer {layerIndex} | Priority: {priority} | Loop: {isLoop} | Speed: {speed}");
        }

        private IEnumerator MonitorLegacyAnimation(int layerIndex, AnimationClip legacyClip, Action onComplete)
        {
            Animation animation = layerAnimations[layerIndex];
            AnimationState state = animation[$"Layer{layerIndex}_Legacy_{legacyClip.name}"];

            yield return new WaitForSeconds(legacyClip.length / state.speed);

            if (!shouldLoop[layerIndex] || !isLayerPlaying[layerIndex])
            {
                OnAnimationEnd?.Invoke(layerIndex, legacyClip);
                ResetLayerState(layerIndex);
                onComplete?.Invoke();
                Debug.Log($"Legacy animation {legacyClip.name} ended on Layer {layerIndex}");
            }
            else
            {
                Debug.Log($"Legacy animation {legacyClip.name} looping on Layer {layerIndex}");
            }
        }

        #endregion


    }
}