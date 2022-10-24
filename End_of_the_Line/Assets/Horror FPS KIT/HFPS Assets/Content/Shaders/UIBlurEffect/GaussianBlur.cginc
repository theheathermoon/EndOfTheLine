#ifndef GAUSS_BLUR
#define GAUSS_BLUR

half4 GaussianTap9(sampler2D sampleTexture, float2 screenPos, float2 direction, float size) {
	float depth = size * 0.0005;

	float hstep = direction.x;
	float vstep = direction.y;

	half4 sum = half4(0.0h, 0.0h, 0.0h, 0.0h);

	//apply blurring, using a 9-tap filter with predefined gaussian weights
	sum += tex2D(sampleTexture, float2(screenPos.x - 4.0*depth*hstep, screenPos.y - 4.0*depth*vstep)) * 0.0162162162;
	sum += tex2D(sampleTexture, float2(screenPos.x - 3.0*depth*hstep, screenPos.y - 3.0*depth*vstep)) * 0.0540540541;
	sum += tex2D(sampleTexture, float2(screenPos.x - 2.0*depth*hstep, screenPos.y - 2.0*depth*vstep)) * 0.1216216216;
	sum += tex2D(sampleTexture, float2(screenPos.x - 1.0*depth*hstep, screenPos.y - 1.0*depth*vstep)) * 0.1945945946;

	sum += tex2D(sampleTexture, screenPos) * 0.2270270270;

	sum += tex2D(sampleTexture, float2(screenPos.x + 1.0*depth*hstep, screenPos.y + 1.0*depth*vstep)) * 0.1945945946;
	sum += tex2D(sampleTexture, float2(screenPos.x + 2.0*depth*hstep, screenPos.y + 2.0*depth*vstep)) * 0.1216216216;
	sum += tex2D(sampleTexture, float2(screenPos.x + 3.0*depth*hstep, screenPos.y + 3.0*depth*vstep)) * 0.0540540541;
	sum += tex2D(sampleTexture, float2(screenPos.x + 4.0*depth*hstep, screenPos.y + 4.0*depth*vstep)) * 0.0162162162;
	
	return sum;
}

#endif