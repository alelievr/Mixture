using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("For Start")]
	public class ForStart : MixtureNode
	{
		[Input]
		public object inputs;

        [Output]
        public object output;

		[System.NonSerialized]
		[Output("Index")]
		public int index = 0;

		[Input("Count")]
		public int inputCount = 0;

		[Output("Count")]
		public int outputCount = 0;

        [SerializeField]
        SerializableType inputType = new SerializableType(typeof(object));

		public override string	name => "For Start";

		public override bool    hasPreview => false;
		public override bool	showDefaultInspector => true;

		protected override void Enable()
		{
		}

		[CustomPortBehavior(nameof(inputs))]
		public IEnumerable< PortData > InputPortType(List< SerializableEdge > edges)
		{
            if (edges.Count > 0)
                inputType.type = edges[0].outputPort.portData.displayType ?? edges[0].outputPort.fieldInfo.FieldType;
			else
				inputType.type = typeof(object);

            yield return new PortData
            {
                identifier = nameof(inputs),
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

		// [CustomPortInput(nameof(inputs), typeof(MixtureMesh))]
		// protected void GetMaterialInputs(List< SerializableEdge > edges)
		// {
        //     if (inputMeshes == null)
        //         inputMeshes = new List<MixtureMesh>();
        //     inputMeshes.Clear();
		// 	foreach (var edge in edges)
        //     {
        //         if (edge.passThroughBuffer is MixtureMesh m)
        //             inputMeshes.Add(m);
        //     }
		// }

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
            inputCount = outputCount;
			index++;
			return true;
		}

		public int PrepareNewIteration() => inputCount;

		public bool IsLastIteration() => index == inputCount || inputCount == 0;
    }
}
