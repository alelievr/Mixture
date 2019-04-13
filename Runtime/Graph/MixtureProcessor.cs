using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Jobs;
using Unity.Collections;
using GraphProcessor;

public class ProcessGraphProcessor : BaseGraphProcessor
{
	List< BaseNode >		processList;
	
	public ProcessGraphProcessor(BaseGraph graph) : base(graph) {}

	public override void UpdateComputeOrder()
	{
		processList = graph.nodes.OrderBy(n => n.computeOrder).ToList();
	}

	public override void Run()
	{
		int count = processList.Count;

		for (int i = 0; i < count; i++)
		{
			processList[i].OnProcess();
		}
	}
}
