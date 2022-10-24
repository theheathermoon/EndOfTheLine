// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/UI Color GaussianBlur"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_BlurSize("Blur Size", Range(0,10)) = 0

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
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

		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask[_ColorMask]

		GrabPass{}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragVertical
			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			ENDCG
		}

		GrabPass{}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragHorizontal
			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			ENDCG
		}

		GrabPass{}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragVertical
			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			ENDCG
		}

		GrabPass{}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragHorizontal
			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			ENDCG
		}
	}

	CGINCLUDE
	#include "UnityCG.cginc"
	#include "UnityUI.cginc"
	#include "GaussianBlur.cginc"

		struct appdata_t
		{
			float4 vertex   : POSITION;
			float4 color    : COLOR;
			float2 texcoord : TEXCOORD0;
		};

		struct v2f
		{
			float4 vertex   : SV_POSITION;
			fixed4 color : COLOR;
			half2 texcoord  : TEXCOORD0;
			float4 worldPosition : TEXCOORD1;
			float4 screenPos : TEXCOORD2;
		};

		sampler2D _GrabTexture : register(s0);
		sampler2D _MainTex;
		fixed4 _Color;
		fixed4 _TextureSampleAdd;
		float4 _ClipRect;
		float _BlurSize;

		v2f vert(appdata_t IN)
		{
			v2f OUT;
			OUT.worldPosition = IN.vertex;
			OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
			OUT.screenPos = ComputeScreenPos(OUT.vertex);
			OUT.texcoord = IN.texcoord;

#ifdef UNITY_HALF_TEXEL_OFFSET
			OUT.vertex.xy += (_ScreenParams.zw - 1.0)*float2(-1, 1);
#endif

			OUT.color = IN.color * _Color;
			return OUT;
		}

		fixed4 fragVertical(v2f i) : SV_Target
		{
			float2 screenPos = i.screenPos.xy / i.screenPos.w;
			half4 blurredColor = GaussianTap9(_GrabTexture, screenPos, float2(1.0, 0.0), _BlurSize);
			half4 uiColor = (tex2D(_MainTex, i.texcoord) + _TextureSampleAdd) * i.color;
			fixed4 color = float4(blurredColor.rgb, 1.0) * uiColor;
			color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);

#ifdef UNITY_UI_ALPHACLIP
			clip(color.a - 0.001);
#endif
			return color;
		}

		fixed4 fragHorizontal(v2f i) : SV_Target
		{
			float2 screenPos = i.screenPos.xy / i.screenPos.w;
			half4 blurredColor = GaussianTap9(_GrabTexture, screenPos, float2(0.0, 1.0), _BlurSize);
			half4 uiColor = (tex2D(_MainTex, i.texcoord) + _TextureSampleAdd) * i.color;
			fixed4 color = float4(blurredColor.rgb, 1.0) * uiColor;
			color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);

#ifdef UNITY_UI_ALPHACLIP
			clip(color.a - 0.001);
#endif
			return color;
		}

	ENDCG
}
