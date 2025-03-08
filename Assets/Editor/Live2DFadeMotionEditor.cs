#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Live2D
{
    [CustomEditor(typeof(DynamicFadeMotionSetup))]
    public class DynamicFadeMotionSetupEditor : Editor
    {
        private Dictionary<string, int> _groupIndices = new Dictionary<string, int>();
        private Dictionary<string, int> _groupPriorities = new Dictionary<string, int>();
        private Dictionary<string, bool> _groupIsLoops = new Dictionary<string, bool>();
        private Dictionary<string, int> _groupLayerIndices = new Dictionary<string, int>();

        private const float LabelWidth = 80f;    // 标签宽度
        private const float FieldWidth = 60f;    // 输入框宽度
        private const float ButtonWidth = 120f;  // 按钮宽度

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DynamicFadeMotionSetup setup = (DynamicFadeMotionSetup)target;
            var motionClipsByGroup = setup.motionClipsByGroup;

            if (motionClipsByGroup != null && motionClipsByGroup.Count > 0)
            {
                EditorGUILayout.Space(10f);
                EditorGUILayout.LabelField("Motion Controls", EditorStyles.boldLabel);

                foreach (var motionGroup in motionClipsByGroup)
                {
                    string groupName = string.IsNullOrEmpty(motionGroup.Key) ? "Unnamed" : motionGroup.Key;

                    // 组标题
                    EditorGUILayout.LabelField($"{groupName} (0-{motionGroup.Value.Count - 1})", EditorStyles.miniBoldLabel);
                    GUILayout.Space(5f);

                    // 第一行：Index 和 LayerIndex
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Index", GUILayout.Width(LabelWidth));
                    int index = EditorGUILayout.IntField(GetIndexForGroup(groupName), GUILayout.Width(FieldWidth));
                    index = Mathf.Clamp(index, 0, motionGroup.Value.Count - 1);
                    SetIndexForGroup(groupName, index);

                    EditorGUILayout.LabelField("Layer", GUILayout.Width(LabelWidth));
                    int layerIndex = EditorGUILayout.IntField(GetLayerIndexForGroup(groupName), GUILayout.Width(FieldWidth));
                    SetLayerIndexForGroup(groupName, layerIndex);
                    EditorGUILayout.EndHorizontal();

                    // 第二行：Priority 和 IsLoop
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Priority", GUILayout.Width(LabelWidth));
                    int priority = EditorGUILayout.IntField(GetPriorityForGroup(groupName), GUILayout.Width(FieldWidth));

                    EditorGUILayout.LabelField("Is Loop", GUILayout.Width(LabelWidth));
                    bool isLoop = EditorGUILayout.Toggle(GetIsLoopForGroup(groupName), GUILayout.Width(FieldWidth));
                    SetPriorityForGroup(groupName, priority);
                    SetIsLoopForGroup(groupName, isLoop);
                    EditorGUILayout.EndHorizontal();

                    // 播放按钮
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(LabelWidth); // 与上面的输入框对齐
                    if (GUILayout.Button($"Play [{index}]", GUILayout.Width(ButtonWidth)))
                    {
                        setup.PlayMotion(motionGroup.Key, index, layerIndex, priority, isLoop);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space(10f); // 组间距
                }
            }
        }

        private int GetIndexForGroup(string groupName)
        {
            return _groupIndices.TryGetValue(groupName, out int index) ? index : 0;
        }

        private void SetIndexForGroup(string groupName, int index)
        {
            _groupIndices[groupName] = index;
        }

        private int GetPriorityForGroup(string groupName)
        {
            return _groupPriorities.TryGetValue(groupName, out int priority) ? priority : 0;
        }

        private void SetPriorityForGroup(string groupName, int priority)
        {
            _groupPriorities[groupName] = priority;
        }

        private bool GetIsLoopForGroup(string groupName)
        {
            return _groupIsLoops.TryGetValue(groupName, out bool isLoop) ? isLoop : true;
        }

        private void SetIsLoopForGroup(string groupName, bool isLoop)
        {
            _groupIsLoops[groupName] = isLoop;
        }

        private int GetLayerIndexForGroup(string groupName)
        {
            return _groupLayerIndices.TryGetValue(groupName, out int layerIndex) ? layerIndex : 0;
        }

        private void SetLayerIndexForGroup(string groupName, int layerIndex)
        {
            _groupLayerIndices[groupName] = layerIndex;
        }
    }
}
#endif