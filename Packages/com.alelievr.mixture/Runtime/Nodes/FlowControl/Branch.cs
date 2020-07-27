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
        public bool condition;

		[Input]
		public object input;

        [Output]
        public object outputTrue;

        [Output]
        public object outputFalse;

        [SerializeField]
        SerializableType inputType = new SerializableType(typeof(object));

		public override string	name => "Branch";

		public override bool    hasPreview => false;
		public override bool	showDefaultInspector => true;

        bool comparison;

		protected override void Enable()
		{
		}

		[CustomPortBehavior(nameof(input))]
		public IEnumerable< PortData > InputPortType(List< SerializableEdge > edges)
		{
			yield return MixtureUtils.UpdateInputPortType(ref inputType, "Input", edges);
		}

		[CustomPortBehavior(nameof(outputTrue))]
		public IEnumerable< PortData > OutputTruePortType(List< SerializableEdge > edges)
		{
            yield return new PortData
            {
                identifier = nameof(outputTrue),
                displayName = "True",
                acceptMultipleEdges = true,
                displayType = inputType.type,
            };
		}
        
		[CustomPortBehavior(nameof(outputFalse))]
		public IEnumerable< PortData > OutputFalsePortType(List< SerializableEdge > edges)
		{
            yield return new PortData
            {
                identifier = nameof(outputFalse),
                displayName = "False",
                acceptMultipleEdges = true,
                displayType = inputType.type,
            };
		}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
            // TODO
            comparison = true;

			return true;
		}

        public string GetExecutedBranch()
        {
            if (comparison)
                return nameof(outputTrue);
            else
                return nameof(outputFalse);
        }
    }
}
