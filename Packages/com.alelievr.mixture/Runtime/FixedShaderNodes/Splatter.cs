using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Textures/Splatter")]
	public class Splatter : FixedShaderNode
	{
		public override string name => "Splatter";

		public override string shaderName => "Hidden/Mixture/Splatter";

		public override bool displayMaterialInspector => true;

		protected override bool ProcessNode()
		{
			if (!base.ProcessNode())
				return false;
            
            // TODO: splat a texture x times in the texture
            // Make a custom buffer with texture rects, rotations and randomization and then bind it to the material
			
			return true;
		}
	}
}