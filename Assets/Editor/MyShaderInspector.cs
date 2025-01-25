using UnityEditor;
using UnityEngine;

public class MyShaderInspector : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material material = materialEditor.target as Material;

        // 检查当前 Shader 是否支持自定义属性
        if (material.shader.name == "Unlit/RoundCornerAA")
        {
            // 查找属性
            MaterialProperty ratioProp = FindProperty("_Ratio", properties, false);
            MaterialProperty radiusProp = FindProperty("_Radius", properties, false);
            MaterialProperty smoothnessProp = FindProperty("_Smoothness", properties, false);
            MaterialProperty radiusMinSmoothProp = FindProperty("_RadiusMinSmooth", properties, false);
            MaterialProperty radiusMaxSmoothProp = FindProperty("_RadiusMaxSmooth", properties, false);

            if (ratioProp != null && radiusProp != null && smoothnessProp != null &&
                radiusMinSmoothProp != null && radiusMaxSmoothProp != null)
            {
                EditorGUI.BeginChangeCheck();
                float ratio = ratioProp.floatValue;
                ratio = EditorGUILayout.Slider("Height/Width", ratio, 0, 100);
                float radius = radiusProp.floatValue;
                radius = EditorGUILayout.Slider("Radius", radius, 0, 1);
                float smoothness = smoothnessProp.floatValue;
                smoothness = EditorGUILayout.Slider("Smoothness", smoothness, 0, 1);

                if (EditorGUI.EndChangeCheck())
                {
                    ratioProp.floatValue = ratio;
                    radiusProp.floatValue = radius;
                    smoothnessProp.floatValue = smoothness;

                    // 根据公式更新 Value2 和 Value3
                    radiusMinSmoothProp.floatValue = radius * (1 - smoothness);
                    radiusMaxSmoothProp.floatValue = radius * (1 + smoothness);
                }

                // 显示 Value2 和 Value3（只读）
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.FloatField("radiusMinSmooth", radiusMinSmoothProp.floatValue);
                EditorGUILayout.FloatField("radiusMaxSmooth", radiusMaxSmoothProp.floatValue);
                EditorGUI.EndDisabledGroup();
            }
        }
        else
        {
            EditorGUILayout.LabelField("This material does not support custom properties.");
        }

        // 显示其他属性
        base.OnGUI(materialEditor, properties);
    }
}