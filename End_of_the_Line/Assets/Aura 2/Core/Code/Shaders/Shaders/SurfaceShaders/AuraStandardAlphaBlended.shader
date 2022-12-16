
/***************************************************************************
*                                                                          *
*  Copyright (c) Raphaël Ernaelsten (@RaphErnaelsten)                      *
*  All Rights Reserved.                                                    *
*                                                                          *
*  NOTICE: Aura 2 is a commercial project.                                 * 
*  All information contained herein is, and remains the property of        *
*  Raphaël Ernaelsten.                                                     *
*  The intellectual and technical concepts contained herein are            *
*  proprietary to Raphaël Ernaelsten and are protected by copyright laws.  *
*  Dissemination of this information or reproduction of this material      *
*  is strictly forbidden.                                                  *
*                                                                          *
***************************************************************************/

Shader "Aura 2/Surface/Standard Alpha Blend"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 200
   
		CGPROGRAM

		#pragma exclude_renderers glcore gles gles3 d3d11_9x n3ds wiiu
		//						↓	 			 			   ↓↓↓↓↓↓↓ AURA 2 COPY THIS TOO ↓↓↓↓↓↓↓ (ATTENTION : Please verify the shading model used by your shader at the begining of the line here under. Use metaPass_Standard If your shading model is "Standard", or metaPass_StandardSpecular if your shading model is "StandardSpecular")
		#pragma surface surf Standard fullforwardshadows alpha finalcolor:metaPass_Standard
		//						↑							   ↑↑↑↑↑↑↑ AURA 2 COPY THIS TOO ↑↑↑↑↑↑↑ (ATTENTION : Please verify the shading model used by your shader at the begining of the line here over. Use metaPass_Standard If your shading model is "Standard", or metaPass_StandardSpecular if your shading model is "StandardSpecular")
		#pragma target 4.5

		struct Input
		{
			float2 uv_MainTex;
			float4 screenPos; // Needed for Aura 2
		};

//↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓
//	AURA 2 : COPY THIS BLOCK AFTER THE Input STRUCT	(ATTENTION THE Input STRUCT NEEDS "screenPos")	↓↓
//↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓
		///-------------------------------------------------------------------------------------------
		///			Aura 2 variants
		///-------------------------------------------------------------------------------------------
		#pragma multi_compile _ AURA
		#pragma multi_compile _ AURA_USE_DITHERING
		#pragma multi_compile _ AURA_USE_CUBIC_FILTERING
		#pragma multi_compile _ AURA_DISPLAY_VOLUMETRIC_LIGHTING_ONLY
		///-------------------------------------------------------------------------------------------
		///			Aura 2 surface shaders specific include
		///-------------------------------------------------------------------------------------------
		#if defined(AURA)
			#include "Assets/Aura 2/Core/Code/Shaders/Includes/AuraSurface.cginc"
		#endif // AURA
	
		///-------------------------------------------------------------------------------------------
		///			Apply Aura 2 after surface shader has been rendered
		///-------------------------------------------------------------------------------------------
		void metaPass_Standard(Input IN, SurfaceOutputStandard o, inout fixed4 color)
		{
			#if defined(AURA)
			Aura2_MetaPass_Standard(IN, o, color); // ATTENTION Input struct needs "screenPos"
			#endif // AURA
		}
		
		void metaPass_StandardSpecular(Input IN, SurfaceOutputStandardSpecular o, inout fixed4 color)
		{
			#if defined(AURA)
			Aura2_MetaPass_StandardSpecular(IN, o, color); // ATTENTION Input struct needs "screenPos"
			#endif // AURA
		}
//↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
//	END OF AURA 2 BLOCK																				↑↑
//↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑

		sampler2D _MainTex;
		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
 
		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}	
}