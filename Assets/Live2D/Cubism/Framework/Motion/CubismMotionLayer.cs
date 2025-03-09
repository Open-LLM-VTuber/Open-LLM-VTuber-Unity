/**
 * Copyright(c) Live2D Inc. All rights reserved.
 *
 * Use of this source code is governed by the Live2D Open Software license
 * that can be found at https://www.live2d.com/eula/live2d-open-software-license-agreement_en.html.
 */

using Live2D.Cubism.Framework.MotionFade;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Live2D.Cubism.Framework.Motion
{
    public class CubismMotionLayer : ICubismFadeState
    {
        #region Action

        public Action<int, float> AnimationEndHandler;

        #endregion

        #region Variable

        private List<CubismFadePlayingMotion> _playingMotions;
        private CubismFadeMotionList _cubismFadeMotionList;
        private int _layerIndex;
        private float _layerWeight;
        private bool _isFinished;
        private AnimationClip _currentClip;

        public bool IsFinished => _isFinished;

        #endregion

        #region Fade State Interface

        public List<CubismFadePlayingMotion> GetPlayingMotions() => _playingMotions;
        public bool IsDefaultState() => false;
        public float GetLayerWeight() => _layerWeight;
        public bool GetStateTransitionFinished() => true;
        public void SetStateTransitionFinished(bool isFinished) { }

        public void StopAnimation(int index)
        {
            if (index >= 0 && index < _playingMotions.Count)
            {
                _playingMotions.RemoveAt(index);
                if (_playingMotions.Count == 0) _isFinished = true;
            }
        }

        public void StopAnimationClip()
        {
            _playingMotions.Clear();
            _currentClip = null;
            _isFinished = true;
        }

        #endregion

        #region Function

        public static CubismMotionLayer CreateCubismMotionLayer(CubismFadeMotionList fadeMotionList, int layerIndex, float layerWeight = 1.0f)
        {
            var ret = new CubismMotionLayer
            {
                _cubismFadeMotionList = fadeMotionList,
                _layerIndex = layerIndex,
                _layerWeight = layerWeight,
                _isFinished = true,
                _playingMotions = new List<CubismFadePlayingMotion>()
            };
            return ret;
        }

        private CubismFadePlayingMotion CreateFadePlayingMotion(AnimationClip clip, bool isLooping, float speed = 1.0f)
        {
            var ret = new CubismFadePlayingMotion();
            var instanceId = -1;
            var events = clip.events;
            foreach (var evt in events)
            {
                if (evt.functionName == "InstanceId")
                {
                    instanceId = evt.intParameter;
                    break;
                }
            }

            bool isNotFound = true;
            for (int i = 0; i < _cubismFadeMotionList.MotionInstanceIds.Length; i++)
            {
                if (_cubismFadeMotionList.MotionInstanceIds[i] != instanceId) continue;

                isNotFound = false;
                ret.Speed = speed;
                ret.StartTime = Time.time;
                ret.FadeInStartTime = Time.time;
                ret.Motion = _cubismFadeMotionList.CubismFadeMotionObjects[i];
                ret.EndTime = ret.Motion.MotionLength <= 0 ? -1 : ret.StartTime + ret.Motion.MotionLength / speed;
                ret.IsLooping = isLooping;
                ret.Weight = 0.0f;
                break;
            }

            if (isNotFound)
            {
                Debug.LogError("CubismMotionController: Not found motion from CubismFadeMotionList.");
            }

            return ret;
        }

        public void PlayAnimation(AnimationClip clip, bool isLoop = true, float speed = 1.0f)
        {
            _currentClip = clip;

            if (_playingMotions.Count > 0)
            {
                var lastMotion = _playingMotions[_playingMotions.Count - 1];
                var time = Time.time;
                var newEndTime = time + lastMotion.Motion.FadeOutTime;
                if (newEndTime < 0.0f || newEndTime < lastMotion.EndTime) lastMotion.EndTime = newEndTime;

                while (lastMotion.IsLooping)
                {
                    if (lastMotion.StartTime + lastMotion.Motion.MotionLength >= time) break;
                    lastMotion.StartTime += lastMotion.Motion.MotionLength;
                }
                _playingMotions[_playingMotions.Count - 1] = lastMotion;
            }

            var playingMotion = CreateFadePlayingMotion(clip, isLoop, speed);
            _playingMotions.Add(playingMotion);
            _isFinished = false;
        }

        public void StopAllAnimation()
        {
            _playingMotions.Clear();
            _isFinished = true;
        }

        public void SetLayerWeight(float weight)
        {
            _layerWeight = weight;
        }

        public void SetStateSpeed(int index, float speed)
        {
            if (index < 0 || index >= _playingMotions.Count) return;
            var motion = _playingMotions[index];
            motion.Speed = speed;
            motion.EndTime = motion.Motion.MotionLength <= 0 ? -1 : Time.time + motion.Motion.MotionLength / speed;
            _playingMotions[index] = motion;
        }

        public void SetStateIsLoop(int index, bool isLoop)
        {
            if (index < 0 || index >= _playingMotions.Count) return;
            var motion = _playingMotions[index];
            motion.IsLooping = isLoop;
            _playingMotions[index] = motion;
            if (_currentClip != null) _currentClip.wrapMode = isLoop ? WrapMode.Loop : WrapMode.Once;
        }

        #endregion

        public void Update()
        {
            if (AnimationEndHandler == null || _playingMotions.Count != 1 || _isFinished || _currentClip == null) return;

            var motion = _playingMotions[0];
            if (!motion.IsLooping && Time.time > motion.EndTime)
            {
                _isFinished = true;
                var instanceId = -1;
                var events = _currentClip.events;
                foreach (var evt in events)
                {
                    if (evt.functionName == "InstanceId")
                    {
                        instanceId = evt.intParameter;
                        break;
                    }
                }
                AnimationEndHandler(_layerIndex, instanceId);
            }
        }
    }
}