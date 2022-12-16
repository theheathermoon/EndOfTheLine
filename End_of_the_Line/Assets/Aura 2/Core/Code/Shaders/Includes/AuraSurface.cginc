
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

#ifndef AURA2_SURFACE_SHADER
#define AURA2_SURFACE_SHADER

#include "Assets/Aura 2/Core/Code/Shaders/Includes/CommonDefines.cginc"
#include "Assets/Aura 2/Core/Code/Shaders/Includes/CommonVariables.cginc"
#include "Assets/Aura 2/Core/Code/Shaders/Includes/TextureSampling.cginc"
#include "Assets/Aura 2/Core/Code/Shaders/Includes/Structs.cginc"
#include "Assets/Aura 2/Core/Code/Shaders/Includes/CommonFunctions.cginc"
#if defined(AURA_USE_DITHERING)
	#include "Assets/Aura 2/Core/Code/Shaders/Includes/BlueNoise.cginc"
#endif // AURA_USE_DITHERING
#define AURA2_COMMON
#define AURA2_BLUE_NOISE
#include "Assets/Aura 2/Core/Code/Shaders/Includes/AuraUsage.cginc"

void Aura2_ApplyFog_Surface(inout FP4 color, FP4 screenPos)
{
	#if defined(AURA)
		half3 screenSpacePosition = screenPos.xyz / screenPos.w;
		screenSpacePosition.z = GetLinearEyeDepth(screenSpacePosition.z);
	
		//// Debug fog only
	#if defined(AURA_DISPLAY_VOLUMETRIC_LIGHTING_ONLY)
		color.xyz = float3(0.0f, 0.0f, 0.0f);
	#endif // AURA_DISPLAY_VOLUMETRIC_LIGHTING_ONLY
	
		Aura2_ApplyFog(color, screenSpacePosition);
	#endif // AURA
}

void Aura2_MetaPass_Standard(Input IN, SurfaceOutputStandard o, inout fixed4 color)
{
	Aura2_ApplyFog_Surface(color, IN.screenPos); // ATTENTION Input struct needs "screenPos"
}

void Aura2_MetaPass_StandardSpecular(Input IN, SurfaceOutputStandardSpecular o, inout fixed4 color)
{
	Aura2_ApplyFog_Surface(color, IN.screenPos); // ATTENTION Input struct needs "screenPos"
}

#endif // AURA2_SURFACE_SHADER