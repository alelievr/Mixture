using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine.Rendering;

namespace Mixture
{
	[Documentation(@"
The cross section node allow you to generate 2D texture by taking either a slice of a texture 2D or 3D.
Right now this node is limited to slices on the Y axis. 
")]
	[System.Serializable, NodeMenuItem("Utils/Cross Section")]
	public class CrossSection : FixedShaderNode
	{
		public override string name => "Cross Section";

		public override string shaderName => "Hidden/Mixture/CrossSection";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{};

        protected override TextureDimension GetTempTextureDimension() => TextureDimension.Tex2D;
	}
}