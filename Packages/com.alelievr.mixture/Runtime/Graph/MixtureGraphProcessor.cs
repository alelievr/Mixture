using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using GraphProcessor;
using UnityEngine.Rendering;

namespace Mixture
{
	public class MixtureGraphProcessor : BaseGraphProcessor
	{
		List< BaseNode >		processList;
		new MixtureGraph		graph => base.graph as MixtureGraph;
		HashSet< BaseNode >		executedNodes = new HashSet<BaseNode>();

		Dictionary<CustomRenderTexture, List<BaseNode>> mixtureDependencies = new Dictionary<CustomRenderTexture, List<BaseNode>>();

		public MixtureGraphProcessor(BaseGraph graph) : base(graph)
		{
			// CustomTextureManager.onBeforeCustomTextureUpdated -= BeforeCustomRenderTextureUpdate;
			// CustomTextureManager.onBeforeCustomTextureUpdated += BeforeCustomRenderTextureUpdate;
		}

		public override void UpdateComputeOrder()
		{
			
		}

		// void BeforeCustomRenderTextureUpdate(CommandBuffer cmd, CustomRenderTexture crt)
		// {
		// 	if (mixtureDependencies.TryGetValue(crt, out var dependencies))
		// 	{
		// 		// Update the dependencies of the CRT
		// 		foreach (var nonCRTDep in dependencies)
		// 		{
		// 			// Make sure we don't execute multiple times the same node if there are multiple dependencies that needs it:
		// 			if (executedNodes.Contains(nonCRTDep))
		// 				continue;

		// 			executedNodes.Add(nonCRTDep);
		// 			ProcessNode(cmd, nonCRTDep);
		// 		}
		// 	}
		// }

		static CommandBuffer currentCmd;
		public static void AddGPUAndCPUBarrier()
		{
			Graphics.ExecuteCommandBuffer(currentCmd);
			currentCmd.Clear();
		}

		public static bool isProcessing;

		public override void Run()
		{
			isProcessing = true;
			mixtureDependencies.Clear();
			// HashSet<BaseNode> nodesToBeProcessed = new HashSet<BaseNode>();
			Stack<BaseNode> nodeToExecute = new Stack<BaseNode>();
			HashSet<ForeachStart> starts = new HashSet<ForeachStart>();
			HashSet<ForeachEnd> ends = new HashSet<ForeachEnd>();
			Stack<(ForeachStart node, int index)> jumps = new Stack<(ForeachStart, int)>();

			processList = graph.nodes.Where(n => n.computeOrder != -1).OrderBy(n => n.computeOrder).ToList();

			currentCmd = new CommandBuffer { name = "Mixture" };

			int maxLoopCount = 0;
			for (int executionIndex = 0; executionIndex < processList.Count; executionIndex++)
			{
				maxLoopCount++;
				if (maxLoopCount > 10000)
				{
					return;
				}

				var node = processList[executionIndex];

				if (node is ForeachStart fs)
				{
					if (!starts.Contains(fs))
					{
						fs.PrepareNewIteration();
						jumps.Push((fs, executionIndex));
						starts.Add(fs);
					}
				}

				bool finalIteration = false;
				if (node is ForeachEnd fe)
				{
					if (!ends.Contains(fe))
					{
						fe.PrepareNewIteration();
						ends.Add(fe);
					}

					if (jumps.Count == 0)
					{
						Debug.Log("Aborted execution, foreach end without start");
						return ;
					}
					var jump = jumps.Peek();

					// Jump back to the foreach start
					if (!jump.node.IsLastIteration())
						executionIndex = jump.index - 1;
					else
					{
						jumps.Pop();
						finalIteration = true;
					}
				}

				ProcessNode(currentCmd, node);
			
				if (finalIteration && node is ForeachEnd fe2)
				{
					fe2.FinalIteration();
				}
			}

			// foreach (var p in processList)
			// 	nodeToExecute.Push(p);

			// while (nodeToExecute.Count > 0)
			// {
			// 	var node = nodeToExecute.Pop();

			// 	ProcessNode(cmd, node);
			// 	if (node is ForeachStart fs)
			// 	{
			// 		if (!starts.Contains(fs))
			// 		{
			// 			// Gather nodes to execute multiple times:
			// 			var nodes = fs.GatherNodesInLoop();
			// 			var it = fs.PrepareNewIteration();
			// 			Debug.Log("Mixture feature it count: " + it);
			// 			foreach (var n in nodes)
			// 				Debug.Log(n);
			// 			for (int i = 0; i < it; i++)
			// 			{
			// 				foreach (var n in nodes)
			// 					nodeToExecute.Push(n);
			// 			}
			// 		}
			// 		starts.Add(fs);
			// 	}
			// }

			// For now we process every node multiple time,
			// future improvement: only refresh nodes when  asked by the CRT
			// foreach (BaseNode node in processList)
			// {
			// 	ProcessNode(cmd, node);
			// 	// if (node is IUseCustomRenderTextureProcessing iUseCRT)
			// 	// {
			// 	// 	var mixtureNode = node as MixtureNode;
			// 	// 	var crt = iUseCRT.GetCustomRenderTexture();

			// 	// 	if (crt != null)
			// 	// 	{
			// 	// 		crt.Update();
			// 	// 		CustomTextureManager.UpdateCustomRenderTexture(cmd, crt);
			// 	// 		// CustomTextureManager.RegisterNewCustomRenderTexture(crt);
			// 	// 		// var deps = mixtureNode.GetMixtureDependencies();
			// 	// 		// foreach (var dep in deps)
			// 	// 		// 	nodesToBeProcessed.Add(dep);
			// 	// 		// mixtureDependencies.Add(crt, deps);
			// 	// 		// crt.Update();
			// 	// 	}
			// 	// }
			// }

			// executedNodes.Clear();
			// CustomTextureManager.ForceUpdateNow();

			// // update all nodes that are not depending on a CRT
			// foreach (var node in processList.Except(nodesToBeProcessed))
			// 	ProcessNode(cmd, node);
			Graphics.ExecuteCommandBuffer(currentCmd);
			isProcessing = false;
		}

		void ProcessNode(CommandBuffer cmd, BaseNode node)
		{
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
	}
}