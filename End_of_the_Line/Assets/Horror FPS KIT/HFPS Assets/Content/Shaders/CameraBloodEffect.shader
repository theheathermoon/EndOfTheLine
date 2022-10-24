// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/CameraBloodEffect"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BloodTex("Blood Texture", 2D) = "white"{}
		_BloodBump("Blood Normal", 2D) = "bump"{}
		_Distortion("Blood Distortion", Range(0,2)) = 1
		_BloodAmount("Blood Amount", Range(0, 1)) = 0

	}

	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

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
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _BloodTex;
			sampler2D _BloodBump;
			float	  _BloodAmount;
			float	  _Distortion;

			fixed4 frag (v2f i) : SV_Target
			{
				// Sample the Source Texture
				fixed4 bloodCol	= tex2D(_BloodTex, i.uv);
				bloodCol.a = saturate(bloodCol.a + ( _BloodAmount * 2 - 1 ));

				half2 bump = UnpackNormal(tex2D( _BloodBump , i.uv )).xy;
				fixed4 srcCol 	= tex2D(_MainTex, i.uv + bump * bloodCol.a * _Distortion);

				fixed4 overlayCol = srcCol * bloodCol * 2;
				overlayCol = lerp( srcCol, overlayCol , 0.75);
				fixed4 output = lerp( srcCol, overlayCol , bloodCol.a );
				return output;
			}
			ENDCG
		}
	}

}