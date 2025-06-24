// Effect applies normalmapped lighting to a 2D sprite.

float4 LightIntensity = 1.0;
int NumberOfLights;
float3 LightPosition[5]; // Max number of lights.
float LightSize[5];
float4 AmbientColor = 1;

sampler TextureSampler : register(s0);
sampler NormalSampler : register(s1);


float4 main(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
	//texCoord coordinates are returned between values 0 to 1.
    // Look up the texture and normalmap values.
    float4 tex = tex2D(TextureSampler, texCoord);

	if (NumberOfLights > 0)
	{
		float3 normal = tex2D(NormalSampler, texCoord);

		float3 pixelPosition = float3(1320 * texCoord.x, 720 * texCoord.y, 0);
		float lightAmount = 0;

		for (int i = 0; i < NumberOfLights; i++)
		{
			float internalAmount = max(dot(normal, normalize(LightPosition[i] - pixelPosition)), 0);
			if (internalAmount > lightAmount)
				lightAmount = internalAmount;
		}	

		color.rgb *= 0 + lightAmount * LightIntensity;
	}
    return tex * color;
}

float4 AmbientNormalCombination(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
   //texCoord coordinates are returned between values 0 to 1.
    float4 tex = tex2D(TextureSampler, texCoord);

	if (NumberOfLights > 0)
	{
		float3 normal = tex2D(NormalSampler, texCoord);

		float3 pixelPosition = float3(1320 * texCoord.x, 720 * texCoord.y, 0);
		float lightAmount = 0;

		for (int i = 0; i < NumberOfLights; i++)
		{
			float internalAmount = max(dot(normal, normalize(LightPosition[i] - pixelPosition)), 0);
			if (internalAmount > lightAmount)
				lightAmount = internalAmount;
		}	

		if ((lightAmount < 1 && lightAmount > 0.7f)) // Change this second value to shrink the circle.
			color = (color * (1 - AmbientColor.a)) + ((AmbientColor + (lightAmount * LightIntensity)) * AmbientColor.a);
		else
			color.rgb *= 0 + lightAmount * LightIntensity * 2;
	}
    return tex * color;
}

technique AmbientNormalmap
{
    pass Pass1
    {
        //PixelShader = compile ps_3_0 main();
        PixelShader = compile ps_3_0 AmbientNormalCombination();

    }
}
