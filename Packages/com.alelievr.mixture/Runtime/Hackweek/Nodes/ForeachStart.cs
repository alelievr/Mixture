using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;
using Net3dBool;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Foreach Start")]
	public class ForeachStart : MixtureNode
	{
		public enum ForeachType
		{
			Attribute,
			Vertex,
			Face,
			Object,
			Pixel
		}

		[Input("Input Attributes", allowMultiple: false)]
		public MixtureAttributeList inputs = new MixtureAttributeList();

        [Output("Output")]
        public MixtureAttribute output;

		[System.NonSerialized]
		[Output("Index")]
		public int index = 0;

		[System.NonSerialized]
		[Output("Count")]
		public int count = 0;

		public override string	name => "Foreach Start";

		public override bool    hasPreview => false;
		public override bool	showDefaultInspector => true;

		// TODO :p 
		public ForeachType type;

		protected override void Enable()
		{
		}

		// Functions with Attributes must be either protected or public otherwise they can't be accessed by the reflection code
		// [CustomPortBehavior(nameof(inputMeshes))]
		// public IEnumerable< PortData > ListMaterialProperties(List< SerializableEdge > edges)
		// {
        //     yield return new PortData
        //     {
        //         identifier = nameof(inputMeshes),
        //         displayName = "Input Meshes",
        //         allowMultiple = true,
        //         displayType()
        //     };
		// }

		// [CustomPortInput(nameof(inputMeshes), typeof(MixtureMesh))]
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
			index++;
			if (inputs == null || inputs.Count == 0 || index >= inputs.Count || index < 0)
				return false;

			output = inputs[index];

			return true;
		}

		public int PrepareNewIteration()
		{
			index = -1;
			if (inputs == null)
				return 0;

			count = inputs.Count;
			return inputs.Count;
		}

		public bool IsLastIteration()
		{
			return index == inputs.Count || inputs.Count == 0;
		}
    }
}