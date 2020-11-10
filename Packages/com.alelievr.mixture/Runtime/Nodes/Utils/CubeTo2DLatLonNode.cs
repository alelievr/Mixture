using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
    [Documentation(@"
Transform a cubemap into a 2D texture using the LatLong convertion function.
")]

    [System.Serializable, NodeMenuItem("Utils/Cube to 2D LatLon")]
    public class CubeTo2DLatLonNode : FixedShaderNode
    {
        public override string name => "Cube to 2D LatLon";

        public override string shaderName => "Hidden/Mixture/CubeTo2DLatLon";

        public override bool displayMaterialInspector => true;

		protected override MixtureRTSettings defaultRTSettings => Get2DOnlyRTSettings(base.defaultRTSettings);
    }
}