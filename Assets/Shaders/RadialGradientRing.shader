Shader "Unlit/RadialGradientRing" {
    Properties {
        [Header(Colors)]
        _Color1 ("Start Color", Color) = (1,0,0,1)   // 渐变起始颜色
        _Color2 ("End Color", Color) = (0,1,0,1)     // 渐变结束颜色

        [Header(Ring)]
        _Width ("Ring Width", Range(0,0.5)) = 0.2    // 环的宽度
        _InnerRadius ("Inner Radius", Range(0,0.5)) = 0.3  // 内半径

        [Header(Angle)]
        _Offset ("Gradient Offset", Range(0,1)) = 0  // 渐变偏移（0-1 对应 0°-360°）
        _EndAngle ("End Angle", Range(0,1)) = 1      // 终止角度（0-1 对应 0°-360°）

        _Stencil ("Stencil ID", Float) = 0
        _StencilComp ("Stencil Comparison", Float) = 8
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15 // 默认写入所有颜色通道（RGBA）

        [Toggle(_UseUIAlphaClip)]
        _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    
    SubShader {
        Tags { 
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
        
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        ZWrite Off
        ZTest Less 
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGB
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv - 0.5; // 将 UV 中心移到 (0,0)
                return o;
            }
            
            fixed4 _Color1, _Color2;
            float _Width, _InnerRadius, _Offset, _EndAngle;
            float _ColorMask;
            float _UseUIAlphaClip;

            fixed4 frag (v2f i) : SV_Target {
                float2 pos = i.uv;
                float distance = length(pos); // 计算半径
                
                // 计算角度（0-1 范围，包含偏移）
                float angle = (atan2(pos.y, pos.x) / (6.283185307) + 0.5 + _Offset) % 1;
                
                // 创建平滑环形
                float ring = smoothstep(_InnerRadius, _InnerRadius + 0.01, distance) * 
                           (1 - smoothstep(_InnerRadius + _Width, _InnerRadius + _Width + 0.01, distance));
                
                // 判断角度是否在范围内（0 到 _EndAngle）
                float inRange = step(0, angle) * step(angle, _EndAngle);
                
                // 获取颜色
                fixed4 color = lerp(_Color1, _Color2, angle);  // 使用颜色插值
                
                // 结合透明度
                color.a *= ring * inRange;

                // 如果采用透明度裁剪
                if (_UseUIAlphaClip > 0)
                {
                    if (color.a < 0.05) // 近乎透明时丢弃
                        discard;
                }

                return color;
            }
            ENDCG
        }
    }
}