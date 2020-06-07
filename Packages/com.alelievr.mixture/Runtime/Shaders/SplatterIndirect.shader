Shader "Hidden/Mixture/Splatter"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[InlineTexture]_Source_2D("Source", 2D) = "white" {}
		[InlineTexture]_Source_3D("Source", 3D) = "white" {}
		[InlineTexture]_Source_Cube("Source", Cube) = "white" {}

		_SourceCrop("Source Crop", Vector) = (0, 0, 0, 0)

		[Enum(Grid, 0, Random, 1, R2, 2, Halton, 3, FibonacciSpiral, 4)] _Sequence("Sequence", Float) = 0

		// Sequence parameters Sequance
		[RangeDrawer]_SplatDensity("Splat Density", Range(1, 8)) = 4

		[Enum(Blend, 0, Add, 1, Sub, 2, Max, 3, Min, 4)]_Operator("Operator", Float) = 0

		[Enum(Fixed, 0, Range, 1, TowardsCenter, 2)] _RotationMode("Rotation Mode", Float) = 0

		// Settings controled by the UI code:
		[HideInInspector] _JitterDistance("Jitter Distance", Float) = 0
	}

	CGINCLUDE
	
	#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureUtils.cginc"
	#include "Packages/com.alelievr.mixture/Runtime/Shaders/Splatter.hlsl"

	TEXTURE_SAMPLER_X(_Source);
	float4 _SourceCrop;
    StructuredBuffer<SplatPoint> _SplatPoints;

	// We only need vertex pos and uv for splat
	struct VertexInput
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
		uint instanceID : SV_InstanceID;
		uint vertexID : SV_VertexID;
	};

	struct FramegentInput
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
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

    FramegentInput IndirectVertex(VertexInput i)
    {
		SplatPoint p = _SplatPoints[i.instanceID];
		FramegentInput o;

		o.uv = uvs[i.vertexID % 6];
		float3 vertex2d = float3(o.uv * 2 - 1, 0) * p.scale;

		float3 rotated = vertex2d;
		rotated = rotate_vertex_position(rotated, float3(1, 0, 0), p.rotation.x);
		rotated = rotate_vertex_position(rotated, float3(0, 1, 0), p.rotation.y);
		rotated = rotate_vertex_position(rotated, float3(0, 0, 1), p.rotation.z);

		o.vertex = float4(rotated + p.position, 1);

		return o;
    }

	float4 Fragment(FramegentInput i) : SV_Target
	{
		return float4(i.uv, 0, 1);
	}

	ENDCG

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Name "Splatter"

			Cull Off
			ZTest Always
			ZClip Off

			CGPROGRAM
				#pragma target 3.0
				// The list of defines that will be active when processing the node with a certain dimension
				#pragma shader_feature CRT_2D
				#pragma vertex IndirectVertex
				#pragma fragment Fragment
			ENDCG
		}
	}
}
