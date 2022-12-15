  Shader "SwordMaster/Ghost" 
  {
    Properties 
	{
      _MainTex ("Main Texture", 2D) = "white" {}
      _BumpTex ("Bump Texture", 2D) = "bump" {}
      _GhostColor ("Ghost Color", Color) = (0.0,0.8,1.0,0.0)
	  _Brightness ("Brightness",Range(0.0,5.0)) = 1.5    
    }

    SubShader 
	{
      Tags { "RenderType" = "Transparent"  "Queue"="Transparent" "IgnoreProjector"="True"}

	  Pass 
	  {
		ZWrite On
		ColorMask 0
	  }
	
	  Pass 
	  {
		Tags { "LightMode" = "ForwardBase" }

		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off 
		ColorMask RGB

		CGPROGRAM

		#pragma vertex vert
		#pragma fragment frag
		#pragma multi_compile_instancing
		#pragma multi_compile_fog
		#pragma multi_compile_fwdbasealpha 
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#include "AutoLight.cginc"

		struct v2f 
		{
			float4 pos : SV_POSITION;
			float4 uv : TEXCOORD0;
			float4 tSpace0 : TEXCOORD1;
			float4 tSpace1 : TEXCOORD2;
			float4 tSpace2 : TEXCOORD3;
			float4 screenPos : TEXCOORD4;
			UNITY_FOG_COORDS(5)
			UNITY_VERTEX_INPUT_INSTANCE_ID
			UNITY_VERTEX_OUTPUT_STEREO
		};

		sampler2D _MainTex;
		float4 _MainTex_ST;
		sampler2D _BumpTex;	   
		float4 _BumpTex_ST;
		float4 _GhostColor;
		float _Brightness;
      

		v2f vert (appdata_full v) 
		{
			UNITY_SETUP_INSTANCE_ID(v);
			v2f o;
			UNITY_INITIALIZE_OUTPUT(v2f,o);
			UNITY_TRANSFER_INSTANCE_ID(v,o);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.uv.zw = TRANSFORM_TEX(v.texcoord, _BumpTex);
			float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			float3 worldNormal = UnityObjectToWorldNormal(v.normal);
			fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
			fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
			fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
			o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
			o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
			o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
			o.screenPos = ComputeScreenPos (o.pos);
			UNITY_TRANSFER_FOG(o,o.pos); 
			return o;
		}

		fixed4 frag (v2f i) : SV_Target 
		{
			UNITY_SETUP_INSTANCE_ID(i);
			
			float3 worldPos = float3(i.tSpace0.w, i.tSpace1.w, i.tSpace2.w);

			#ifndef USING_DIRECTIONAL_LIGHT
			fixed3 worldSpacelightDir = normalize(UnityWorldSpaceLightDir(worldPos));
			#else
			fixed3 worldSpacelightDir = _WorldSpaceLightPos0.xyz;
			#endif

			float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
            
      		half4 mainTexColor = tex2D (_MainTex, i.uv.xy);

			fixed3 normal = UnpackNormal (tex2D (_BumpTex, i.uv.zw));
			float3 worldNormal;
			worldNormal.x = dot(i.tSpace0.xyz, normal);
			worldNormal.y = dot(i.tSpace1.xyz, normal);
			worldNormal.z = dot(i.tSpace2.xyz, normal);

			      
     		half3 graycol = dot(mainTexColor.rgb,float3(0.3,0.59,0.11));

			half rim = 1.0 - saturate(dot (worldViewDir, worldNormal));
			fixed3 emission = _GhostColor.rgb * rim * _Brightness;
			fixed transparency = rim;

			UNITY_LIGHT_ATTENUATION(atten, i, worldPos)
		  
			fixed diff = max (0, dot (worldNormal, worldSpacelightDir));
			fixed4 diffColor;
			diffColor.rgb = graycol * _LightColor0.rgb * diff;
			diffColor.a = transparency;

			fixed4 c = 0;
			c += diffColor;
			c.rgb += emission;
			UNITY_APPLY_FOG(i.fogCoord, c);
			return c;
		}

		ENDCG
	  }

    } 

  }