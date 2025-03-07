Shader "Custom/Edge_Fog"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _EdgeWidth ("Edge Width", Range(0, 0.5)) = 0.1
        _EdgePower ("Edge Blur Power", Range(0, 5)) = 1
        _BlurIntensity ("Blur Intensity", Range(0, 1)) = 0.2

        _Stencil ("Stencil ID", Float) = 0
        _StencilComp ("Stencil Comparison", Float) = 8
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15 // 默认写入所有颜色通道（RGBA）

        [Toggle(_UseUIAlphaClip)]
        _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }

        Stencil {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        ColorMask [_ColorMask]
        
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _EdgeWidth;
            float _EdgePower;
            float _BlurIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 基础纹理采样
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;

                // 计算水平和垂直方向的边缘距离
                float edgeX = min(i.uv.x, 1.0 - i.uv.x);
                float edgeY = min(i.uv.y, 1.0 - i.uv.y);

                // 对每个方向应用smoothstep
                float maskX = smoothstep(0.0, _EdgeWidth, edgeX);
                float maskY = smoothstep(0.0, _EdgeWidth, edgeY);

                // 合并水平和垂直的虚化因子
                float edgeMask = maskX * maskY;

                // 边缘虚化
                float edgeBlur = pow(edgeMask, _EdgePower);
                col.a *= edgeBlur;

                // 整体朦胧效果
                col.rgb = lerp(col.rgb, col.rgb * 0.8 + 0.2, _BlurIntensity);
                col.a = saturate(col.a * (1.2 - _BlurIntensity));

                return col;
            }
            ENDCG
        }
    }
}

