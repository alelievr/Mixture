using UnityEngine;
using GraphProcessor;
using System.Collections.Generic;

namespace Mixture
{
    [Documentation(@"
Generates procedural shapes based on the Superformula (Gielis formula).
Works in both 2D and 3D:
- 2D: Creates flat shapes with radial symmetry (flowers, stars, etc.)
- 3D: Generates volumetric shapes by combining two superformulas

Parameters:
- N1, N2, N3: Control shape symmetry and complexity
- M: Number of repetitions/sides
- A, B: Shape scale along axes
- Scale: Overall shape size
- Rotation: Shape orientation
- Inside/Outside Colors: Shape coloring
- Line Frequency/Definition: Control line pattern appearance

Implementation based on https://www.shadertoy.com/view/NdXBWB
")]
    [System.Serializable, NodeMenuItem("Procedural/SuperShape")]
    public class SuperShapeNode : FixedShaderNode
    {
        public override string name => "SuperShape";

        public override string shaderName => "Hidden/Mixture/SuperShape";

        public override bool displayMaterialInspector => true;

        protected override IEnumerable<string> filteredOutProperties => new string[]{};
    }
}
