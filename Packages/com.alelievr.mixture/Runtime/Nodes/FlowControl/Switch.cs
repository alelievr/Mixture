using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Switch")]
	public class Switch : MixtureNode, IConditional
	{
		[Input]
		public List<object> inputs;

        [Input]
        public int index;

        [Output]
        public object output;

        [SerializeField]
        SerializableType inputType = new SerializableType(typeof(object));

		public override string	name => "Switch";

		public override bool    hasPreview => false;
		public override bool	showDefaultInspector => true;
		
		public override float 	nodeWidth => MixtureUtils.smallNodeWidth;

		protected override void Enable()
		{
		}

		[CustomPortBehavior(nameof(inputs))]
		public IEnumerable< PortData > InputPortType(List< SerializableEdge > edges)
		{
			var data = MixtureUtils.UpdateInputPortType(ref inputType, "Input", edges);
            data.acceptMultipleEdges = true;
            yield return data;
		}

		[CustomPortInput(nameof(inputs), typeof(object))]
		void AssignComputeInputs(List< SerializableEdge > edges)
		{
			// TODO
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

        public string GetExecutedBranch()
        {
            throw new NotImplementedException();
        }
    }
}
