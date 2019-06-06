using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Experimental.Rendering;
using UnityEditor;
using System;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("CustomTexture")]
	public class CustomTextureNode : MixtureNode
	{
		[Input(name = "Init")]
		public List< object >		initMaterialInputs;

		[Input(name = "Update")]
		public List< object >		updateMaterialInputs;

		[Output(name = "Out")]
		public Texture				output;

		[SerializeField, HideInInspector]
		public CustomRenderTexture	customTexture { get; private set; }

		public Material				initializationMaterial { get; private set; }
		public Material				updateMaterial { get; private set; }

		static readonly string		defaultCRTInitShader = "Hidden/DefaultCRTInitialization";
		static readonly string		defaultCRTUpdateShader = "Hidden/DefaultCRTUpdate";

		public override string		name => "CustomTexture";

		#region Ports control

		[CustomPortBehavior(nameof(initMaterialInputs))]
		IEnumerable< PortData > ListMaterialProperties(List< SerializableEdge > edges)
		{
			return GetMaterialPortDatas(initializationMaterial);
		}

		[CustomPortBehavior(nameof(updateMaterialInputs))]
		IEnumerable< PortData > ListMaterialProperties2(List< SerializableEdge > edges)
		{
			return GetMaterialPortDatas(updateMaterial);
		}

		#endregion

		protected override void Enable()
		{
			if (customTexture == null)
			{
				customTexture = new CustomRenderTexture(512, 512, GraphicsFormat.R8G8B8A8_UNorm);
				initializationMaterial = new Material(Shader.Find(defaultCRTInitShader));
				updateMaterial = new Material(Shader.Find(defaultCRTUpdateShader));

				customTexture.material = updateMaterial;
				customTexture.initializationMaterial = initializationMaterial;

				// Add all objects to the graph asset:
				AddObjectToGraph(customTexture);
				AddObjectToGraph(initializationMaterial);
				AddObjectToGraph(updateMaterial);
			}
		}

		protected override void Process()
		{
			output = customTexture;
		}
	}
}