Shader "Hidden/Mixture/Transform"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[InlineTexture]_Input_2D("Input", 2D) = "white" {}
		[InlineTexture]_Input_3D("Input", 3D) = "white" {}
		[InlineTexture]_Input_Cube("Input", Cube) = "white" {}

		[InlineTexture]_PositionOffset_2D("Position Offset Map", 2D) = "black" {}
		[InlineTexture]_PositionOffset_3D("Position Offset Map", 3D) = "black" {}
		[InlineTexture]_PositionOffset_Cube("Position Offset Map", Cube) = "black" {}

		[Tooltip(The rotation is stored in the X, Y and Z channels. A value of 1 means 360 degree.)][InlineTexture]_Rotation_2D("Rotation Map", 2D) = "black" {}
		[Tooltip(The rotation is stored in the X, Y and Z channels. A value of 1 means 360 degree.)][InlineTexture]_Rotation_3D("Rotation Map", 3D) = "black" {}
		[Tooltip(The rotation is stored in the X, Y and Z channels. A value of 1 means 360 degree.)][InlineTexture]_Rotation_Cube("Rotation Map", Cube) = "black" {}

		[InlineTexture]_Scale_2D("Scale Map", 2D) = "white" {}
		[InlineTexture]_Scale_3D("Scale Map", 3D) = "white" {}
		[InlineTexture]_Scale_Cube("Scale Map", Cube) = "white" {}

		// Other parameters
		[MixtureVector3]_PositionOffset("Position Offset", Vector) = (0, 0, 0, 0)
		[Tooltip(Rotation in euler angles between 0 and 360)][MixtureVector3]_Rotation("Rotation", Vector) = (0, 0, 0, 0)
		[MixtureVector3]_Scale("Scale", Vector) = (1, 1, 1, 1)
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
            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE

			// This macro will declare a version for each dimention (2D, 3D and Cube)
			TEXTURE_SAMPLER_X(_Input);
			TEXTURE_SAMPLER_X(_PositionOffset);
			TEXTURE_SAMPLER_X(_Rotation);
			TEXTURE_SAMPLER_X(_Scale);

			float3 _PositionOffset;
			float3 _Rotation;
			float3 _Scale;

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				float3 uv = GetDefaultUVs(i);

				// Compute the rotation matrices
				float3 rotation = SAMPLE_X(_Rotation, i.localTexcoord.xyz, i.direction).xyz * 360 + _Rotation;
				rotation = rotation * (PI / 180); // deg to rad


				float4x4 mx = rotationMatrix(float3(1, 0, 0), rotation.x);
				float4x4 my = rotationMatrix(float3(0, 1, 0), rotation.y);
				float4x4 mz = rotationMatrix(float3(0, 0, 1), rotation.z);

#if defined(CRT_2D) || defined(CRT_3D)

				uv.xyz = uv.xyz * 2 - 1;

				// Position
				uv.xyz += SAMPLE_X(_PositionOffset, i.localTexcoord.xyz, i.direction).xyz + _PositionOffset;

				// Scale
				uv.xyz *= SAMPLE_X(_Scale, i.localTexcoord.xyz, i.direction).xyz * _Scale;

				// Rotation
				uv.xyz = mul(mz, mul(my, mul(mx, float4(uv.xyz, 0)))).xyz;

				uv.xyz = uv.xyz * 0.5 + 0.5;
#else

				// For cubemaps right now, we only handle the rotation
				uv.xyz = mul(mul(mul(float4(uv.xyz, 1), mx), my), mz).xyz;

#endif

				return SAMPLE_X(_Input, uv, uv);
			}
			ENDHLSL
		}
	}
}
