using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Experimental.Rendering;
using UnityEditor;
using System;
using UnityEngine.Rendering;

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

		public override Texture previewTexture => output;

		static readonly string		defaultCRTInitShader = "Hidden/DefaultCRTInitialization";
		static readonly string		defaultCRTUpdateShader = "Hidden/DefaultCRTUpdate";

		public override float		nodeWidth => 350f;
		public override string		name => "CustomTexture";

		#region Ports control

		[CustomPortBehavior(nameof(initMaterialInputs))]
		protected IEnumerable< PortData > ListMaterialProperties(List< SerializableEdge > edges)
		{
			return GetMaterialPortDatas(initializationMaterial);
		}

		[CustomPortBehavior(nameof(updateMaterialInputs))]
		protected IEnumerable< PortData > ListMaterialProperties2(List< SerializableEdge > edges)
		{
			return GetMaterialPortDatas(updateMaterial);
		}

		#endregion

		protected override void Enable()
		{
			
			if (customTexture == null)
			{
				customTexture = new CustomRenderTexture(512, 512, GraphicsFormat.R8G8B8A8_UNorm);
				customTexture.name = "Custom Texture Node";
				customTexture.enableRandomWrite = true;
				initializationMaterial = new Material(Shader.Find(defaultCRTInitShader));
				updateMaterial = new Material(Shader.Find(defaultCRTUpdateShader));
				customTexture.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
				initializationMaterial.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
				updateMaterial.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

				customTexture.material = updateMaterial;
				customTexture.initializationMaterial = initializationMaterial;
			}
		}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			output = customTexture;

			customTexture.Update(); // TODO: remove this when the CRT dependency is fixed

			return true;
		}
	}
}