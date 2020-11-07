using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
		[Documentation(@"
Perform a remap between intput min/max and output min/max values.
")]

	[System.Serializable, NodeMenuItem("Math/Vector Remap")]
	public class VectorRemapNode : MixtureNode
	{
		// TODO: multi VectorRemap port
		public override bool hasSettings => false;
		public override bool showDefaultInspector => true;

		[Input("A"), ShowAsDrawer]
		public Vector4	a;
		
		[Output("Out")]
		public Vector4	o;

		public float	inputMin = -1;
		public float	inputMax = 1;
		public float	outputMin = 0;
		public float	outputMax = 1;

		public override string name => "Remap";

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			Vector4 iMin = MixtureConversions.ConvertFloatToVector4(inputMin);
			Vector4 oMin = MixtureConversions.ConvertFloatToVector4(outputMin);
			o = oMin + (a - iMin) * (outputMax - outputMin) / (inputMax - inputMin);
			return true;
		}
	}
}