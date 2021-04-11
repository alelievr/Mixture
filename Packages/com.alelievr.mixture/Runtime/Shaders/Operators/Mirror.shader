Shader "Hidden/Mixture/Mirror"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[InlineTexture]_Source_2D("Source", 2D) = "white" {}
		[InlineTexture]_Source_3D("Source", 3D) = "white" {}
		[InlineTexture]_Source_Cube("Source", Cube) = "white" {}

		// Other parameters
		[Enum(Mirror X, 0, Mirror Y, 1, Mirror Z, 2, Mirror Corner, 3)]_Mode("Mode", Float) = 0
		[VisibleIf(_Mode, 3)][Enum(Top Left, 0, Top Right, 1, Bottom Left, 2, Bottom Right, 3)]_CornerType("Corner", Float) = 0
		[VisibleIf(_Mode, 3)][Enum(Back, 0, Front, 1)]_CornerZPosition("Corner Depth_3D", Float) = 0
		[VisibleIf(_Mode, 0, 1, 2)]_Offset("Offset", Range(0, 1)) = 0.5
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
			TEXTURE_SAMPLER_X(_Source);
			float _Mode;
			float _CornerType;
			float _Offset;
			float _CornerZPosition;

			float3 MirrorMask(float3 uv, bool3 mask)
			{
				uv.x = (uv.x <= _Offset || !mask.x) ? abs(uv.x) : _Offset - (uv.x - _Offset);
				uv.y = (uv.y <= _Offset || !mask.y) ? abs(uv.y) : _Offset - (uv.y - _Offset);
				uv.z = (uv.z <= _Offset || !mask.z) ? abs(uv.z) : _Offset - (uv.z - _Offset);

				return uv;
			}

			float3 MirrorCorner(float3 uv)
			{
				_Offset = 0.5;

				switch (_CornerType)
				{
					default:
					case 0: // Top Left
						uv.x = (uv.x <= _Offset) ? uv.x : 1 - uv.x;
						uv.y = (uv.y <= _Offset) ? 1 - uv.y : uv.y;
						break;
					case 1: // Top Right 
						uv.x = (uv.x <= _Offset) ? 1 - uv.x : uv.x;
						uv.y = (uv.y <= _Offset) ? 1 - uv.y : uv.y;
						break;
					case 2: // Bottom Left
						uv.x = (uv.x <= _Offset) ? uv.x : 1 - uv.x;
						uv.y = (uv.y <= _Offset) ? uv.y : 1 - uv.y;
						break;
					case 3: // Bottom Right 
						uv.x = (uv.x <= _Offset) ? 1 - uv.x : uv.x;
						uv.y = (uv.y <= _Offset) ? uv.y : 1 - uv.y;
						break;
				}

				switch (_CornerZPosition)
				{
					default:
					case 0: // Front
						uv.z = (uv.z <= _Offset) ? 1 - uv.z : uv.z;
						break;
					case 1: // Back
						uv.z = (uv.z <= _Offset) ? uv.z : 1 - uv.z;
						break;
				}
				return uv;
			}

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				float3 uv = i.localTexcoord.xyz;
				float3 dir = i.direction;

				// 2D and 3D textures
				switch (_Mode)
				{
					case 0: // X
						uv = MirrorMask(uv, bool3(1, 0, 0));
						break;
					case 1: // Y
						uv = MirrorMask(uv, bool3(0, 1, 0));
						break;
					case 2: // Z
						uv = MirrorMask(uv, bool3(0, 0, 1));
						break;
					case 3: // Corner
						uv = MirrorCorner(uv);
						break;
				}

				// Cubemaps
				// TODO: adjust the algorithms to correctly handle vectors
				switch (_Mode)
				{
					case 0: // X
						dir = MirrorMask(dir, bool3(1, 0, 0));
						break;
					case 1: // Y
						dir = MirrorMask(dir, bool3(0, 1, 0));
						break;
					case 2: // Z
						dir = MirrorMask(dir, bool3(0, 0, 1));
						break;
					case 3: // Corner
						dir = MirrorCorner(dir);
						break;
				}

				return SAMPLE_X(_Source, uv, dir);
			}
			ENDHLSL
		}
	}
}
