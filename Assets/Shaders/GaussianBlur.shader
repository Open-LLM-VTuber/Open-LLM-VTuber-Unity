Shader "Unlit/GaussianBlur" {
    Properties {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BlurRadius ("Blur Radius", Range(0, 10)) = 2
        _Alpha ("Blur Transparency", Range(0, 1)) = 0.5
        // 以下是为 UI.Mask 兼容性设置的属性
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader {
        Tags { 
            "Queue"="Transparent" 
            "RenderType"="Transparent"
            "IgnoreProjector"="True" 
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Fog { Mode Off }
        ColorMask RGB
        Blend One OneMinusSrcAlpha

        // 为 UI.Mask 提供支持
        Stencil {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        ColorMask [_ColorMask]

        // 使用 GrabPass 捕获屏幕内容
        GrabPass { "_BackgroundTexture" }

        // 单 Pass 处理模糊效果
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f {
                float4 pos : SV_POSITION;
                float4 grabPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            v2f vert(appdata_base v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.pos);
                o.uv = v.texcoord;
                return o;
            }

            sampler2D _BackgroundTexture;
            float4 _BackgroundTexture_TexelSize;
            float _BlurRadius;
            float _Alpha;
            sampler2D _MainTex;
            float4 _Color;

            // 预计算的 5x5 高斯核权重
            static const half weights[25] = {
                0.003765, 0.015019, 0.023792, 0.015019, 0.003765,
                0.015019, 0.059912, 0.094907, 0.059912, 0.015019,
                0.023792, 0.094907, 0.150342, 0.094907, 0.023792,
                0.015019, 0.059912, 0.094907, 0.059912, 0.015019,
                0.003765, 0.015019, 0.023792, 0.015019, 0.003765
            };

            fixed4 frag(v2f i) : SV_Target {
                float2 uv = i.grabPos.xy / i.grabPos.w;
                float2 texelSize = _BackgroundTexture_TexelSize.xy * _BlurRadius;

                fixed4 sum = 0;
                int index = 0;

                // 手动展开 5x5 高斯模糊采样
                sum += tex2D(_BackgroundTexture, uv + float2(-2, -2) * texelSize) * weights[index++];
                sum += tex2D(_BackgroundTexture, uv + float2(-1, -2) * texelSize) * weights[index++];
                sum += tex2D(_BackgroundTexture, uv + float2(0, -2) * texelSize) * weights[index++];
                sum += tex2D(_BackgroundTexture, uv + float2(1, -2) * texelSize) * weights[index++];
                sum += tex2D(_BackgroundTexture, uv + float2(2, -2) * texelSize) * weights[index++];

                sum += tex2D(_BackgroundTexture, uv + float2(-2, -1) * texelSize) * weights[index++];
                sum += tex2D(_BackgroundTexture, uv + float2(-1, -1) * texelSize) * weights[index++];
                sum += tex2D(_BackgroundTexture, uv + float2(0, -1) * texelSize) * weights[index++];
                sum += tex2D(_BackgroundTexture, uv + float2(1, -1) * texelSize) * weights[index++];
                sum += tex2D(_BackgroundTexture, uv + float2(2, -1) * texelSize) * weights[index++];

                sum += tex2D(_BackgroundTexture, uv + float2(-2, 0) * texelSize) * weights[index++];
                sum += tex2D(_BackgroundTexture, uv + float2(-1, 0) * texelSize) * weights[index++];
                sum += tex2D(_BackgroundTexture, uv + float2(0, 0) * texelSize) * weights[index++];
                sum += tex2D(_BackgroundTexture, uv + float2(1, 0) * texelSize) * weights[index++];
                sum += tex2D(_BackgroundTexture, uv + float2(2, 0) * texelSize) * weights[index++];

                sum += tex2D(_BackgroundTexture, uv + float2(-2, 1) * texelSize) * weights[index++];
                sum += tex2D(_BackgroundTexture, uv + float2(-1, 1) * texelSize) * weights[index++];
                sum += tex2D(_BackgroundTexture, uv + float2(0, 1) * texelSize) * weights[index++];
                sum += tex2D(_BackgroundTexture, uv + float2(1, 1) * texelSize) * weights[index++];
                sum += tex2D(_BackgroundTexture, uv + float2(2, 1) * texelSize) * weights[index++];

                sum += tex2D(_BackgroundTexture, uv + float2(-2, 2) * texelSize) * weights[index++];
                sum += tex2D(_BackgroundTexture, uv + float2(-1, 2) * texelSize) * weights[index++];
                sum += tex2D(_BackgroundTexture, uv + float2(0, 2) * texelSize) * weights[index++];
                sum += tex2D(_BackgroundTexture, uv + float2(1, 2) * texelSize) * weights[index++];
                sum += tex2D(_BackgroundTexture, uv + float2(2, 2) * texelSize) * weights[index++];

                // 混合模糊后的背景与原始图像
                fixed4 original = tex2D(_MainTex, i.uv) * _Color;
                return lerp(sum, original, _Alpha);
            }
            ENDCG
        }
    }
}