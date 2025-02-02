Shader "Unlit/RoundCornerAA"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Radius ("Radius",float) = 0.5
        _Ratio("Height/Width",float )= 1
        _Smoothness ("Smoothness", float) = 0.01
        _RadiusMinSmooth ("radiusMinSmooth", float) = 0.495
        _RadiusMaxSmooth ("radiusMaxSmooth", float) = 0.505

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
        Tags { "RenderType"="Transparent" "Queue" = "Transparent"}
        LOD 100
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

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; // 顶点颜色
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 color : COLOR; // 顶点颜色
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Radius;
            float _Ratio;
            float _RadiusMinSmooth;
            float _RadiusMaxSmooth;
            float _ColorMask;
            float _UseUIAlphaClip;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.color = v.color; // 传递顶点颜色
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {  
                // 坐标等价到左下角
                float2 p = abs(step(0.5,i.uv) - i.uv);
                float dist = length(float2(p.x - _Radius, p.y * _Ratio - _Radius));
                // 使用 smoothstep 替代 step 来实现抗锯齿
                // float radiusMinSmooth = _Radius * (1 - _Smoothness);
                // float radiusMaxSmooth = _Radius * (1 + _Smoothness);
                float alpha = max(max(step(_Radius, p.x),
                             step(_Radius, p.y * _Ratio)),
                             1 - smoothstep(_RadiusMinSmooth, _RadiusMaxSmooth, dist));

                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                col.a *= alpha; // 应用透明度
                // 如果采用透明度裁剪
                if (_UseUIAlphaClip > 0)
                {
                    if (col.a < 0.05) // 近乎透明时丢弃
                        discard;
                }
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }


    CustomEditor "MyShaderInspector"
}