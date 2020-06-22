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

		public override void Run()
		{
			mixtureDependencies.Clear();
			// HashSet<BaseNode> nodesToBeProcessed = new HashSet<BaseNode>();
			Stack<BaseNode> nodeToExecute = new Stack<BaseNode>();
			HashSet<ForeachStart> starts = new HashSet<ForeachStart>();

			processList = graph.nodes.Where(n => n.computeOrder != -1).OrderByDescending(n => n.computeOrder).ToList();

			CommandBuffer cmd = new CommandBuffer { name = "Mixture" };

			foreach (var p in processList)
				nodeToExecute.Push(p);

			while (nodeToExecute.Count > 0)
			{
				var node = nodeToExecute.Pop();

				ProcessNode(cmd, node);
				if (node is ForeachStart fs)
				{
					if (!starts.Contains(fs))
					{
						// Gather nodes to execute multiple times:
						var nodes = fs.GatherNodesInLoop();
						var it = fs.PrepareNewIteration();
						Debug.Log("Mixture feature it count: " + it);
						foreach (var n in nodes)
							Debug.Log(n);
						for (int i = 0; i < it; i++)
						{
							foreach (var n in nodes)
								nodeToExecute.Push(n);
						}
					}
					starts.Add(fs);
				}
			}

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
			Graphics.ExecuteCommandBuffer(cmd);
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