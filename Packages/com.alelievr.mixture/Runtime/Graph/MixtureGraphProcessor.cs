using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using GraphProcessor;
using UnityEngine.Rendering;
using System;

namespace Mixture
{
	public class ComputeOrderInfo
	{
		public Dictionary<BaseNode, int> forLoopLevel = new Dictionary<BaseNode, int>();

		public void Clear() => forLoopLevel.Clear();
	}

	public class MixtureGraphProcessor : BaseGraphProcessor
	{
		List<List<BaseNode>>	processLists = new List<List<BaseNode>>();
		new MixtureGraph		graph => base.graph as MixtureGraph;
		HashSet< BaseNode >		executedNodes = new HashSet<BaseNode>();

        // Dictionary<BaseNode, List<BaseNode>> mixtureDependencies = new Dictionary<BaseNode, List<BaseNode>>();

        struct ProcessingScope : IDisposable
        {
			MixtureGraphProcessor processor;

			public ProcessingScope(MixtureGraphProcessor processor)
			{
				this.processor = processor;
				processor.isProcessing++;
			}

            public void Dispose() => processor.isProcessing--;
        }

        internal int isProcessing = 0;

		public MixtureGraphProcessor(BaseGraph graph) : base(graph)
		{
			// TODO: custom CRT sorting in the  CRT manager to manage the case where we have multiple runtime CRTs on the same branch and they need to be updated
			// in the correct order even if their materials/textures are not referenced into their params (they can be connected to a compute shader node for example).
			CustomTextureManager.onBeforeCustomTextureUpdated -= BeforeCustomRenderTextureUpdate;
			CustomTextureManager.onBeforeCustomTextureUpdated += BeforeCustomRenderTextureUpdate;
		}

		public override void UpdateComputeOrder()
		{
			// TODO: Gather dependencies for all the nodes:
			
			processLists.Clear();
			// processLists.Add(lst.Where(n => n.computeOrder > 0).OrderBy(n => n.computeOrder).ToList());

			// TODO: update infos
			info.Clear();
			foreach (var processList in processLists)
			{
				int loopIndex = 0;
				foreach (var node in processList)
				{
					if (node is ForStart fs)
						loopIndex++;
					info.forLoopLevel[node] = loopIndex;
					if (node is ForEnd fe)
						loopIndex--;
				}
			}
		}

		void BeforeCustomRenderTextureUpdate(CommandBuffer cmd, CustomRenderTexture crt)
		{
			if (isProcessing == 0)
			{
				// TODO: cache
				// Trigger the graph processing from a CRT update if we weren't processing
				BaseNode node = graph.nodes.FirstOrDefault(n => n is IUseCustomRenderTextureProcessing i && i.GetCustomRenderTexture() == crt);

				// node can be null if the CRT doesn't belong to the graph.
				if (node != null)
					RunGraphFor(cmd, new List<BaseNode>{ node });
			}
			else
			{
				// We don't do anything
			}
			// if (mixtureDependencies.TryGetValue(crt, out var dependencies))
			// {
			// 	// Update the dependencies of the CRT
			// 	foreach (var nonCRTDep in dependencies)
			// 	{
			// 		// Make sure we don't execute multiple times the same node if there are multiple dependencies that needs it:
			// 		if (executedNodes.Contains(nonCRTDep))
			// 			continue;

			// 		executedNodes.Add(nonCRTDep);
			// 		ProcessNode(cmd, nonCRTDep);
			// 	}
			// }
		}

		public static void AddGPUAndCPUBarrier(CommandBuffer currentCmd)
		{
			Graphics.ExecuteCommandBuffer(currentCmd);
			currentCmd.Clear();
		}

		void RunGraphFor(CommandBuffer cmd, IEnumerable<BaseNode> nodesToProcess)
		{
			using (new ProcessingScope(this))
			{
				isProcessing++;
				HashSet<BaseNode> finalNodes = new HashSet<BaseNode>();

				// Gather all nodes to process:
				foreach (var node in nodesToProcess)
				{
					foreach (var dep in GetNodeDependencies(node))
					{
						finalNodes.Add(dep);
					}
				}

				ProcessNodeList(cmd, finalNodes);
			}
		}

		public List<BaseNode> GetNodeDependencies(BaseNode node)
		{
			HashSet<BaseNode> dependencies = new HashSet<BaseNode>();
			Stack<BaseNode> inputNodes = new Stack<BaseNode>(node.GetInputNodes());

			dependencies.Add(node);

			while (inputNodes.Count > 0)
			{
				var child = inputNodes.Pop();

				foreach (var parent in child.GetInputNodes())
					inputNodes.Push(parent);

				dependencies.Add(child);

				// Max dependencies on a node, maybe we can put a warning if it's reached?
				if (dependencies.Count > 2000)
					break;
			}

			return dependencies.OrderBy(d => d.computeOrder).ToList();
		}

		void ProcessNodeList(CommandBuffer cmd, HashSet<BaseNode> nodes)
		{
			HashSet<ILoopStart> starts = new HashSet<ILoopStart>();
			HashSet<ILoopEnd> ends = new HashSet<ILoopEnd>();
			HashSet<INeedLoopReset> iNeedLoopReset = new HashSet<INeedLoopReset>();
			Stack<(ILoopStart node, int index)> jumps = new Stack<(ILoopStart, int)>();

			var sortedNodes = nodes.Where(n => n.computeOrder > 0).OrderBy(n => n.computeOrder).ToList();

			int maxLoopCount = 0;
			jumps.Clear();
			starts.Clear();
			ends.Clear();
			iNeedLoopReset.Clear();

			for (int executionIndex = 0; executionIndex < sortedNodes.Count; executionIndex++)
			{
				maxLoopCount++;
				if (maxLoopCount > 10000)
					return;

				var node = sortedNodes[executionIndex];

				if (node is ILoopStart loopStart)
				{
					if (!starts.Contains(loopStart))
					{
						loopStart.PrepareLoopStart();
						jumps.Push((loopStart, executionIndex));
						starts.Add(loopStart);
					}
				}

				bool finalIteration = false;
				if (node is ILoopEnd loopEnd)
				{
					if (jumps.Count == 0)
					{
						Debug.Log("Aborted execution, for end without start");
						return ;
					}

					var startLoop = jumps.Peek();
					if (!ends.Contains(loopEnd))
					{
						loopEnd.PrepareLoopEnd(startLoop.node);
						ends.Add(loopEnd);
					}

					// Jump back to the foreach start
					if (!startLoop.node.IsLastIteration())
					{
						executionIndex = startLoop.index - 1;
					}
					else
					{
						var fs2 = jumps.Pop();
						starts.Remove(fs2.node);
						ends.Remove(loopEnd);
						finalIteration = true;
					}
				}

				if (node is INeedLoopReset i)
				{
					if (!iNeedLoopReset.Contains(i))
					{
						i.PrepareNewIteration();
						iNeedLoopReset.Add(i);
					}

					// TODO: remove this node form iNeedLoopReset when we go over a foreach start again
				}
			
				if (finalIteration && node is ILoopEnd le)
				{
					le.FinalIteration();
				}

				ProcessNode(cmd, node);
			}
		}

		public override void Run()
		{
			using (new ProcessingScope(this))
			{
				// mixtureDependencies.Clear();

				UpdateComputeOrder();

				// Update node dependencies
				// foreach (var node in graph.nodes)
				// 	mixtureDependencies.Add(node, GetNodeDependencies(node));

				// New code:
				var cmd = new CommandBuffer { name = "Mixture" };
				RunGraphFor(cmd, graph.graphOutputs);

				// int maxLoopCount = 0;
				// foreach (var processList in processLists)
				// {
				// 	jumps.Clear();
				// 	starts.Clear();
				// 	ends.Clear();
				// 	iNeedLoopReset.Clear();

				// 	for (int executionIndex = 0; executionIndex < processList.Count; executionIndex++)
				// 	{
				// 		maxLoopCount++;
				// 		if (maxLoopCount > 10000)
				// 			return;

				// 		var node = processList[executionIndex];

				// 		if (node is ILoopStart loopStart)
				// 		{
				// 			if (!starts.Contains(loopStart))
				// 			{
				// 				loopStart.PrepareLoopStart();
				// 				jumps.Push((loopStart, executionIndex));
				// 				starts.Add(loopStart);
				// 			}
				// 		}

				// 		bool finalIteration = false;
				// 		if (node is ILoopEnd loopEnd)
				// 		{
				// 			if (jumps.Count == 0)
				// 			{
				// 				Debug.Log("Aborted execution, for end without start");
				// 				return ;
				// 			}

				// 			var startLoop = jumps.Peek();
				// 			if (!ends.Contains(loopEnd))
				// 			{
				// 				loopEnd.PrepareLoopEnd(startLoop.node);
				// 				ends.Add(loopEnd);
				// 			}

				// 			// Jump back to the foreach start
				// 			if (!startLoop.node.IsLastIteration())
				// 			{
				// 				executionIndex = startLoop.index - 1;
				// 			}
				// 			else
				// 			{
				// 				var fs2 = jumps.Pop();
				// 				starts.Remove(fs2.node);
				// 				ends.Remove(loopEnd);
				// 				finalIteration = true;
				// 			}
				// 		}

				// 		if (node is INeedLoopReset i)
				// 		{
				// 			if (!iNeedLoopReset.Contains(i))
				// 			{
				// 				i.PrepareNewIteration();
				// 				iNeedLoopReset.Add(i);
				// 			}

				// 			// TODO: remove this node form iNeedLoopReset when we go over a foreach start again
				// 		}
					
				// 		if (finalIteration && node is ILoopEnd le)
				// 		{
				// 			le.FinalIteration();
				// 		}

				// 		ProcessNode(cmd, node);
				// 	}
				// }

				Graphics.ExecuteCommandBuffer(cmd);
			}
		}

		void ProcessNode(CommandBuffer cmd, BaseNode node)
		{
			if (node.computeOrder < 0)
				return;

			if (node is MixtureNode m)
			{
				m.OnProcess(cmd);
				if (node is IUseCustomRenderTextureProcessing iUseCRT)
				{
                    var crt = iUseCRT.GetCustomRenderTexture();

                    if (crt != null)
                    {
                        crt.Update();
                        CustomTextureManager.UpdateCustomRenderTexture(cmd, crt);
                    }
				}
			}
			else
				node.OnProcess();
		}

		// public override void UpdateComputeOrder()
		// {
		// 	// Find graph outputs:
		// 	HashSet<BaseNode> outputs = new HashSet<BaseNode>();
		// 	foreach (var node in graph.nodes)
		// 	{
		// 		if (node.GetOutputNodes().Count() == 0)
		// 			outputs.Add(node);
		// 		node.computeOrder = 1;
		// 	}


		// 	Stack<BaseNode> dfs = new Stack<BaseNode>();
		// 	foreach (var output in outputs)
		// 	{
		// 		dfs.Push(output);
		// 		int index = 0;

		// 		var lst = new HashSet<BaseNode>();

		// 		while (dfs.Count > 0)
		// 		{
		// 			var node = dfs.Pop();

		// 			node.computeOrder = Mathf.Min(node.computeOrder, index);
		// 			index--;

		// 			foreach (var dep in node.GetInputNodes())
		// 				dfs.Push(dep);
					
		// 			lst.Add(node);
		// 		}

		// 	}

		// 	// foreach (var processList in processLists)
		// 	// 	foreach (var p in processList)
		// 	// 		Debug.Log(p + " | " + p.computeOrder);
		// }

		ComputeOrderInfo info = new ComputeOrderInfo();
		public ComputeOrderInfo GetComputeOrderInfo()
		{
			return info;
		}
	}
}