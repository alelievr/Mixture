#if MIXTURE_EXPERIMENTAL
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using GraphProcessor;
using System.Collections.Generic;
using System.IO;
using System;

namespace Mixture
{
	[NodeCustomEditor(typeof(BakedGeometry))]
	public class bakedGeometryView : MixtureNodeView
	{
		BakedGeometry	bakedGeometry => nodeTarget as BakedGeometry;

		ObjectField		debugCustomRenderTextureField;
		ObjectField		meshField;

		public override void Enable()
		{
			base.Enable();


			meshField = new ObjectField
			{
				value = bakedGeometry.mesh,
				objectType = typeof(Mesh),
			};

			nodeTarget.onProcessed += () =>
			{
				// if (bakedGeometry.mesh != null && !owner.graph.IsObjectInGraph(bakedGeometry.mesh))
				// {
				// 	owner.graph.AddObjectToGraph(bakedGeometry.mesh);
				// }
			};

			// meshField.RegisterValueChangedCallback((v) => {
			// 	// Duplicate mesh before assigning it:
			// 	var m = (Mesh)v.newValue;
			// 	var mesh = new Mesh();
			// 	mesh.vertices = m.vertices;
			// 	mesh.normals = m.normals;
			// 	mesh.triangles = m.triangles;
			// 	mesh.uv = m.uv;
			// 	mesh.hideFlags = HideFlags.NotEditable;
			// 	mesh.UploadMeshData(false);
			// 	mesh.RecalculateBounds();
			// 	bakedGeometry.mesh = mesh;
			// });

			controlsContainer.Add(meshField);
		}

		public override void OnRemoved() => owner.graph.RemoveObjectFromGraph(bakedGeometry.mesh);
	}
}
#endif