Shader "Unlit/LinearGradient"
{
	// 该 shader 支持 2 个颜色，任意方向渐变，并可调节中间颜色的纵向位置，支持透明度

	Properties
	{
		_MainTex ("贴图", 2D) = "white" {} // 主贴图
		_Color1 ("颜色1", Color) = (1.0, 0.0, 0.0, 1.0) // 起始颜色
		_Color2 ("颜色2", Color) = (0.0, 1.0, 0.0, 1.0) // 结束颜色
	    _GradientDirection("渐变方向", Vector) = (0, 1, 0, 0) // 渐变方向
		[PowerSlider(2)] _Alpha("透明度", Range(0.0, 1.0)) = 1.0 // 透明度控制

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
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" } // 透明队列
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha // 透明混合
		ZWrite Off // 关闭深度写入
		ColorMask RGB
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
			// 定义顶点、片元渲染器，引入工具宏
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			// 程序传入顶点渲染器的参数
			struct a2v
			{
				float4 pos : POSITION; // 顶点位置
				float2 uv : TEXCOORD0; // UV 坐标
			};

			// 顶点渲染器传入片元渲染器
			struct v2f
			{
				float2 uv : TEXCOORD0; // UV 坐标
				float4 pos : SV_POSITION; // 裁剪空间位置
				float3 worldPos : TEXCOORD1; // 世界空间位置
			};

			// 定义参数：贴图、颜色、位置、透明度、渐变方向
			sampler2D _MainTex;
			fixed4 _Color1;
			fixed4 _Color2;
			float _Alpha;
			float3 _GradientDirection;

			// 顶点渲染器
			v2f vert (a2v v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.pos); // 模型空间转裁剪空间
				o.uv = v.uv; // 传递 UV 坐标
				o.worldPos = mul(unity_ObjectToWorld, v.pos).xyz; // 计算世界空间位置
				return o;
			}

			// 片元渲染器
			fixed4 frag (v2f i) : SV_Target
			{
				// 世界空间像素位置
				float3 worldPosition = i.worldPos;

				// 归一化渐变方向向量
				float3 normalizedDirection = normalize(_GradientDirection);

				// 计算像素在渐变方向上的投影比例
				float projection = dot(worldPosition, normalizedDirection);

				
				// 计算投影比例的最小值和最大值
				float minValue = min(worldPosition.x, min(worldPosition.y, worldPosition.z));
				float maxValue = max(worldPosition.x, max(worldPosition.y, worldPosition.z));

				// 归一化投影比例到 [0, 1] 范围
				float scaledProjection = saturate((projection - minValue) / (maxValue - minValue));

				// 根据投影比例插值颜色
				fixed4 col = lerp(_Color1, _Color2, scaledProjection);

				// 应用透明度
				col.a *= _Alpha;

				// 合成贴图
				return tex2D(_MainTex, i.uv) * col;
			}
			ENDCG
		}
	}

	Fallback "Transparent/Diffuse"
}