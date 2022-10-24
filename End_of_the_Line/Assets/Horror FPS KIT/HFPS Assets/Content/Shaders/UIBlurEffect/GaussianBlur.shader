// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/UI GaussianBlur" {
    Properties {
		_Color("Color", Color) = (1,1,1,1)
		_BlurSize("Blur Size", Range(0,10)) = 0
	}

	SubShader {
		Tags { "Queue" = "Transparent" }

		ZWrite Off

        GrabPass { }
        Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragVertical 
			ENDCG
        }

		GrabPass{}
		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragHorizontal 
			ENDCG
		}

		GrabPass{}
		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragVertical 
			ENDCG
		}

		GrabPass{}
		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragHorizontal 
			ENDCG
		}
    }
	Fallback Off

	CGINCLUDE
	#include "UnityCG.cginc"
	#include "GaussianBlur.cginc"

	sampler2D _GrabTexture : register(s0);
	float _BlurSize;
	float4 _Color;

	struct v2f {
		float4 position : POSITION;
		float4 screenPos : TEXCOORD0;
	};

	v2f vert(appdata_base i) {
		v2f o;
		o.position = UnityObjectToClipPos(i.vertex);
		o.screenPos = ComputeScreenPos(o.position);
		return o;
	}

	half4 fragVertical(v2f i) : COLOR {
		float2 screenPos = i.screenPos.xy / i.screenPos.w;
		half4 blurredColor = GaussianTap9(_GrabTexture, screenPos, float2(1.0, 0.0), _BlurSize);
		return float4(blurredColor.rgb, 1.0) * _Color;
	}

	half4 fragHorizontal(v2f i) : COLOR {
		float2 screenPos = i.screenPos.xy / i.screenPos.w;
		half4 blurredColor = GaussianTap9(_GrabTexture, screenPos, float2(0.0, 1.0), _BlurSize);
		return float4(blurredColor.rgb, 1.0) * _Color;
	}
	ENDCG
} 