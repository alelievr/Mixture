using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;
using GraphProcessor;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Utils/Sprite Sheet To Volume")]
	public class SpriteSheetToVolume : FixedShaderNode
	{
		public override string name => "Sprite Sheet To Volume";

		public override string shaderName => "Hidden/Mixture/SpriteSheetToVolume";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{};

        protected override TextureDimension GetTempTextureDimension() => TextureDimension.Tex3D;

		protected override MixtureSettings defaultRTSettings => Get3DOnlyRTSettings(base.defaultRTSettings);

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			float sliceCount = material.GetFloat("_SliceCount");

			if (sliceCount > 0)
			{
				rtSettings.depthMode = OutputSizeMode.Fixed;
				rtSettings.sliceCount = (int)sliceCount;
			}

			return base.ProcessNode(cmd);
		}
	}
}