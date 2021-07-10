using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Generates a vector field using presets to achieve different patterns. The mode property is used to control the pattern to use for the vector field.
Currently these patterns are implemented:
- Direction: generates an uniform vector field with a direction
- Circular: generates a vector field rotating around the middle of the texture
- Stripes: generates alternated stripes of direction vectors
- Turbulence: perlin noise based turbulence vector field.

This node works great in combination with the fluid simulation nodes.
")]

	[System.Serializable, NodeMenuItem("Procedural/Vector Field")]
	public class VectorField : FixedShaderNode
	{
		public override string name => "Vector Field";

		public override string shaderName => "Hidden/Mixture/VectorField";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{ "_Mode", "_Direction", "_PointInwards", "_StripeCount", "_Randomness", "_Seed", "_Frequency", "_ScrollDirection"};
	}
}