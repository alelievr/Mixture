using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Tube")]
	public class TubeNode : MixtureNode
	{
		[Output("Tube")]
		public MixtureMesh output;

		public override string	name => "Tube";

		public override bool hasPreview => true;
		public override bool showDefaultInspector => true;

		private Mesh mesh;
		public override Texture previewTexture => !MixtureGraphProcessor.isProcessing ? UnityEditor.AssetPreview.GetAssetPreview(mesh) ?? Texture2D.blackTexture : Texture2D.blackTexture;

		// Tube Parameters
		public float height = 1f;
		public int nbSides = 24;
		public float radius1 = .5f;
		public float radius2 = .15f;


		// Transform Parameters
		public Vector3 scale = Vector3.one;
		// There is an issue with json serialization and new keyword :) 
		public Vector3 bug_position = Vector3.zero;
		// TODO: rotation

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			mesh = new Mesh{ indexFormat = IndexFormat.UInt32};

			// Outter shell is at radius1 + radius2 / 2, inner shell at radius1 - radius2 / 2
			float bottomRadius1 = radius1; // ability to have a different radius on the top and the bottom. // disable by the UI for now
			float bottomRadius2 = radius2;
			float topRadius1 = radius1;
			float topRadius2 = radius2;

			int nbVerticesCap = nbSides * 2 + 2;
			int nbVerticesSides = nbSides * 2 + 2;
			#region Vertices

			// bottom + top + sides
			Vector3[] vertices = new Vector3[nbVerticesCap * 2 + nbVerticesSides * 2];
			int vert = 0;
			float _2pi = Mathf.PI * 2f;

			// Bottom cap
			int sideCounter = 0;
			while (vert < nbVerticesCap)
			{
				sideCounter = sideCounter == nbSides ? 0 : sideCounter;

				float r1 = (float)(sideCounter++) / nbSides * _2pi;
				float cos = Mathf.Cos(r1);
				float sin = Mathf.Sin(r1);
				vertices[vert] = new Vector3(cos * (bottomRadius1 - bottomRadius2 * .5f), 0f, sin * (bottomRadius1 - bottomRadius2 * .5f));
				vertices[vert + 1] = new Vector3(cos * (bottomRadius1 + bottomRadius2 * .5f), 0f, sin * (bottomRadius1 + bottomRadius2 * .5f));
				vert += 2;
			}

			// Top cap
			sideCounter = 0;
			while (vert < nbVerticesCap * 2)
			{
				sideCounter = sideCounter == nbSides ? 0 : sideCounter;

				float r1 = (float)(sideCounter++) / nbSides * _2pi;
				float cos = Mathf.Cos(r1);
				float sin = Mathf.Sin(r1);
				vertices[vert] = new Vector3(cos * (topRadius1 - topRadius2 * .5f), height, sin * (topRadius1 - topRadius2 * .5f));
				vertices[vert + 1] = new Vector3(cos * (topRadius1 + topRadius2 * .5f), height, sin * (topRadius1 + topRadius2 * .5f));
				vert += 2;
			}

			// Sides (out)
			sideCounter = 0;
			while (vert < nbVerticesCap * 2 + nbVerticesSides)
			{
				sideCounter = sideCounter == nbSides ? 0 : sideCounter;

				float r1 = (float)(sideCounter++) / nbSides * _2pi;
				float cos = Mathf.Cos(r1);
				float sin = Mathf.Sin(r1);

				vertices[vert] = new Vector3(cos * (topRadius1 + topRadius2 * .5f), height, sin * (topRadius1 + topRadius2 * .5f));
				vertices[vert + 1] = new Vector3(cos * (bottomRadius1 + bottomRadius2 * .5f), 0, sin * (bottomRadius1 + bottomRadius2 * .5f));
				vert += 2;
			}

			// Sides (in)
			sideCounter = 0;
			while (vert < vertices.Length)
			{
				sideCounter = sideCounter == nbSides ? 0 : sideCounter;

				float r1 = (float)(sideCounter++) / nbSides * _2pi;
				float cos = Mathf.Cos(r1);
				float sin = Mathf.Sin(r1);

				vertices[vert] = new Vector3(cos * (topRadius1 - topRadius2 * .5f), height, sin * (topRadius1 - topRadius2 * .5f));
				vertices[vert + 1] = new Vector3(cos * (bottomRadius1 - bottomRadius2 * .5f), 0, sin * (bottomRadius1 - bottomRadius2 * .5f));
				vert += 2;
			}
			#endregion

			#region Normales

			// bottom + top + sides
			Vector3[] normales = new Vector3[vertices.Length];
			vert = 0;

			// Bottom cap
			while (vert < nbVerticesCap)
			{
				normales[vert++] = Vector3.down;
			}

			// Top cap
			while (vert < nbVerticesCap * 2)
			{
				normales[vert++] = Vector3.up;
			}

			// Sides (out)
			sideCounter = 0;
			while (vert < nbVerticesCap * 2 + nbVerticesSides)
			{
				sideCounter = sideCounter == nbSides ? 0 : sideCounter;

				float r1 = (float)(sideCounter++) / nbSides * _2pi;

				normales[vert] = new Vector3(Mathf.Cos(r1), 0f, Mathf.Sin(r1));
				normales[vert + 1] = normales[vert];
				vert += 2;
			}

			// Sides (in)
			sideCounter = 0;
			while (vert < vertices.Length)
			{
				sideCounter = sideCounter == nbSides ? 0 : sideCounter;

				float r1 = (float)(sideCounter++) / nbSides * _2pi;

				normales[vert] = -(new Vector3(Mathf.Cos(r1), 0f, Mathf.Sin(r1)));
				normales[vert + 1] = normales[vert];
				vert += 2;
			}
			#endregion

			#region UVs
			Vector2[] uvs = new Vector2[vertices.Length];

			vert = 0;
			// Bottom cap
			sideCounter = 0;
			while (vert < nbVerticesCap)
			{
				float t = (float)(sideCounter++) / nbSides;
				uvs[vert++] = new Vector2(0f, t);
				uvs[vert++] = new Vector2(1f, t);
			}

			// Top cap
			sideCounter = 0;
			while (vert < nbVerticesCap * 2)
			{
				float t = (float)(sideCounter++) / nbSides;
				uvs[vert++] = new Vector2(0f, t);
				uvs[vert++] = new Vector2(1f, t);
			}

			// Sides (out)
			sideCounter = 0;
			while (vert < nbVerticesCap * 2 + nbVerticesSides)
			{
				float t = (float)(sideCounter++) / nbSides;
				uvs[vert++] = new Vector2(t, 0f);
				uvs[vert++] = new Vector2(t, 1f);
			}

			// Sides (in)
			sideCounter = 0;
			while (vert < vertices.Length)
			{
				float t = (float)(sideCounter++) / nbSides;
				uvs[vert++] = new Vector2(t, 0f);
				uvs[vert++] = new Vector2(t, 1f);
			}
			#endregion

			#region Triangles
			int nbFace = nbSides * 4;
			int nbTriangles = nbFace * 2;
			int nbIndexes = nbTriangles * 3;
			int[] triangles = new int[nbIndexes];

			// Bottom cap
			int i = 0;
			sideCounter = 0;
			while (sideCounter < nbSides)
			{
				int current = sideCounter * 2;
				int next = sideCounter * 2 + 2;

				triangles[i++] = next + 1;
				triangles[i++] = next;
				triangles[i++] = current;

				triangles[i++] = current + 1;
				triangles[i++] = next + 1;
				triangles[i++] = current;

				sideCounter++;
			}

			// Top cap
			while (sideCounter < nbSides * 2)
			{
				int current = sideCounter * 2 + 2;
				int next = sideCounter * 2 + 4;

				triangles[i++] = current;
				triangles[i++] = next;
				triangles[i++] = next + 1;

				triangles[i++] = current;
				triangles[i++] = next + 1;
				triangles[i++] = current + 1;

				sideCounter++;
			}

			// Sides (out)
			while (sideCounter < nbSides * 3)
			{
				int current = sideCounter * 2 + 4;
				int next = sideCounter * 2 + 6;

				triangles[i++] = current;
				triangles[i++] = next;
				triangles[i++] = next + 1;

				triangles[i++] = current;
				triangles[i++] = next + 1;
				triangles[i++] = current + 1;

				sideCounter++;
			}


			// Sides (in)
			while (sideCounter < nbSides * 4)
			{
				int current = sideCounter * 2 + 6;
				int next = sideCounter * 2 + 8;

				triangles[i++] = next + 1;
				triangles[i++] = next;
				triangles[i++] = current;

				triangles[i++] = current + 1;
				triangles[i++] = next + 1;
				triangles[i++] = current;

				sideCounter++;
			}
			#endregion

			mesh.vertices = vertices;
			mesh.normals = normales;
			mesh.uv = uvs;
			mesh.triangles = triangles;

			mesh.RecalculateBounds();
			mesh.Optimize();

			output = new MixtureMesh { mesh = mesh, localToWorld = Matrix4x4.TRS(bug_position, Quaternion.identity, scale) };

			return true;

		}
    }
}