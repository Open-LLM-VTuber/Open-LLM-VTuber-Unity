using UnityEngine;
using UnityEditor;

namespace Live2D
{
    [CustomEditor(typeof(DynamicExpressionSetup))]
    public class Live2DExpressionEditor : Editor
    {
        private int expressionIndex = 0; // 用于存储输入的 index

        public override void OnInspectorGUI()
        {
            // 绘制默认的 Inspector 界面（例如 modelJsonPath 字段）
            DrawDefaultInspector();

            // 获取目标对象
            DynamicExpressionSetup setup = (DynamicExpressionSetup)target;

            // 添加一个整数输入字段
            expressionIndex = EditorGUILayout.IntField("Expression Index", expressionIndex);

            // 添加一个按钮
            if (GUILayout.Button("Apply Expression"))
            {
                // 检查是否在运行模式下
                if (EditorApplication.isPlaying)
                {
                    // 调用 SetExpression 方法
                    setup.SetExpression(expressionIndex);
                }
                else
                {
                    Debug.LogWarning("Please enter Play Mode to apply expressions.");
                }
            }
        }
    }
}