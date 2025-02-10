Shader "Unlit/XDirectionGradient"
{
    Properties
    {
        // 主纹理
        _MainTex ("Texture", 2D) = "white" {}
        // 颜色1
        _Color1 ("Color 1", Color) = (1, 0, 0, 1) // 红色
        // 颜色2
        _Color2 ("Color 2", Color) = (0, 1, 0, 1) // 绿色
        // 颜色3
        _Color3 ("Color 3", Color) = (0, 0, 1, 1) // 蓝色
        // 渐变位置1
        _Pos1 ("Position 1", Range(0, 1)) = 0.3
        // 渐变位置2
        _Pos2 ("Position 2", Range(0, 1)) = 0.7

        // required for UI.Mask
        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255
        _ColorMask("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha // 透明度混合
        LOD 200
        // required for UI.Mask
        Stencil
        {
            Ref[_Stencil]
            Comp[_StencilComp]
            Pass[_StencilOp]
            ReadMask[_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }
        ColorMask[_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // 属性变量
            sampler2D _MainTex;
            float4 _MainTex_ST; // 纹理的缩放和偏移
            fixed4 _Color1;
            fixed4 _Color2;
            fixed4 _Color3;
            float _Pos1;
            float _Pos2;

            // 顶点着色器输入
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            // 顶点着色器输出（传递给片元着色器）
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // 顶点着色器
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // 将顶点从模型空间转换到裁剪空间
                o.uv = TRANSFORM_TEX(v.uv, _MainTex); // 应用纹理的缩放和偏移
                return o;
            }

            // 片元着色器
            fixed4 frag (v2f i) : SV_Target
            {
                // 插值权重
                float lp = 0.0;

                // 根据X坐标计算颜色插值
                if (i.uv.x >= _Pos1)
                {
                    // 在 _Pos1 到 1 之间，使用 _Color1 和 _Color2 插值
                    lp = (1 - i.uv.x) / (1 - _Pos1);
                    return lerp(_Color1, _Color2, lp) * tex2D(_MainTex, i.uv);
                }
                else if (i.uv.x <= _Pos2)
                {
                    // 在 0 到 _Pos2 之间，使用 _Color3 和 _Color2 插值
                    lp = i.uv.x / _Pos2;
                    return lerp(_Color3, _Color2, lp) * tex2D(_MainTex, i.uv);
                }
                else
                {
                    // 在 _Pos2 到 _Pos1 之间，直接使用 _Color2
                    return _Color2 * tex2D(_MainTex, i.uv);
                }
            }
            ENDCG
        }
    }
    FallBack "Diffuse" // 回退Shader
}