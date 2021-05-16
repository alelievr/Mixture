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
	public class SelfNode : ComputeShaderNode 
	{
		[Output(name = "Out"), Tooltip("Output Texture"), NonSerialized]
		public RenderTexture	output = null;

		[Input, ShowAsDrawer]
		public Color			initialColor;

		public override Texture previewTexture => output;
		public override bool	hasSettings => false;
		public override bool	showDefaultInspector => true;
		public override string			name => "Self";

        protected override string computeShaderResourcePath => "Mixture/SelfInitialization";

        [NonSerialized]
		bool					initialization = true;

		protected override void Enable()
		{
			base.Enable();

			initialization = true;

			// Update output rt:
			if (output == null)
			{
				output = new RenderTexture(1, 1, 0, GraphicsFormat.R16G16B16A16_SFloat);
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

		// [CustomPortBehavior(nameof(output))]
		// protected IEnumerable< PortData > ChangeOutputPortType(List< SerializableEdge > edges)
		// {
		// 	yield return new PortData{
		// 		displayName = "output",
		// 		displayType = TextureUtils.GetTypeFromDimension(rtSettings.GetTextureDimension(graph)),
		// 		identifier = "output",
		// 		acceptMultipleEdges = true,
		// 	};
		// }

		public void ResetOutputTexture() => initialization = true;

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (output == null)
				return false;

			var sourceTarget = graph.outputNode.mainOutput.finalCopyRT;

			// We force the initialization if the graph texture have been destroyed (c++ cleanup for example)
			if (sourceTarget == null || !sourceTarget.IsCreated())
				initialization = true;

			var dim = settings.GetTextureDimension(graph);

			if (output.width != settings.GetWidth(graph) || output.dimension != settings.GetTextureDimension(graph))
			{
				output.Release();
				output.width = settings.GetWidth(graph);
				output.height = settings.GetHeight(graph);
				output.volumeDepth = settings.GetDepth(graph);
				output.graphicsFormat = settings.GetGraphicsFormat(graph);
				output.enableRandomWrite = true;
				output.Create();
				initialization = true;
			}

			// TODO: support mip maps

			if (initialization)
			{
				cmd.SetComputeVectorParam(computeShader, "_ClearColor", initialColor);
				// We can't clear a cubemap from a compute shader :(
				switch (dim)
				{
					case TextureDimension.Cube:
						cmd.SetRenderTarget(output);
						break;
					case TextureDimension.Tex2D:
						cmd.SetComputeTextureParam(computeShader, 0, MixtureUtils.texture2DPrefix, output);
						DispatchCompute(cmd, 0, output.width, output.height);
						break;
					case TextureDimension.Tex3D:
						cmd.SetComputeTextureParam(computeShader, 0, MixtureUtils.texture3DPrefix, output);
						DispatchCompute(cmd, 1, output.width, output.height, output.volumeDepth);
						break;
				}
				initialization = false;
			}
			else
			{
				cmd.CopyTexture(graph.outputNode.mainOutput.finalCopyRT, output);
			}

			return true;
		}
	}
}