using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("For Start")]
	public class ForStart : MixtureNode, ILoopStart
	{
		[Input]
		public object input;

		[Input("Count")]
		public int inputCount = 4;

        [Output]
        public object output;

		[System.NonSerialized]
		[Output("Index")]
		public int index = 0;

		[Output("Count")]
		public int outputCount = 0;

        [HideInInspector, SerializeField]
        internal SerializableType inputType = new SerializableType(typeof(object));

		public override string	name => "For Start";

		public override bool    hasPreview => false;
		public override bool    hasSettings => false;
		public override bool	showDefaultInspector => true;
		public override float	nodeWidth => MixtureUtils.smallNodeWidth;

		[CustomPortBehavior(nameof(input))]
		public IEnumerable< PortData > InputPortType(List< SerializableEdge > edges)
		{
			yield return MixtureUtils.UpdateInputPortType(ref inputType, "Input", edges);
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

		public HashSet<BaseNode> GatherNodesInLoop()
		{
			Stack<BaseNode> l = new Stack<BaseNode>();
			HashSet<BaseNode> h = new HashSet<BaseNode>();
			
			l.Push(this);

			while (l.Count > 0)
			{
				var node = l.Pop();

				if (h.Contains(node))
					continue;
				
				if (!(node is ForeachStart))
					foreach (var i in node.GetInputNodes())
						l.Push(i);

				if (!(node is ForeachEnd))
					foreach (var o in node.GetOutputNodes())
						l.Push(o);

				h.Add(node);
			}

			return h;
		}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			index++;
			return true;
		}

		public bool IsLastIteration() => index == inputCount || inputCount <= 0;

        public void PrepareLoopStart()
        {
			index = 0;
            outputCount = inputCount;
			output = input;
        }
    }
}
