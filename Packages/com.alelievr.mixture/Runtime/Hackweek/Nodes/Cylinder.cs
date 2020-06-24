using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Cylinder")]
	public class CylinderNode : MixtureNode
	{
		[Output("Cylinder")]
		public MixtureMesh output;

		public override string	name => "Cylinder";

		public override bool hasPreview => true;
		public override bool showDefaultInspector => true;
		public override Texture previewTexture => UnityEditor.AssetPreview.GetAssetPreview(mesh) ?? Texture2D.blackTexture;

		private Mesh mesh;

		// Cylinder Parameters
		public float Radius = .5f;
		public float Height = 1f;
		public int Sides = 24;
		public bool Smooth = true;

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			Sides = Mathf.Max(Sides, 3);
			float _2pi = Mathf.PI * 2f;

			//Meshes
			Mesh botCapMesh = new Mesh();
			Mesh topCapMesh = new Mesh();
			Mesh sideMesh = new Mesh();

			int nbVertices = Sides;
			int nbTriangles = (Sides - 2) * 3;

			Vector3[] vertices = new Vector3[nbVertices];
			Vector3[] normals = new Vector3[nbVertices];
			Vector2[] uvs = new Vector2[nbVertices];
			int[] triangles = new int[nbTriangles];

			// Bot Caps -----------------------------------------

			float capHeight = 0;
			float dir = 1;
			Vector3 normal = Vector3.down;

			// Verts
			for (int i = 0; i < Sides; i++)
			{
				int index = i;
				float angle = (float)(i) / Sides * _2pi * dir;
				float cos = Mathf.Cos(angle);
				float sin = Mathf.Sin(angle);
				Vector3 point = new Vector3(cos * Radius, capHeight, sin * Radius);
				Vector2 uv =  new Vector2((cos * 0.5f)+0.5f, (sin * 0.5f) + 0.5f);
				uv *= new Vector2(1, -1);
				vertices[index] = point;
				normals[index] = normal;
				uvs[index] = uv;
			}

			// Triangles
			for (int i = 0; i < (Sides - 2); i++)
			{

				int triIndex = i * 3;
				int vertIndex = i;
				triangles[triIndex] = 0;
				triangles[triIndex+1] = i + 1;
				triangles[triIndex+2] = i + 2;
			}
			

			botCapMesh.vertices = vertices;
			botCapMesh.normals = normals;
			botCapMesh.uv = uvs;
			botCapMesh.triangles = triangles;

			// Top Caps -----------------------------------------

			capHeight = Height;
			dir = -1;
			normal = Vector3.up;

			// Verts
			for (int i = 0; i < Sides; i++)
			{
				int index = i;
				float angle = (float)(i) / Sides * _2pi * dir;
				float cos = Mathf.Cos(angle);
				float sin = Mathf.Sin(angle);
				Vector3 point = new Vector3(cos * Radius, capHeight, sin * Radius);
				Vector2 uv = new Vector2((cos * 0.5f) + 0.5f, (sin * 0.5f) + 0.5f);
				vertices[index] = point;
				normals[index] = normal;
				uvs[index] = uv;
			}

			// Triangles
			for (int i = 0; i < (Sides - 2); i++)
			{

				int triIndex = i * 3;
				triangles[triIndex] = 0;
				triangles[triIndex + 1] = i + 1;
				triangles[triIndex + 2] = i + 2;
			}


			topCapMesh.vertices = vertices;
			topCapMesh.normals = normals;
			topCapMesh.uv = uvs;
			topCapMesh.triangles = triangles;


			// Sides -----------------------------------------

			nbVertices = Smooth ? Sides * 2 + 2: Sides * 4;
			nbTriangles = Sides * 2 * 3;

			vertices = new Vector3[nbVertices];
			normals = new Vector3[nbVertices];
			uvs = new Vector2[nbVertices];
			triangles = new int[nbTriangles];

			// Verts
			if (Smooth)
			{
				for (int i = 0; i < Sides; i++)
				{
					int index = i * 2;
					float angle01 = (float)(i) / Sides;
					float cos = Mathf.Cos(angle01 * _2pi);
					float sin = Mathf.Sin(angle01 * _2pi);
					Vector3 point1 = new Vector3(cos * Radius, 0, sin * Radius);
					Vector3 point2 = new Vector3(cos * Radius, capHeight, sin * Radius);
					normal = new Vector3(cos, 0, sin);
					Vector2 uv1 = new Vector2(angle01, 0);
					Vector2 uv2 = new Vector2(angle01, 1);

					vertices[index] = point1;
					vertices[index+1] = point2;
					normals[index] = normal;
					normals[index+1] = normal;
					uvs[index] = uv1;
					uvs[index + 1] = uv2;
					if (i == 0)
					{
						index = Sides * 2;
						vertices[index] = point1;
						vertices[index + 1] = point2;
						normals[index] = normal;
						normals[index + 1] = normal;
						uvs[index] = new Vector2(1, 0);
						uvs[index + 1] = new Vector2(1, 1);
					}
				}

				// Triangles
				for (int i = 0; i < Sides; i++)
				{
					int triIndex = i * 6;
					int vertIndex = i * 2;

					triangles[triIndex] = vertIndex;
					triangles[triIndex + 1] = vertIndex + 1;
					triangles[triIndex + 2] = vertIndex + 2;

					triangles[triIndex + 3] = vertIndex + 1;
					triangles[triIndex + 4] = vertIndex + 3;
					triangles[triIndex + 5] = vertIndex + 2;
				}
			}
			else
			{
				for (int i = 0; i < Sides; i++)
				{
					int index = i * 4;
					float angle01_1 = (float)(i) / Sides;
					float angle01_2 = ((float)(i+1)) / Sides;
					float cos = Mathf.Cos(angle01_1 *_2pi);
					float sin = Mathf.Sin(angle01_1 * _2pi);
					float cos2 = Mathf.Cos(angle01_2 * _2pi);
					float sin2 = Mathf.Sin(angle01_2 * _2pi);
					Vector3 point1 = new Vector3(cos * Radius, 0, sin * Radius);
					Vector3 point2 = new Vector3(cos * Radius, capHeight, sin * Radius);
					Vector3 point3 = new Vector3(cos2 * Radius, 0, sin2 * Radius);
					Vector3 point4 = new Vector3(cos2 * Radius, capHeight, sin2 * Radius);
					normal = new Vector3((cos + cos2) * 0.5f, 0, (sin + sin2) * 0.5f);
					Vector2 uv1 = new Vector2(angle01_1, 0);
					Vector2 uv2 = new Vector2(angle01_1, 1);
					Vector2 uv3 = new Vector2(angle01_2, 0);
					Vector2 uv4 = new Vector2(angle01_2, 1);

					vertices[index] = point1;
					vertices[index + 1] = point2;
					vertices[index + 2] = point3;
					vertices[index + 3] = point4;
					normals[index] = normal;
					normals[index + 1] = normal;
					normals[index + 2] = normal;
					normals[index + 3] = normal;
					uvs[index] = uv1;
					uvs[index + 1] = uv2;
					uvs[index + 2] = uv3;
					uvs[index + 3] = uv4;
				}

				// Triangles
				for (int i = 0; i < Sides; i++)
				{
					int triIndex = i * 6;
					int vertIndex = i * 4;

					triangles[triIndex] = vertIndex;
					triangles[triIndex + 1] = vertIndex + 1;
					triangles[triIndex + 2] = vertIndex + 2;

					triangles[triIndex + 3] = vertIndex + 1;
					triangles[triIndex + 4] = vertIndex + 3;
					triangles[triIndex + 5] = vertIndex + 2;
				}
			}

			sideMesh.vertices = vertices;
			sideMesh.normals = normals;
			sideMesh.uv = uvs;
			sideMesh.triangles = triangles;

			// Combine -----------------------------------------

			mesh = new Mesh();
			CombineInstance[] combine = new CombineInstance[3];
			combine[0].mesh = botCapMesh;
			combine[0].transform = Matrix4x4.identity;
			combine[1].mesh = topCapMesh;
			combine[1].transform = Matrix4x4.identity;
			combine[2].mesh = sideMesh;
			combine[2].transform = Matrix4x4.identity;
			mesh.CombineMeshes(combine);
			mesh.RecalculateBounds();
			mesh.Optimize();

			botCapMesh.Clear();
			topCapMesh.Clear();
			sideMesh.Clear();

			output = new MixtureMesh { mesh = mesh, localToWorld = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one) };

			return true;

		}
    }
}