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
		}

		protected override void AddButtons()
		{
			// Add the hello world button on the left of the toolbar
			ToggleRealtime(graph.realtimePreview);
			AddToggle(Styles.realtimePreviewToggleText, graph.realtimePreview, ToggleRealtime, left: false);

			// bool exposedParamsVisible = graphView.GetPinnedElementStatus< ExposedParameterView >() != Status.Hidden;
			// For now we don't display the show parameters
			// AddToggle("Show Parameters", exposedParamsVisible, (v) => graphView.ToggleView<ExposedParameterView>());
			AddButton("Show In Project", () => EditorGUIUtility.PingObject(graphView.graph));
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