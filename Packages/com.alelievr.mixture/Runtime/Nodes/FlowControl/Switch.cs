using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Switch"), NodeMenuItem("Select")]
	public class Switch : MixtureNode
	{
		[Input]
		public List<object> inputs = new List<object>();

        [Input]
        public int index;

        [Output]
        public object output;

        [HideInInspector, SerializeField]
        SerializableType inputType = new SerializableType(typeof(object));

		public override string	name => "Switch";

		public override bool    hasPreview => false;
		public override bool	showDefaultInspector => true;
		
		public override float 	nodeWidth => MixtureUtils.smallNodeWidth;

		[CustomPortBehavior(nameof(inputs))]
		public IEnumerable< PortData > InputPortType(List< SerializableEdge > edges)
		{
			var data = MixtureUtils.UpdateInputPortType(ref inputType, "Input", edges);
            data.acceptMultipleEdges = true;
            yield return data;
		}

		[CustomPortInput(nameof(inputs), typeof(object))]
		void AssignSwitchInputs(List< SerializableEdge > edges)
		{
			inputs = edges.Select(e => e.passThroughBuffer).ToList();
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
			if (inputs == null || index < 0 || index >= inputs.Count)
				return false;

			output = inputs[index];
			return true;
		}
    }
}
