using UnityEngine;
using UnityEditor;

namespace Live2D
{
    [CustomEditor(typeof(DynamicAnimatorSetup))]
    public class DynamicAnimatorSetupEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DynamicAnimatorSetup setup = (DynamicAnimatorSetup)target;

            if (setup.motionPaths != null && setup.motionPaths.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Motion Controls", EditorStyles.boldLabel);

                foreach (var motionGroup in setup.motionPaths)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    string groupName = string.IsNullOrEmpty(motionGroup.Key) ? "Unnamed" : motionGroup.Key;
                    EditorGUILayout.LabelField($"{groupName} (0-{motionGroup.Value.Length-1})", GUILayout.Width(150));

                    // 输入框用于指定index
                    int index = EditorGUILayout.IntField(setup.GetIndexForGroup(motionGroup.Key), GUILayout.Width(50));
                    index = Mathf.Clamp(index, 0, motionGroup.Value.Length - 1);
                    setup.SetIndexForGroup(motionGroup.Key, index);

                    // 播放按钮
                    if (GUILayout.Button($"Play {groupName} [{index}]"))
                    {
                        setup.TriggerMotionWithIndex(motionGroup.Key, index);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
        }
    }

}
