using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
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
            
			var uvPort = inputPorts.Find(p => p.portData.identifier.Contains("_UV_"));
            if (uvPort == null)
                return false;
            
            material.SetKeywordEnabled("USE_CUSTOM_UV", uvPort.GetEdges().Count != 0);
            return true;
        }

    }
}