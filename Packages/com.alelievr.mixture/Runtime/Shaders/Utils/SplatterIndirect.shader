Shader "Hidden/Mixture/Splatter"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[InlineTexture]_Source_2D("Source", 2D) = "white" {}
		[InlineTexture]_Source_3D("Source", 3D) = "white" {}
		[InlineTexture]_Source_Cube("Source", Cube) = "white" {}

		[Enum(Grid, 0, Random, 1, R2, 2, Halton, 3, FibonacciSpiral, 4)] _Sequence("Sequence", Float) = 0

		// Sequence parameters Sequance
		[RangeDrawer]_SplatDensity("Splat Density", Range(1, 8)) = 4

		[Enum(Blend, 0, Add, 1, Sub, 2, Max, 3, Min, 4)]_Operator("Operator", Float) = 0

		[Enum(Fixed, 0, Range, 1, TowardsCenter, 2)] _RotationMode("Rotation Mode", Float) = 0

		// Settings controled by the UI code:
		[HideInInspector] _JitterDistance("Jitter Distance", Float) = 0
	}

	HLSLINCLUDE

	#pragma shader_feature CRT_2D CRT_3D
	#pragma target 4.5
	
	#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureUtils.hlsl"
	#include "Packages/com.alelievr.mixture/Runtime/Shaders/Splatter.hlsl"
	#include "Packages/com.alelievr.mixture/Runtime/Shaders/NoiseUtils.hlsl"

	#line 33

	TEXTURE_X(_Source0);
	TEXTURE_X(_Source1);
	TEXTURE_X(_Source2);
	TEXTURE_X(_Source3);
	TEXTURE_X(_Source4);
	TEXTURE_X(_Source5);
	TEXTURE_X(_Source6);
	TEXTURE_X(_Source7);
	TEXTURE_X(_Source8);
	TEXTURE_X(_Source9);
	TEXTURE_X(_Source10);
	TEXTURE_X(_Source11);
	TEXTURE_X(_Source12);
	TEXTURE_X(_Source13);
	TEXTURE_X(_Source14);
	TEXTURE_X(_Source15);
	uint _TextureCount;
    StructuredBuffer<SplatPoint> _SplatPoints;

// Blend
	float _SrcBlend;
	float _DstBlend;
	float _BlendOp;

	// Output channels
	float _ChannelModeR;
	float _ChannelModeG;
	float _ChannelModeB;
	float _ChannelModeA;

	// Mode
	float _Mode;

	float _Seed;

	// We only need vertex pos and uv for splat
	struct VertexInput
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
		uint instanceID : SV_InstanceID;
		uint vertexID : SV_VertexID;
	};

	struct FragmentInput
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
		uint id : SV_InstanceID;
	};

	struct FragmentOutput
	{
		float4 color : SV_Target;
		float depth : SV_Depth;
	};

	#define QUATERNION_IDENTITY float4(0, 0, 0, 1)

	float4 quat_from_axis_angle(float3 axis, float angle)
	{ 
		float4 qr;
		float half_angle = (angle * 0.5) * 3.14159 / 180.0;
		qr.x = axis.x * sin(half_angle);
		qr.y = axis.y * sin(half_angle);
		qr.z = axis.z * sin(half_angle);
		qr.w = cos(half_angle);
		return qr;
	}

	float3 rotate_vertex_position(float3 position, float3 axis, float angle)
	{ 
		float4 q = quat_from_axis_angle(axis, angle);
		float3 v = position.xyz;
		return v + 2.0 * cross(q.xyz, cross(q.xyz, v) + q.w * v);
	}

	static float2 uvs[6] = {float2(0, 0), float2(1, 1), float2(0, 1), float2(0, 0), float2(1, 0), float2(1, 1)};

    FragmentInput IndirectVertex(VertexInput i)
    {
		SplatPoint p = _SplatPoints[i.instanceID];
		FragmentInput o;

		o.uv = uvs[i.vertexID % 6];
		float3 vertex2d = float3(o.uv * 2 - 1, 0) * p.scale;

		float3 rotated = vertex2d;
		rotated = rotate_vertex_position(rotated, float3(1, 0, 0), p.rotation.x);
		rotated = rotate_vertex_position(rotated, float3(0, 1, 0), p.rotation.y);
		rotated = rotate_vertex_position(rotated, float3(0, 0, 1), p.rotation.z);

		o.vertex = float4(rotated + p.position, 1);
		o.id = p.id;
		o.uv.y = 1 - o.uv.y;

		return o;
    }

	#define SAMPLE_RANDOM(id, uv) SAMPLE_X_LINEAR_CLAMP(_Source##id, uv, uv)

	float4 SampleRandomTexture(uint id, float3 uv)
	{
		uint r = WhiteNoise(id * 562 + _Seed) * _TextureCount;

		switch (r % _TextureCount)
		{
			case 0: return SAMPLE_RANDOM(0, uv);
			case 1: return SAMPLE_RANDOM(1, uv);
			case 2: return SAMPLE_RANDOM(2, uv);
			case 3: return SAMPLE_RANDOM(3, uv);
			case 4: return SAMPLE_RANDOM(4, uv);
			case 5: return SAMPLE_RANDOM(5, uv);
			case 6: return SAMPLE_RANDOM(6, uv);
			case 7: return SAMPLE_RANDOM(7, uv);
			case 8: return SAMPLE_RANDOM(8, uv);
			case 9: return SAMPLE_RANDOM(9, uv);
			case 10: return SAMPLE_RANDOM(10, uv);
			case 11: return SAMPLE_RANDOM(11, uv);
			case 12: return SAMPLE_RANDOM(12, uv);
			case 13: return SAMPLE_RANDOM(13, uv);
			case 14: return SAMPLE_RANDOM(14, uv);
			case 15: return SAMPLE_RANDOM(15, uv);
			default: return 0;
		}
	}

	float GetRandomFloat(int id)
	{
		float p = float(id * 42 + 69);
		// Source: https://www.shadertoy.com/view/4dS3Wd
		p = frac(p * 0.011); p *= p + 7.5; p *= p + p;
		return frac(p);
	}

	float ComputeOutputChannel(float2 uv, float4 value, float channelMode, int id)
	{
		switch (channelMode)
		{
			default:
			case 0: // InputR
				return value.r;
			case 1: // InputG
				return value.g;
			case 2: // InputB
				return value.b;
			case 3: // InputA
				return value.a;
			case 4: // UV X
				return uv.x * (value.a > 0);
			case 5: // UV Y
				return uv.y * (value.a > 0);
			case 6: // Random Uniform Color
				return GetRandomFloat(id) * (value.a > 0);
		}
	}

	float4 ComputeOutputChannels(float2 uv, float4 value, int id)
	{
		return float4(
			ComputeOutputChannel(uv, value, _ChannelModeR, id),
			ComputeOutputChannel(uv, value, _ChannelModeG, id),
			ComputeOutputChannel(uv, value, _ChannelModeB, id),
			ComputeOutputChannel(uv, value, _ChannelModeA, id)
		);
	}

	FragmentOutput Fragment(FragmentInput i) 
	{
		FragmentOutput output;

		float4 value = SampleRandomTexture(i.id, float3(i.uv, 0));
		output.color = ComputeOutputChannels(i.uv.xy, value, i.id);

		if (_Mode == 1) // Depth tile mode is enabled
		{
			// In depth tile mode, the depth from the input tile is always stored in the alpha channel.
			float depth = output.color.b;
			clip(depth - 0.000000001); // We discard every height <= 0 pixels

			depth = (depth > 0) ? depth + i.vertex.z : -1e10;
			output.color.b = depth;

			// Remap the depth between 0 and 1 to have keep valid z-testing
			output.depth = (output.color.b + MAX_DEPTH_VALUE) / (2 * MAX_DEPTH_VALUE);
		}
		else
		{
			output.depth = 0;
		}

		return output;
	}

	ENDHLSL

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Name "Splatter"

			Cull Off
			ZTest [_ZTest] 
			ZClip Off
			Blend [_SrcBlend] [_DstBlend], One [_DstBlend] 
			BlendOp [_BlendOp]

			HLSLPROGRAM
				// The list of defines that will be active when processing the node with a certain dimension
				#pragma vertex IndirectVertex
				#pragma fragment Fragment
			ENDHLSL
		}
	}
}
