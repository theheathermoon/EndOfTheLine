
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

#ifndef AURA2_TEXTURE_SAMPLING
#define AURA2_TEXTURE_SAMPLING

// Sampler declaration
#if defined(SHADER_STAGE_COMPUTE)
	#define TEXTURE3D_SAMPLER_DECLARATION Texture3D
#else
	#define TEXTURE3D_SAMPLER_DECLARATION sampler3D
#endif

#if defined(SHADER_STAGE_COMPUTE)
#define SAMPLE_TEX3D_LINEAR(tex, texCoords) tex.SampleLevel(_LinearClamp, texCoords, 0)
#elif defined(SHADER_STAGE_FRAGMENT)
#define SAMPLE_TEX3D_LINEAR(tex, texCoords) tex3D(tex, texCoords)
#else
#define SAMPLE_TEX3D_LINEAR(tex, texCoords) tex3Dlod(tex, float4(texCoords, 0.0f))
#endif

// Linear sampling
inline FP4 SampleTexture3D_Linear(TEXTURE3D_SAMPLER_DECLARATION tex, float3 texCoords)
{
    return SAMPLE_TEX3D_LINEAR(tex, texCoords);
}

// Cubic Texture3D Sampling
// Based on : http://www.dannyruijters.nl/cubicinterpolation/
inline FP4 SampleTexture3D_Cubic(TEXTURE3D_SAMPLER_DECLARATION tex, float3 texCoords, float3 texelSize)
{
    texCoords /= texelSize;

	// shift the coordinate from [0,extent] to [-0.5, extent-0.5]
    FP3 coord_grid = texCoords - 0.5f;
    FP3 index = floor(coord_grid);
    FP3 fraction = coord_grid - index;
    FP3 one_frac = 1.0f - fraction;
    FP3 squared = fraction * fraction;
    FP3 one_sqd = one_frac * one_frac;
    
	// bspline convolution weights, without conditional statements
    FP3 w0 = 1.0f / 6.0f * one_sqd * one_frac;
    FP3 w1 = 2.0f / 3.0f - 0.5f * squared * (2.0f - fraction);
    FP3 w2 = 2.0f / 3.0f - 0.5f * one_sqd * (2.0f - one_frac);
    FP3 w3 = 1.0f / 6.0f * squared * fraction;

    FP3 g0 = w0 + w1;
    FP3 g1 = w2 + w3;
    FP3 h0 = (w1 / g0) - 0.5f + index; //h0 = w1/g0 - 1, move from [-0.5, extent-0.5] to [0, extent]
    FP3 h1 = (w3 / g1) + 1.5f + index; //h1 = w3/g1 + 1, move from [-0.5, extent-0.5] to [0, extent]

	// fetch the eight linear interpolations
	// weighting and fetching is interleaved for performance and stability reasons
	// TODO : USE LERP
    FP4 tex000 = SAMPLE_TEX3D_LINEAR(tex, FP3(h0.x, h0.y, h0.z) * texelSize);
    FP4 tex100 = SAMPLE_TEX3D_LINEAR(tex, FP3(h1.x, h0.y, h0.z) * texelSize);
    tex000 = g0.x * tex000 + g1.x * tex100; //weigh along the x-direction
    FP4 tex010 = SAMPLE_TEX3D_LINEAR(tex, FP3(h0.x, h1.y, h0.z) * texelSize);
    FP4 tex110 = SAMPLE_TEX3D_LINEAR(tex, FP3(h1.x, h1.y, h0.z) * texelSize);
    tex010 = g0.x * tex010 + g1.x * tex110; //weigh along the x-direction
    tex000 = g0.y * tex000 + g1.y * tex010; //weigh along the y-direction
    FP4 tex001 = SAMPLE_TEX3D_LINEAR(tex, FP3(h0.x, h0.y, h1.z) * texelSize);
    FP4 tex101 = SAMPLE_TEX3D_LINEAR(tex, FP3(h1.x, h0.y, h1.z) * texelSize);
    tex001 = g0.x * tex001 + g1.x * tex101; //weigh along the x-direction
    FP4 tex011 = SAMPLE_TEX3D_LINEAR(tex, FP3(h0.x, h1.y, h1.z) * texelSize);
    FP4 tex111 = SAMPLE_TEX3D_LINEAR(tex, FP3(h1.x, h1.y, h1.z) * texelSize);
    tex011 = g0.x * tex011 + g1.x * tex111; //weigh along the x-direction
    tex001 = g0.y * tex001 + g1.y * tex011; //weigh along the y-direction
    return g0.z * tex000 + g1.z * tex001;	//weigh along the z-direction
}

// Generic texture sampling function
inline float4 SampleTexture3D(TEXTURE3D_SAMPLER_DECLARATION tex, float3 texCoords, float3 texelSize)
{
	#if defined(AURA_USE_CUBIC_FILTERING)
		return SampleTexture3D_Cubic(tex, texCoords, texelSize);
	#else
		return SAMPLE_TEX3D_LINEAR(tex, texCoords);
	#endif // AURA_USE_CUBIC_FILTERING
}

#endif //  AURA2_TEXTURE_SAMPLING