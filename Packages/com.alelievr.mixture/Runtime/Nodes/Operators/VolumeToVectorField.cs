using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Operators/Volume To Vector Field"), NodeMenuItem("Operators/Volume Gradient")]
	public class VolumeToVectorField : FixedShaderNode
	{
		public override string name => "Volume To Vector Field";

		public override string shaderName => "Hidden/Mixture/VolumeToVectorField";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{};

        protected override MixtureSettings defaultSettings => base.Get3DOnlyRTSettings(base.defaultSettings);

		public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			OutputDimension.Texture3D,
		};
	}
}