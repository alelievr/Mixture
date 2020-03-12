using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using GraphProcessor;

namespace Mixture
{
	using Status = UnityEngine.UIElements.DropdownMenuAction.Status;

	public class MixtureToolbar : ToolbarView
	{
		public MixtureToolbar(BaseGraphView graphView) : base(graphView) {}

		MixtureGraph			graph => graphView.graph as MixtureGraph;
		new MixtureGraphView	graphView => base.graphView as MixtureGraphView;

		class Styles
		{
			public const string realtimePreviewToggleText = "RealTime Preview";
			public const string processButtonText = "Process";
            public const string saveAllText = "Save All";
		}

		protected override void AddButtons()
		{
			// Add the hello world button on the left of the toolbar
			ToggleRealtime(graph.realtimePreview);
			AddToggle(Styles.realtimePreviewToggleText, graph.realtimePreview, ToggleRealtime, left: false);

			// bool exposedParamsVisible = graphView.GetPinnedElementStatus< ExposedParameterView >() != Status.Hidden;
			// For now we don't display the show parameters
			// AddToggle("Show Parameters", exposedParamsVisible, (v) => graphView.ToggleView<ExposedParameterView>());
			AddButton("Show In Project", () => {
				Selection.activeObject = graph;
				EditorGUIUtility.PingObject(graph.outputTexture);
			});
			bool pinnedViewsVisible = graphView.GetPinnedElementStatus< PinnedViewBoard >() != Status.Hidden;
			AddToggle("Pinned Views", pinnedViewsVisible, (v) => graphView.ToggleView< PinnedViewBoard >());
			if (!graph.isRealtime)
				AddButton(Styles.saveAllText, SaveAll , left: false);
		}

        void SaveAll()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Mixture", "Saving All...", 0.0f);

                graph.SaveMainTexture();

                List<ExternalOutputNode> externalOutputs = new List<ExternalOutputNode>();

                foreach(var node in graph.nodes)
                {
                    if(node is ExternalOutputNode && (node as ExternalOutputNode).asset != null)
                    {
                        externalOutputs.Add(node as ExternalOutputNode);
                    }
                }

                int i = 0;
                foreach(var node in externalOutputs)
                {
                    EditorUtility.DisplayProgressBar("Mixture", $"Saving {node.asset.name}...", (float)i/externalOutputs.Count);
                    (node as ExternalOutputNode).OnProcess();
                    graph.SaveExternalTexture((node as ExternalOutputNode), false);
                    i++;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

		void ToggleRealtime(bool state)
		{
			if (state)
			{
				RemoveButton(Styles.processButtonText, false);
				MixtureUpdater.AddGraphToProcess(graphView);
			}
			else
			{
				AddProcessButton();
				MixtureUpdater.RemoveGraphToProcess(graphView);
			}
			graph.realtimePreview = state;
		}

		void AddProcessButton()
		{
			AddButton(Styles.processButtonText, Process, left: false);
		}

		void Process()
		{
			graphView.processor.Run();
		}
	}
}