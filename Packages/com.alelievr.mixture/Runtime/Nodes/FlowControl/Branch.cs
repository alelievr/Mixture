using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;

namespace Mixture
{
	[Documentation(@"
Conditionally outputs either the true of false value depending on the condition value.
")]

	[System.Serializable, NodeMenuItem("Flow/Branch"), NodeMenuItem("Flow/If")]
	public class Branch : MixtureNode
	{
        [Input]
        public object inputTrue;

        [Input]
        public object inputFalse;

        [Input]
        public bool condition;

		[Output]
		public object output;

        [HideInInspector, SerializeField]
        SerializableType inputType = new SerializableType(typeof(object));

		public override string	name => "Branch";

		public override bool    hasPreview => false;
		public override bool	showDefaultInspector => true;

		[CustomPortBehavior(nameof(inputTrue))]
		public IEnumerable< PortData > InputPortTypeTrue(List< SerializableEdge > edges)
		{
			yield return MixtureUtils.UpdateInputPortType(ref inputType, "True", edges);
		}

		[CustomPortBehavior(nameof(inputFalse))]
		public IEnumerable< PortData > InputPortTypeFalse(List< SerializableEdge > edges)
		{
			yield return MixtureUtils.UpdateInputPortType(ref inputType, "False", edges);
		}

		[CustomPortBehavior(nameof(output))]
		public IEnumerable< PortData > OutputTruePortType(List< SerializableEdge > edges)
		{
            yield return new PortData
            {
                identifier = nameof(inputTrue),
                displayName = "Result",
                acceptMultipleEdges = true,
                displayType = inputType.type,
            };
		}
        
		protected override bool ProcessNode(CommandBuffer cmd)
		{
            output = condition ? inputTrue : inputFalse;
                
			return true;
		}
    }
}
