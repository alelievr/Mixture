using System.Collections.Generic;
using UnityEngine.Rendering;
using GraphProcessor;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Custom/Volume To Sprite Sheet")]
	public class VolumeToSpriteSheet : FixedShaderNode
	{
		public override string name => "Volume To Sprite Sheet";

		public override string shaderName => "Hidden/Mixture/VolumeToSpriteSheet";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{"_STARTPOSITION"};

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (!base.ProcessNode(cmd))
				return false;
			
			var volume = material.GetTexture("_Volume");

			if (volume != null)
				material.SetFloat("_SliceCount", TextureUtils.GetSliceCount(volume));

			return true;
		}
	}
}