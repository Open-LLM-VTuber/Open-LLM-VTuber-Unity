using Live2D.Cubism.Framework.MotionFade;
using System;
using UnityEngine;


namespace Live2D.Cubism.Framework.Motion
{
    [RequireComponent(typeof(CubismFadeController), typeof(Animator))]
    public class CubismMotionController : MonoBehaviour 
    {
        #region Action

        [SerializeField]
        public Action<float> AnimationEndHandler;

        private void OnAnimationEnd(int layerIndex, float instanceId)
        {
            _motionPriorities[layerIndex] = CubismMotionPriority.PriorityNone;
            AnimationEndHandler?.Invoke(instanceId);
        }

        #endregion

        #region Variable

        public int LayerCount = 1;

        private CubismFadeMotionList _cubismFadeMotionList;
        private bool _isActive = false;

        private Animator _parentAnimator;
        private Animator _animator;
        private AnimatorOverrideController _overrideController;

        private CubismMotionLayer[] _motionLayers;
        private int[] _motionPriorities;
        #endregion

        #region Function

        public void PlayAnimation(AnimationClip clip, int layerIndex = 0, int priority = CubismMotionPriority.PriorityNormal, bool isLoop = true, float speed = 1.0f)
        {
            if (!enabled || !_isActive || _cubismFadeMotionList == null || clip == null
                || layerIndex < 0 || layerIndex >= LayerCount ||
                ((_motionPriorities[layerIndex] >= priority) && priority != CubismMotionPriority.PriorityForce))
            {
                Debug.Log("Can't start motion.");
                return;
            }

            _motionPriorities[layerIndex] = priority;
            clip.wrapMode = isLoop ? WrapMode.Loop : WrapMode.Once;

            string stateName = $"Layer{layerIndex}";
            // _motionLayers[layerIndex].PlayAnimation(clip, isLoop, speed);
            Debug.Log($"Playing clip: {clip.name}, Length: {clip.length}, Loop: {isLoop}, Speed: {speed}");

            _overrideController[stateName] = clip;

            _animator.SetLayerWeight(layerIndex, 1.0f);
            _animator.speed = speed;
            _animator.Play(stateName, layerIndex, 0f);
        }

        public void StopAnimation(int animationIndex, int layerIndex = 0)
        {
            if (layerIndex < 0 || layerIndex >= LayerCount) return;
            _motionLayers[layerIndex].StopAnimation(animationIndex);
            if (_motionLayers[layerIndex].IsFinished) _animator.SetLayerWeight(layerIndex, 0f);
        }

        public void StopAllAnimation()
        {
            for (var i = 0; i < LayerCount; ++i)
            {
                _motionLayers[i].StopAnimationClip();
                _animator.SetLayerWeight(i, 0f);
            }
        }

        public bool IsPlayingAnimation(int layerIndex = 0)
        {
            if (layerIndex < 0 || layerIndex >= LayerCount) return false;
            return !_motionLayers[layerIndex].IsFinished;
        }

        public void SetLayerWeight(int layerIndex, float weight)
        {
            if (layerIndex < 0 || layerIndex >= LayerCount) return;
            _motionLayers[layerIndex].SetLayerWeight(weight);
            _animator.SetLayerWeight(layerIndex, weight);
        }

        public void SetLayerAdditive(int layerIndex, bool isAdditive)
        {
            if (layerIndex < 0 || layerIndex >= LayerCount) return;
            _animator.SetLayerWeight(layerIndex, isAdditive ? 1f : 0f);
        }

        public void SetAnimationSpeed(int layerIndex, int index, float speed)
        {
            if (layerIndex < 0 || layerIndex >= LayerCount) return;
            _motionLayers[layerIndex].SetStateSpeed(index, speed);
            _animator.speed = speed; // 全局速度，需优化为层级速度
        }

        public void SetAnimationIsLoop(int layerIndex, int index, bool isLoop)
        {
            if (layerIndex < 0 || layerIndex >= LayerCount) return;
            _motionLayers[layerIndex].SetStateIsLoop(index, isLoop);
        }

        public ICubismFadeState[] GetFadeStates()
        {
            if (_motionLayers == null)
            {
                LayerCount = Mathf.Max(1, LayerCount);
                _motionLayers = new CubismMotionLayer[LayerCount];
                _motionPriorities = new int[LayerCount];
            }
            return _motionLayers;
        }

        #endregion

        #region Unity Events Handling

        private void OnEnable()
        {
            _cubismFadeMotionList = GetComponent<CubismFadeController>().CubismFadeMotionList;
            if (_cubismFadeMotionList == null)
            {
                Debug.LogError("CubismMotionController: CubismFadeMotionList doesn't set in CubismFadeController.");
                return;
            }

            _parentAnimator = transform.parent?.GetComponentInParent<Animator>();
            _animator = GetComponent<Animator>();

            SyncAnimators();
            
            _overrideController = new()
            {
                runtimeAnimatorController = _animator.runtimeAnimatorController
            };
            _animator.runtimeAnimatorController = _overrideController;

            _isActive = true;

            if (_motionLayers == null)
            {
                LayerCount = Mathf.Max(1, LayerCount);
                _motionLayers = new CubismMotionLayer[LayerCount];
                _motionPriorities = new int[LayerCount];
            }

            for (var i = 0; i < LayerCount; ++i)
            {
                _motionLayers[i] = CubismMotionLayer.CreateCubismMotionLayer(_cubismFadeMotionList, i);
                _motionLayers[i].AnimationEndHandler += OnAnimationEnd;
            }
        }

        void SyncAnimators()
        {
            // 复制控制器
            _animator.runtimeAnimatorController = _parentAnimator.runtimeAnimatorController;

            // 同步动画状态和层权重
            for (int layerIndex = 0; layerIndex < _parentAnimator.layerCount; layerIndex++)
            {
                AnimatorStateInfo parentState = _parentAnimator.GetCurrentAnimatorStateInfo(layerIndex);
                _animator.Play(parentState.fullPathHash, layerIndex, parentState.normalizedTime);
                _animator.SetLayerWeight(layerIndex, _parentAnimator.GetLayerWeight(layerIndex));
            }

            // 同步速度
            _animator.speed = _parentAnimator.speed;

            // 同步参数
            foreach (AnimatorControllerParameter param in _parentAnimator.parameters)
            {
                switch (param.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        _animator.SetBool(param.name, _parentAnimator.GetBool(param.name));
                        break;
                    case AnimatorControllerParameterType.Float:
                        _animator.SetFloat(param.name, _parentAnimator.GetFloat(param.name));
                        break;
                    case AnimatorControllerParameterType.Int:
                        _animator.SetInteger(param.name, _parentAnimator.GetInteger(param.name));
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        if (_parentAnimator.GetBool(param.name))
                            _animator.SetTrigger(param.name);
                        break;
                }
            }

            Debug.Log("Animators synchronized successfully!");
        }

        private void OnDisable()
        {
            _isActive = false;
        }

        private void Update()
        {
            if (!_isActive) return;

            for (var i = 0; i < _motionLayers.Length; ++i)
            {
                _motionLayers[i].Update();
                if (_motionLayers[i].IsFinished) _motionPriorities[i] = 0;
            }
        }

        #endregion
    }
}