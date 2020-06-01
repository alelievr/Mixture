using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
    [System.Serializable, NodeMenuItem("Normal/Height To Normal")]
    public class HeightToNormalNode : FixedShaderNode
    {
        public override string name => "Height To Normal";

        public override string shaderName => "Hidden/Mixture/HeightToNormal";

        protected override IEnumerable<string> filteredOutProperties => new string[] { "_OutputRange", "_Channel", "_OutputSpace" };

        public override bool displayMaterialInspector => true;

        protected override MixtureRTSettings defaultRTSettings
        {
            get {
                var rts = MixtureRTSettings.defaultValue;
                rts.dimension = OutputDimension.Texture2D;
                return rts;
            }
        }

        public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() { OutputDimension.Texture2D };
    }
}