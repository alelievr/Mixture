using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using GraphProcessor;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Simulation/2D Fluid")]
	public class Fluid2DNode : ComputeShaderNode 
	{
		[Input]
		public Texture density;

		[Input]
		public Texture velocity;
		
		[Output]
		public Texture output;

		// [Output]
		// public Texture vectorField;
		// [Output]
		// public Texture outputDensity;

		public override string name => "2D Fluid";

		protected override string computeShaderResourcePath => "Mixture/Fluid2D";

		public override bool showDefaultInspector => true;
		public override Texture previewTexture => output;

		protected override MixtureSettings defaultSettings => Get2DOnlyRTSettings(base.defaultSettings);

		public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			OutputDimension.Texture2D,
		};

		// For now only available in realtime mixtures, we'll see later for static with a spritesheet mode maybe
		[IsCompatibleWithGraph]
		static bool IsCompatibleWithRealtimeGraph(BaseGraph graph)
			=> (graph as MixtureGraph).type == MixtureGraphType.Realtime;

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (!base.ProcessNode(cmd))
				return false;
			
			return true;
		}
	}
}