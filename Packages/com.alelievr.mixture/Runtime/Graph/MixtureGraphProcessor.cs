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

	// TODO: move to a dedicated workaround file for realtime processing
#if UNITY_EDITPR
	[UnityEditor.InitializeOnLoad(-10)]
#endif
	public static class MixtureProcessingCallbacks
	{
		static MixtureProcessingCallbacks() => Load();

		[RuntimeInitializeOnLoadMethod]
		static void Load()
		{
			// TODO: custom CRT sorting in the  CRT manager to manage the case where we have multiple runtime CRTs on the same branch and they need to be updated
			// in the correct order even if their materials/textures are not referenced into their params (they can be connected to a compute shader node for example).
			CustomTextureManager.onBeforeCustomTextureUpdated -= BeforeCustomRenderTextureUpdate;
			CustomTextureManager.onBeforeCustomTextureUpdated += BeforeCustomRenderTextureUpdate;
		}

		static void BeforeCustomRenderTextureUpdate(CommandBuffer cmd, CustomRenderTexture crt)
		{
			// TODO: check with everything we can that the CRT is a potential candidate for being aa graph
			// var graph = MixtureUtils.GetRealtimeGraphFromCRT(crt);

			// Debug.Log("crt: " + crt + " executed, loading graph " + graph);

			// // If the graph is valid and realtime
			// if (graph != null && graph.isRealtime == true)
			// {
			// 	MixtureGraphProcessor.processorInstances.TryGetValue(graph, out var processorSet);
			// 	if (processorSet == null)
			// 	{
			// 		var processor = new MixtureGraphProcessor(graph);
			// 		// Relay the event to the processor
			// 		processor.BeforeCustomRenderTextureUpdate(cmd, crt);
			// 	}
			// 	else
			// 	{
			// 		foreach (var processor in processorSet)
			// 		{
			// 			// Relay the event to the processor
			// 			processor.BeforeCustomRenderTextureUpdate(cmd, crt);
			// 		}
			// 	}
			// }
		}
	}

	public class MixtureGraphProcessor : BaseGraphProcessor
	{
		// A Multiton, oh god I never thought i'd be writing one in my life
		internal static Dictionary<MixtureGraph, HashSet<MixtureGraphProcessor>> processorInstances = new Dictionary<MixtureGraph, HashSet<MixtureGraphProcessor>>();

		new MixtureGraph		graph => base.graph as MixtureGraph;
		HashSet< BaseNode >		executedNodes = new HashSet<BaseNode>();
		public ComputeOrderInfo	info { get; private set; } = new ComputeOrderInfo();

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
			processorInstances.TryGetValue(graph as MixtureGraph, out var hashset);
			if (hashset == null)
				hashset = processorInstances[graph as MixtureGraph] = new HashSet<MixtureGraphProcessor>();

			hashset.Add(this);
		}

		~MixtureGraphProcessor()
		{
			processorInstances.Remove(graph);
		}

		public override void UpdateComputeOrder()
		{
			// TODO: update infos
			info.Clear();

			var sortedNodes = graph.nodes.Where(n => n.computeOrder >= 0).OrderBy(n => n.computeOrder).ToList();

			int loopIndex = 0;
			foreach (var node in sortedNodes)
			{
				if (node is ILoopStart fs)
					loopIndex++;
				info.forLoopLevel[node] = loopIndex;
				if (node is ILoopEnd fe)
					loopIndex--;
			}
		}

		internal void BeforeCustomRenderTextureUpdate(CommandBuffer cmd, CustomRenderTexture crt)
		{
			Debug.Log("Before: " + crt);
			if (isProcessing == 0)
			{
				// TODO: cache
				// Trigger the graph processing from a CRT update if we weren't processing
				BaseNode node = graph.nodes.FirstOrDefault(n => n is IUseCustomRenderTextureProcessing i && i.GetCustomRenderTexture() == crt);

				// node can be null if the CRT doesn't belong to the graph.
				if (node != null)
					RunGraphFor(cmd, new List<BaseNode>{ node });
				Debug.Log("RunGraphFOr: " + node);
			}
			else
			{
				// We don't do anything
			}
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
				HashSet<BaseNode> finalNodes = new HashSet<BaseNode>();

				// Gather all nodes to process:
				foreach (var node in nodesToProcess)
				{
					// TODO: cache node dependencies
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

				if (!dependencies.Add(child))
					continue;

				foreach (var parent in child.GetInputNodes())
					inputNodes.Push(parent);


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

			// Note that this jump pattern doesn't handle correctly the multiple dependencies a for loop
			// can have and it may cause some nodes to be processed multiple times unnecessarily, depending on the compute order.
			Stack<(ILoopStart node, int index)> jumps = new Stack<(ILoopStart, int)>();

			// TODO: cache?
			var sortedNodes = nodes.Where(n => n.computeOrder >= 0).OrderBy(n => n.computeOrder).ToList();

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
				UpdateComputeOrder();

				var cmd = new CommandBuffer { name = "Mixture" };
				RunGraphFor(cmd, graph.graphOutputs);

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
	}
}