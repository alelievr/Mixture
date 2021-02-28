Shader "Hidden/Mixture/StochasticTiling"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[InlineTexture]_Source("Input", 2D) = "white" {}

        _BorderSize("Border Size", Range(0,0.6)) = 0.2
        _BlendPow("Blend Power", Range(1,15)) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			HLSLPROGRAM
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.hlsl"
            #pragma vertex CustomRenderTextureVertexShader
			#pragma fragment MixtureFragment
			#pragma target 3.0

			// The list of defines that will be active when processing the node with a certain dimension
            #define CRT_2D

			// This macro will declare a version for each dimention(2D, 3D and Cube)
			TEXTURE2D(_Source);
			SAMPLER(sampler_Source);
			float _BorderSize;
			float _BlendPow;

			float RemapValue(float s, float a1, float a2, float b1, float b2)
			{
				return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
			}

			float Hash(float n)
			{
				return frac(sin(n) * 43758.5453123);
			}

			float2 Hash2(float2 n)
			{
				return frac(sin(mul(float2x2(127.1, 311.7, 269.5, 183.3), n)) * 43758.5453);
			}

			float4 SampleMainTex(float2 uv, float2 ddx, float2 ddy)
			{
				return SAMPLE_TEXTURE2D_GRAD(_Source, sampler_Source, uv, ddx, ddy);
			}

			// Source: https://drive.google.com/file/d/1QecekuuyWgw68HU9tg6ENfrCTCVIjm6l/view
			// Modified version made by Thomas Deliot to make a texture tile.
			float4 SampleTextureMakeTileable(float2 uv)
			{
				float4 result = 0;
				// Precompute uv derivatives
				float2 duvdx = ddx(uv);
				float2 duvdy = ddy(uv);
				uv = frac(uv);
				// Apply random texture patches
				float tilesPerSide = floor(1.0 / _BorderSize);
				float tileRadius = 1.0 / tilesPerSide;
				// Display border size in blue
				/*float displayBorder = 0.005;
				if ((uv.x > tileRadius && uv.x < tileRadius + displayBorder || 1.0 - uv.x > tileRadius && 1.0 - uv.x < tileRadius + displayBorder
					|| uv.y > tileRadius && uv.y < tileRadius + displayBorder || 1.0 - uv.y > tileRadius && 1.0 - uv.y < tileRadius + displayBorder)
					&& uv.x > 0.0 && uv.x < 1.0 && uv.y > 0.0 && uv.y < 1.0)
				{
					return float4(0, 0, 1, 1);
				}*/
				// Middle section (no blend)
				if (uv.x > tileRadius && 1.0 - uv.x > tileRadius
					&& uv.y > tileRadius && 1.0 - uv.y > tileRadius)
				{
					result = SampleMainTex(uv, duvdx, duvdy);
				}
				else
				// Border blend
				{
					// Linear interpolation on borders from input to tiles
					float weightCenter = min(RemapValue(uv.x, 0.0, tileRadius, 0.0, 1.0), 1.0); // Left border
					weightCenter *= min(RemapValue(uv.x, 1.0, 1.0 - tileRadius, 0.0, 1.0), 1.0); // Right border
					weightCenter *= min(RemapValue(uv.y, 0.0, tileRadius, 0.0, 1.0), 1.0); // Top border
					weightCenter *= min(RemapValue(uv.y, 1.0, 1.0 - tileRadius, 0.0, 1.0), 1.0); // Bottom border
					float4 sampleCenter = SampleMainTex(uv, duvdx, duvdy);
					// Horizontal borders + Corner
					float4 sample0H = 0.0.rrrr;
					float4 sample1H = 0.0.rrrr;
					float weight0H = 0.0;
					float weight1H = 0.0;
					if (uv.y < tileRadius || uv.y > 1.0 - tileRadius)
					{
						// Deal with up border like it's the bottom border
						float2 tempUV = uv;
						float2 tempUV2 = uv;
						if (tempUV.y > tileRadius)
						{
							tempUV.y = 1.0 - uv.y;
							tempUV2.y = uv.y - 1.0;
						}
						float2 uvTile0 = float2((int)(uv.x / tileRadius) * tileRadius, 0.0);
						float2 offset0 = (1.0.rr - tileRadius.rr * 2.0) * Hash2(uvTile0) + tileRadius.rr;
						float2 uvTile1 = float2((int)(uv.x / tileRadius + 1.0) * tileRadius, 0.0);
						float2 uvTile1bis = uvTile1.x >= 1.0 ? 0.0.rr : uvTile1; // Last tile needs to be identical to first (corner)
						float2 offset1 = (1.0.rr - tileRadius.rr * 2.0) * Hash2(uvTile1bis) + tileRadius.rr;
						weight0H = RemapValue(uv.x, uvTile0.x, uvTile1.x, 1.0, 0.0)
							* RemapValue(tempUV.y, 0.0, tileRadius, 1.0, 0.0);
						sample0H = SampleMainTex(offset0 + (tempUV2 - uvTile0), duvdx, duvdy);
						weight1H = RemapValue(uv.x, uvTile0.x, uvTile1.x, 0.0, 1.0)
							* RemapValue(tempUV.y, 0.0, tileRadius, 1.0, 0.0);
						sample1H = SampleMainTex(offset1 + (tempUV2 - uvTile1), duvdx, duvdy);
					}
					// Vertical borders (exclude corner)
					float4 sample0V = 0.0.rrrr;
					float4 sample1V = 0.0.rrrr;
					float weight0V = 0.0;
					float weight1V = 0.0;
					if (uv.x < tileRadius || uv.x > 1.0 - tileRadius)
					{
						// Deal with right border like it's the left border
						float2 tempUV = uv;
						float2 tempUV2 = uv;
						if (tempUV.x > tileRadius)
						{
							tempUV.x = 1.0 - uv.x;
							tempUV2.x = uv.x - 1.0;
						}
						float2 uvTile0 = float2(0.0, (int)(uv.y / tileRadius) * tileRadius);
						float2 offset0 = (1.0.rr - tileRadius.rr * 2.0) * Hash2(uvTile0) + tileRadius.rr;
						float2 uvTile1 = float2(0.0, (int)(uv.y / tileRadius + 1.0) * tileRadius);
						float2 offset1 = (1.0.rr - tileRadius.rr * 2.0) * Hash2(uvTile1) + tileRadius.rr;
						weight0V = RemapValue(uv.y, uvTile0.y, uvTile1.y, 1.0, 0.0)
							* RemapValue(tempUV.x, 0.0, tileRadius, 1.0, 0.0);
						sample0V = SampleMainTex(offset0 + (tempUV2 - uvTile0), duvdx, duvdy);
						weight1V = RemapValue(uv.y, uvTile0.y, uvTile1.y, 0.0, 1.0)
							* RemapValue(tempUV.x, 0.0, tileRadius, 1.0, 0.0);
						sample1V = SampleMainTex(offset1 + (tempUV2 - uvTile1), duvdx, duvdy);
						// Ignore tile sample if it's the corner tile (already applied above)
						if (uvTile0.y <= 0.0)
							weight0V = 0.0;
						if (uvTile1.y >= 1.0)
							weight1V = 0.0;
					}
					// Sharpen blend zone
					weightCenter = pow(abs(weightCenter), _BlendPow);
					weight0H = pow(abs(weight0H), _BlendPow);
					weight1H = pow(abs(weight1H), _BlendPow);
					weight0V = pow(abs(weight0V), _BlendPow);
					weight1V = pow(abs(weight1V), _BlendPow);
					float totalWeight = weightCenter + weight0H + weight1H + weight0V + weight1V;
					// Final result
					result =
						sampleCenter * (weightCenter / totalWeight)
						+ sample0H * (weight0H / totalWeight)
						+ sample1H * (weight1H / totalWeight)
						+ sample0V * (weight0V / totalWeight)
						+ sample1V * (weight1V / totalWeight);
				}
				return result;
			}

			float4 mixture(v2f_customrendertexture i) : SV_Target
			{
				//stochastic sampling
				return SampleTextureMakeTileable(i.localTexcoord.xy);
			}
			ENDHLSL
		}
	}
}
