using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Branch"), NodeMenuItem("If")]
	public class Branch : MixtureNode
	{
		[Input]
		public object input;

        [Input]
        public bool condition;

        [Output]
        public object output;

        [SerializeField]
        SerializableType inputType = new SerializableType(typeof(object));

		public override string	name => "Branch";

		public override bool    hasPreview => false;
		public override bool	showDefaultInspector => true;

		protected override void Enable()
		{
		}

		[CustomPortBehavior(nameof(input))]
		public IEnumerable< PortData > InputPortType(List< SerializableEdge > edges)
		{
            if (edges.Count == 1)
                inputType.type = edges[0].outputPort.portData.displayType;

            yield return new PortData
            {
                identifier = nameof(input),
                displayName = "Input",
                acceptMultipleEdges = true,
                displayType = inputType.type,
            };
		}

		[CustomPortBehavior(nameof(output))]
		public IEnumerable< PortData > OutputPortType(List< SerializableEdge > edges)
		{
            yield return new PortData
            {
                identifier = nameof(output),
                displayName = "Output",
                acceptMultipleEdges = true,
                displayType = inputType.type,
            };
		}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
            // TODO
			return true;
		}
    }
}
