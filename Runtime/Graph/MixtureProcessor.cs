using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Jobs;
using Unity.Collections;
using GraphProcessor;

namespace Mixture
{
	public class MixtureProcessor : BaseGraphProcessor
	{
		List< BaseNode >		processList;
		new MixtureGraph		graph => base.graph as MixtureGraph;

		public MixtureProcessor(BaseGraph graph) : base(graph) {}

		public override void UpdateComputeOrder()
		{
			processList = graph.nodes.OrderBy(n => n.computeOrder).ToList();
		}

		public override void Run()
		{
			int count = processList.Count;

			// The process of the mixture graph will update all CRTs,
			// assign their materials and set local material values
			for (int i = 0; i < count; i++)
			{
				processList[i].OnProcess();
			}

			graph.outputNode.tempRenderTexture.Update();
		}
	}
}