using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScrollRectFix))]
public class ScrollRectFixEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 绘制默认的可序列化字段
        base.OnInspectorGUI();

        // 添加对不可见委托的说明
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "以下字段需要通过代码设置：\n\n" +
            "- refreshAction: 需要返回IEnumerator的异步刷新操作\n" +
            "- postRefreshAction: 刷新完成后的回调操作",
            MessageType.Info
        );
    }
}
