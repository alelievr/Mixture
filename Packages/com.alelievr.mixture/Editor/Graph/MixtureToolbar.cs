using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
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
			public const string parameterViewsText = "Parameters";
			public static GUIContent bugReport = new GUIContent("Bug Report", MixtureEditorUtils.bugIcon);
		}

		protected override void AddButtons()
		{
			// Add the hello world button on the left of the toolbar
			AddButton(Styles.processButtonText, Process, left: false);
			ToggleRealtime(graph.realtimePreview);
			AddToggle(Styles.realtimePreviewToggleText, graph.realtimePreview, ToggleRealtime, left: false);

			// bool exposedParamsVisible = graphView.GetPinnedElementStatus< ExposedParameterView >() != Status.Hidden;
			// For now we don't display the show parameters
			// AddToggle("Show Parameters", exposedParamsVisible, (v) => graphView.ToggleView<ExposedParameterView>());
			AddButton("Show In Project", () => {
				EditorGUIUtility.PingObject(graph.outputTexture);
				ProjectWindowUtil.ShowCreatedAsset(graph.outputTexture);
				// Selection.activeObject = graph;
			});
			AddToggle(Styles.parameterViewsText, graph.isParameterViewOpen, ToggleParameterView, left: true);

			// Pinned views
			bool pinnedViewsVisible = graphView.GetPinnedElementStatus< PinnedViewBoard >() != Status.Hidden;
			AddToggle("Pinned Views", pinnedViewsVisible, (v) => graphView.ToggleView< PinnedViewBoard >());
			if (!graph.isRealtime)
				AddButton(Styles.saveAllText, SaveAll , left: false);
			AddButton(Styles.bugReport, ReportBugCallback, left: false);
		}

		void ReportBugCallback()
		{
			Application.OpenURL(@"https://github.com/alelievr/Mixture/issues/new?assignees=alelievr&labels=bug&template=bug_report.md&title=%5BBUG%5D");
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
				HideButton(Styles.processButtonText);
				MixtureUpdater.AddGraphToProcess(graphView);
			}
			else
			{
				ShowButton(Styles.processButtonText);
				MixtureUpdater.RemoveGraphToProcess(graphView);
			}
			graph.realtimePreview = state;
		}

		void ToggleParameterView(bool state)
		{
			graphView.ToggleView<MixtureParameterView>();
			graph.isParameterViewOpen = state;
		}

		void AddProcessButton()
		{
		}

		void Process()
		{
			graphView.processor.Run();
		}
	}
}