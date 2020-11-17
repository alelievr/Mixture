using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;


namespace Mixture
{
	[Documentation(@"
Apply a transformation on the input texture. This node allows you to offset, scale and rotate the input texture based on either another texture or a constant.

Note that the values from the rotation map will be converted to euler angles in the node so that 1 means 360 degree. 
")]

	[System.Serializable, NodeMenuItem("Custom/Transform")]
	public class Transform : FixedShaderNode
	{
		public override string name => "Transform";

		public override string shaderName => "Hidden/Mixture/Transform";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{};
	}
}