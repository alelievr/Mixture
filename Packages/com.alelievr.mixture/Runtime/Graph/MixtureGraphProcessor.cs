using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using GraphProcessor;
using UnityEngine.Rendering;

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

		Dictionary<CustomRenderTexture, List<BaseNode>> mixtureDependencies = new Dictionary<CustomRenderTexture, List<BaseNode>>();

		public MixtureGraphProcessor(BaseGraph graph) : base(graph)
		{
			// CustomTextureManager.onBeforeCustomTextureUpdated -= BeforeCustomRenderTextureUpdate;
			// CustomTextureManager.onBeforeCustomTextureUpdated += BeforeCustomRenderTextureUpdate;
		}

		// public override void UpdateComputeOrder()
		// {
			
		// }

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
			HashSet<ILoopStart> starts = new HashSet<ILoopStart>();
			HashSet<ILoopEnd> ends = new HashSet<ILoopEnd>();
			HashSet<INeedLoopReset> iNeedLoopReset = new HashSet<INeedLoopReset>();
			Stack<(ILoopStart node, int index)> jumps = new Stack<(ILoopStart, int)>();

			UpdateComputeOrder();

			// processList = graph.nodes.Where(n => n.computeOrder != -1).OrderBy(n => n.computeOrder).ToList();

			currentCmd = new CommandBuffer { name = "Mixture" };

			int maxLoopCount = 0;
			foreach (var processList in processLists)
			{
				jumps.Clear();
				starts.Clear();
				ends.Clear();
				iNeedLoopReset.Clear();

				for (int executionIndex = 0; executionIndex < processList.Count; executionIndex++)
				{
					maxLoopCount++;
					if (maxLoopCount > 10000)
						return;

					var node = processList[executionIndex];

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

					ProcessNode(currentCmd, node);
				}
			}

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

		public override void UpdateComputeOrder()
		{
			// Find graph outputs:
			HashSet<BaseNode> outputs = new HashSet<BaseNode>();
			foreach (var node in graph.nodes)
			{
				if (node.GetOutputNodes().Count() == 0)
					outputs.Add(node);
				node.computeOrder = 1;
			}

			processLists.Clear();

			info.Clear();

			Stack<BaseNode> dfs = new Stack<BaseNode>();
			foreach (var output in outputs)
			{
				dfs.Push(output);
				int index = 0;

				var lst = new HashSet<BaseNode>();

				while (dfs.Count > 0)
				{
					var node = dfs.Pop();

					node.computeOrder = Mathf.Min(node.computeOrder, index);
					index--;

					foreach (var dep in node.GetInputNodes())
						dfs.Push(dep);
					
					lst.Add(node);
				}

				processLists.Add(lst.Where(n => n.computeOrder != 1).OrderBy(n => n.computeOrder).ToList());
			}

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

			// foreach (var processList in processLists)
			// 	foreach (var p in processList)
			// 		Debug.Log(p + " | " + p.computeOrder);
		}

		ComputeOrderInfo info = new ComputeOrderInfo();
		public ComputeOrderInfo GetComputeOrderInfo()
		{
			return info;
		}
	}
}