#if false
using Live2D.Cubism.Framework.Json;
using UnityEngine;

using UnityEditor;
using UnityEditor.Animations;

using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Live2D
{
    public class DynamicAnimatorSetup : MonoBehaviour
    {
        [SerializeField] private string modelJsonPath;
        private Animator animator;
        private AnimatorState idleState;
        [SerializeField] public Dictionary<string, string[]> motionPaths;
        [SerializeField] private Dictionary<string, int> motionIndices = new Dictionary<string, int>();

        private const float TRANSITION_START = 0.92f; // 当前动画播放92%时开始过渡
        private const float TRANSITION_END = 0f;   // 下一个动画从0%开始
        private const float MIN_TRANSITION_DURATION = 0.2f; // 最小过渡时间0.2秒

        public void Initialize(string jsonPath)
        {
            modelJsonPath = jsonPath;
            StartSetup();
        }

        void StartSetup()
        {
            LoadMotionPaths();
            animator = GetComponent<Animator>() ?? gameObject.AddComponent<Animator>();

            AnimatorController controller = new AnimatorController
            {
                name = FileManager.GetParentFolderName(modelJsonPath)
            };

            controller.AddLayer("BaseLayer");
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

            foreach (var motionGroup in motionPaths.Keys.Where(k => k != "Idle"))
            {
                controller.AddParameter(motionGroup, AnimatorControllerParameterType.Trigger);
            }

            SetupIdleState(stateMachine);
            SetupMotionStates(stateMachine);

            animator.runtimeAnimatorController = controller;

            AssetDatabase.CreateAsset(controller, 
                $"Assets/Animate/{controller.name}.controller"
            );
            AssetDatabase.SaveAssets();
            animator.Play("Idle");
        }

        private void LoadMotionPaths()
        {
            var modelJson = CubismModel3Json.LoadAtPath(modelJsonPath, FileManager.LoadAssetAtPath);
            var motions = modelJson.FileReferences.Motions;

            motionPaths = new Dictionary<string, string[]>();
            string modelJsonDir = Path.GetDirectoryName(modelJsonPath);

            if (motions.GroupNames != null && motions.Motions != null && motions.GroupNames.Length == motions.Motions.Length)
            {
                for (int i = 0; i < motions.GroupNames.Length; i++)
                {
                    string groupName = motions.GroupNames[i];
                    var motionArray = motions.Motions[i];
                    if (motionArray != null && motionArray.Length > 0)
                    {
                        motionPaths[groupName] = motionArray.Select(m =>
                            Path.GetFullPath(Path.Combine(modelJsonDir, m.File)).Replace("\\", "/")
                        ).ToArray();
                        motionIndices[groupName] = 0;
                    }
                }
            }
        }

        private void SetupIdleState(AnimatorStateMachine stateMachine)
        {
            idleState = stateMachine.AddState("Idle");
            AnimatorStateMachine idleSubStateMachine = stateMachine.AddStateMachine("IdleSubMachine");
            AnimationClip[] idleClips = LoadMotionClips(motionPaths.ContainsKey("Idle") ? motionPaths["Idle"] : new string[0]);

            AnimatorState[] idleSubStates = new AnimatorState[idleClips.Length];
            for (int i = 0; i < idleClips.Length; i++)
            {
                idleSubStates[i] = idleSubStateMachine.AddState($"Idle_{i}");
                idleSubStates[i].motion = idleClips[i];
            }

            if (idleClips.Length > 0)
            {
                stateMachine.defaultState = idleState;
                AnimatorStateTransition entryTransition = idleState.AddTransition(idleSubStates[0]);
                entryTransition.hasExitTime = true;
                entryTransition.exitTime = 0f;
                entryTransition.duration = 0f;
                idleSubStateMachine.defaultState = idleSubStates[0];
            }
        }

        private void SetupMotionStates(AnimatorStateMachine stateMachine)
        {
            foreach (var motionGroup in motionPaths.Keys.Where(k => k != "Idle"))
            {
                AddMotionState(stateMachine, motionGroup, LoadMotionClips(motionPaths[motionGroup]));
            }
        }

        private void AddMotionState(AnimatorStateMachine stateMachine, string stateName, AnimationClip[] clips)
        {
            AnimatorState motionState = stateMachine.AddState(stateName);
            AnimatorStateMachine motionSubStateMachine = stateMachine.AddStateMachine(stateName + "SubMachine");

            AnimatorState[] subStates = new AnimatorState[clips.Length];
            for (int i = 0; i < clips.Length; i++)
            {
                subStates[i] = motionSubStateMachine.AddState($"{stateName}_{i}");
                subStates[i].motion = clips[i];
            }

            AnimatorStateTransition toSubMachine = motionState.AddTransition(motionSubStateMachine);
            toSubMachine.hasExitTime = true;
            toSubMachine.exitTime = 0f;
            toSubMachine.duration = 0f;

            AnimatorStateTransition anyStateTransition = stateMachine.AddAnyStateTransition(motionState);
            anyStateTransition.hasExitTime = false; // 立即响应Trigger
            anyStateTransition.duration = 0f;
            anyStateTransition.AddCondition(AnimatorConditionMode.If, 0, stateName);

            AnimatorStateMachine idleSubStateMachine = stateMachine.stateMachines
                .First(sm => sm.stateMachine.name == "IdleSubMachine").stateMachine;
            AnimatorState[] idleStates = idleSubStateMachine.states
                .Where(s => s.state.name.StartsWith("Idle_")).Select(s => s.state).ToArray();

            foreach (var subState in subStates)
            {
                if (idleStates.Length > 0)
                {
                    int randomIdleIndex = Random.Range(0, idleStates.Length);
                    AnimatorStateTransition toIdle = subState.AddTransition(idleStates[randomIdleIndex]);
                    toIdle.hasExitTime = true;
                    toIdle.exitTime = TRANSITION_START; // 92%时开始过渡

                    // 计算过渡时间：取动画剩余时间和0.2秒的最大值
                    float clipLength = subState.motion.averageDuration;
                    float remainingTime = clipLength * (1f - TRANSITION_START);
                    toIdle.duration = Mathf.Max(remainingTime, MIN_TRANSITION_DURATION);

                    // 设置目标动画的起始时间为8%
                    toIdle.offset = TRANSITION_END;
                }
            }
        }

        private AnimationClip[] LoadMotionClips(string[] motionPaths)
        {
            AnimationClip[] clips = new AnimationClip[motionPaths.Length];
            for (int i = 0; i < motionPaths.Length; i++)
            {
                var motion3JsonString = File.ReadAllText(motionPaths[i]);
                CubismMotion3Json motion3Json = CubismMotion3Json.LoadFrom(motion3JsonString);
                clips[i] = motion3Json.ToAnimationClip(poseJson: null);
                clips[i].name = Path.GetFileNameWithoutExtension(motionPaths[i]);
            }
            return clips;
        }

        public int GetIndexForGroup(string groupName)
        {
            return motionIndices.ContainsKey(groupName) ? motionIndices[groupName] : 0;
        }

        public void SetIndexForGroup(string groupName, int index)
        {
            motionIndices[groupName] = index;
        }

        public void TriggerMotionWithIndex(string groupName, int index)
        {
            if (animator != null && motionPaths.ContainsKey(groupName))
            {
                index = Mathf.Clamp(index, 0, motionPaths[groupName].Length - 1);
                string stateName = groupName;

                if (groupName == "Idle")
                {
                    animator.Play($"Idle_{index}", 0, 0f);
                }
                else
                {
                    animator.Play($"{stateName}_{index}", 0, 0f);
                }
                animator.Update(0f);
            }
        }

        public void TriggerState(string stateName)
        {
            animator.SetTrigger(stateName);
        }
    }
}
#endif