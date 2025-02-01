Shader "Unlit/TransparentBlur"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {} // Sprite自带的贴图
        _Color("Tint", Color) = (1,1,1,1) // 混合预设的颜色
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0 // 用于Sprite像素对齐

        // blur采样贴图尺寸
        // 因为shader取值都是百分比，所以我们要定出贴图的尺寸来进行计算
        _TextureSize("_Texture Size", Float) = 256
        // 模糊半径
        _BlurRadius("_Blur Radius", Range(5, 5)) = 5

        // 模糊效果的透明度，0是完全模糊，1是完全清晰
        _Alpha("_Alpha", Range(0.0, 1.0)) = 0.5

        // 去色效果的透明度，0是原来颜色，1是完全黑白
        _GrayRate("_Gray Rate", Range(0.0, 1.0)) = 0

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
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Fog { Mode Off }
        ColorMask RGB
        Blend One OneMinusSrcAlpha

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

        GrabPass
        {
            "_BgColor"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile DUMMY PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                half2 texcoord : TEXCOORD0;
                float4 grabPos : TEXCOORD1; // for _BgColor UV
            };

            fixed4 _Color;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.grabPos = ComputeGrabScreenPos(OUT.vertex);
                OUT.color = IN.color * _Color;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            sampler2D _MainTex;
            sampler2D _BgColor;
            float _TextureSize;
            float _BlurRadius;

            // 高斯模糊算法
            float GetGaussianDistribution(float x, float y, float rho)
            {
                float g = 1.0f / sqrt(2.0f * 3.141592654f * rho * rho);
                return g * exp(-(x * x + y * y) / (2 * rho * rho));
            }

            float4 GetGaussBlurColor(sampler2D blurTex, float2 uv)
            {
                float space = 1.0 / _TextureSize;
                float rho = (float)_BlurRadius * space / 3.0;

                float weightTotal = 0.0f;
                // 预计算高斯权重, _BlurRadius = 5, 5 * 2 + 1 = 11
                float weights[11][11];
                for (int x = -_BlurRadius; x <= _BlurRadius; x++)
                {
                    for (int y = -_BlurRadius; y <= _BlurRadius; y++)
                    {
                        weights[x + _BlurRadius][y + _BlurRadius] = GetGaussianDistribution(x * space, y * space, rho);
                        weightTotal += weights[x + _BlurRadius][y + _BlurRadius];
                    }
                }

                // 归一化总权重
                float invWeightTotal = 1.0f / weightTotal;
    
                float4 colorTmp = float4(0, 0, 0, 0);
                for (int x = -_BlurRadius; x <= _BlurRadius; x++)
                {
                    for (int y = -_BlurRadius; y <= _BlurRadius; y++)
                    {
                        // 计算权重并加权采样
                        float weight = weights[x + _BlurRadius][y + _BlurRadius] * invWeightTotal;
                        float4 color = tex2D(blurTex, uv + float2(x * space, y * space));
                        colorTmp += color * weight;
                    }
                }

                return colorTmp;
            }

            float _Alpha;
            float _GrayRate;

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord); // 原始纹理颜色

                float2 sceneUVs = (IN.grabPos.xy / IN.grabPos.w); // 屏幕UV
                float4 blurCol = GetGaussBlurColor(_BgColor, sceneUVs); // 高斯模糊颜色

                c = (c * _Alpha) + (blurCol * (1 - _Alpha)); // 模糊与清晰的比重
                c *= IN.color; // 与顶点色和输入颜色混合

                float gray = dot(c.xyz, float3(0.299, 0.587, 0.114)); // 去色
                c.xyz = float3(gray, gray, gray) * _GrayRate + c.xyz * ((1 - _GrayRate));

                return c;
            }
            ENDCG
        }
    }
}