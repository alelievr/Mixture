using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using GraphProcessor;
using System;
using UnityEngine.Experimental.Rendering;

namespace Mixture
{
	[Documentation(@"
The self node holds a copy of the output node last processing texture.
When the node is executed for the first time, the initialization color is used instead of the output texture.

Currently only the first output texture of the output node can be retrieved.
")]

	[System.Serializable, NodeMenuItem("Utils/Self")]
	public class SelfNode : MixtureNode 
	{
		[Output(name = "Out"), Tooltip("Output Texture"), NonSerialized]
		public CustomRenderTexture	output = null;

		[Input, ShowAsDrawer]
		public Texture			initialTexture;

		[Input, ShowAsDrawer]
		public Color			initialColor = Color.white;

		public override Texture previewTexture => output;
		public override bool	hasSettings => false;
		public override bool	showDefaultInspector => true;
		public override string	name => "Self";

        [NonSerialized]
		bool					initialization = true;

		protected override void Enable()
		{
			base.Enable();

			initialization = true;

			// Update output rt:
			if (output == null)
			{
				output = new CustomRenderTexture(1, 1, GraphicsFormat.R16G16B16A16_SFloat);
				output.enableRandomWrite = true;
				output.hideFlags = HideFlags.HideAndDontSave;
			}
		}

        protected override void Disable()
		{
			base.Disable();
			initialization = false;
			CoreUtils.Destroy(output);
		}

		[IsCompatibleWithGraph]
		static bool IsCompatibleWithRealtimeGraph(BaseGraph graph)
			=> (graph as MixtureGraph).type == MixtureGraphType.Realtime;

		public void ResetOutputTexture() => initialization = true;

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (output == null)
				return false;

			var sourceTarget = graph.outputNode.mainOutput.finalCopyRT;

			// We force the initialization if the graph texture have been destroyed (c++ cleanup for example)
			if (sourceTarget == null || !sourceTarget.IsCreated())
				initialization = true;

			var dim = settings.GetResolvedTextureDimension(graph);

			if (UpdateTempRenderTexture(ref output, hasMips: graph.mainOutputTexture.mipmapCount > 1))
				initialization = true;

			if (initialization)
			{
				var initTexture = initialTexture == null ? TextureUtils.GetWhiteTexture(settings.GetResolvedTextureDimension(graph)) : initialTexture;
				output.material = GetTempMaterial("Hidden/Mixture/SelfInitialization");
				output.material.SetColor("_InitializationColor", initialColor);
				MixtureUtils.SetTextureWithDimension(output.material, "_InitializationTexture", initTexture);
				output.Update();
				CustomTextureManager.UpdateCustomRenderTexture(cmd, output);
				initialization = false;
			}
			else
			{
				if (graph.outputNode.outputTextureSettings.Count > 0)
				{
					// We don't take the result of the output node because we want to avoid the sRRB conversion;
					var graphOutputTexture = graph.outputNode.outputTextureSettings[0].inputTexture;
					if (graphOutputTexture != null)
						TextureUtils.CopyTexture(cmd, graphOutputTexture, output, true);
				}
			}

			return true;
		}
	}
}