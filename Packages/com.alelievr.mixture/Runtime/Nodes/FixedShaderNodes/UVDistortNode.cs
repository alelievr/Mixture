using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
    [Documentation(@"
Apply a distortion to an UV texture. The distortion map must be encoded as vectors and doesn't have to be normalized.

If fact this node just adds an UV to the distoriton texture value after applying the scale and bias to it.
")]

	[System.Serializable, NodeMenuItem("Operators/UV Distort")]
	public class UVDistortNode : FixedShaderNode
	{
		public override string name => "UV Distort";

		public override string shaderName => "Hidden/Mixture/UVDistort";

		public override bool displayMaterialInspector => true;

        public override bool hasSettings => true;

		protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd))
                return false;

            bool useCustomUV = material.HasTextureBound("_UV", rtSettings.GetTextureDimension(graph));
            material.SetKeywordEnabled("USE_CUSTOM_UV", useCustomUV);
            return true;
        }
    }
}