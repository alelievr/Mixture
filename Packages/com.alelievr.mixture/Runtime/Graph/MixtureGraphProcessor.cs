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

		Dictionary<CustomRenderTexture, List<BaseNode>> mixtureDependencies = new Dictionary<CustomRenderTexture, List<BaseNode>>();

		public MixtureGraphProcessor(BaseGraph graph) : base(graph)
		{
			CustomTextureManager.onBeforeCustomTextureUpdated -= BeforeCustomRenderTextureUpdate;
			CustomTextureManager.onBeforeCustomTextureUpdated += BeforeCustomRenderTextureUpdate;
		}

		public override void UpdateComputeOrder() {}

		void BeforeCustomRenderTextureUpdate(CommandBuffer cmd, CustomRenderTexture crt)
		{
			if (mixtureDependencies.TryGetValue(crt, out var dependencies))
			{
				// Update the dependencies of the CRT
				foreach (var nonCRTDep in dependencies)
					ProcessNode(cmd, nonCRTDep);
			}
		}

		public override void Run()
		{
			mixtureDependencies.Clear();
			HashSet<BaseNode> nodesToBeProcessed = new HashSet<BaseNode>();

			processList = graph.nodes.OrderBy(n => n.computeOrder).ToList();

			// For now we process every node multiple time,
			// future improvement: only refresh nodes when  asked by the CRT
			foreach (BaseNode node in processList)
			{
				if (node is IUseCustomRenderTextureProcessing iUseCRT)
				{
					var mixtureNode = node as MixtureNode;
					var crt = iUseCRT.GetCustomRenderTexture();

					if (crt != null)
					{
						CustomTextureManager.RegisterNewCustomRenderTexture(crt);
						var deps = mixtureNode.GetMixtureDependencies();
						foreach (var dep in deps)
							nodesToBeProcessed.Add(dep);
						mixtureDependencies.Add(crt, deps);
						crt.Update();
					}
				}
			}

			CustomTextureManager.ForceUpdateNow();

			// update all nodes that are not depending on a CRT
			CommandBuffer cmd = new CommandBuffer{ name = "Temp no-crt" };
			foreach (var node in processList.Except(nodesToBeProcessed))
				ProcessNode(cmd, node);
			Graphics.ExecuteCommandBuffer(cmd);
		}

		void ProcessNode(CommandBuffer cmd, BaseNode node)
		{
			if (node is MixtureNode m)
				m.OnProcess(cmd);
			else
				node.OnProcess();
		}
	}
}