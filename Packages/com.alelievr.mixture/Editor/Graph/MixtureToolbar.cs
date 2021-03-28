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
			public const string realtimePreviewToggleText = "Always Update";
			public const string processButtonText = "Process";
            public const string saveAllText = "Save All";
			public const string parameterViewsText = "Parameters";
			public static GUIContent documentation = new GUIContent("Documentation", MixtureEditorUtils.documentationIcon);
			public static GUIContent bugReport = new GUIContent("Bug Report", MixtureEditorUtils.bugIcon);
			public static GUIContent featureRequest = new GUIContent("Feature Request", MixtureEditorUtils.featureRequestIcon);
			public static GUIContent improveMixture = new GUIContent("Improve Mixture", MixtureEditorUtils.featureRequestIcon);
			public static GUIContent focusText = new GUIContent("Fit View");
			static GUIStyle _improveButtonStyle = null;
			public static GUIStyle improveButtonStyle => _improveButtonStyle == null ? _improveButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft } : _improveButtonStyle;
		}

		public class ImproveMixturePopupWindow : PopupWindowContent
		{
			public static readonly int width = 150;

			public override Vector2 GetWindowSize()
			{
				return new Vector2(width, 94);
			}

			public override void OnGUI(Rect rect)
			{
				if (GUILayout.Button(Styles.documentation, Styles.improveButtonStyle))
					Application.OpenURL(@"https://alelievr.github.io/Mixture/");
				if (GUILayout.Button(Styles.bugReport, Styles.improveButtonStyle))
					Application.OpenURL(@"https://github.com/alelievr/Mixture/issues/new?assignees=alelievr&labels=bug&template=bug_report.md&title=%5BBUG%5D");
				if (GUILayout.Button(Styles.featureRequest, Styles.improveButtonStyle))
					Application.OpenURL(@"https://github.com/alelievr/Mixture/issues/new?assignees=alelievr&labels=enhancement&template=feature_request.md&title=");
			}
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
				EditorGUIUtility.PingObject(graph.mainOutputTexture);
				ProjectWindowUtil.ShowCreatedAsset(graph.mainOutputTexture);
				// Selection.activeObject = graph;
			});
			AddToggle(Styles.parameterViewsText, graph.isParameterViewOpen, ToggleParameterView, left: true);
			AddButton(Styles.focusText, () => graphView.FrameAll(), left: true);

			if (graph.type != MixtureGraphType.Realtime)
				AddButton(Styles.saveAllText, SaveAll , left: false);
			// AddButton(Styles.bugReport, ReportBugCallback, left: false);
			AddDropDownButton(Styles.improveMixture, ShowImproveMixtureWindow, left: false);
		}

		void ShowImproveMixtureWindow()
		{
			var rect = EditorWindow.focusedWindow.position;
			// rect.position = Vector2.zero;
			rect.xMin = rect.width - ImproveMixturePopupWindow.width;
			rect.yMin = 21;
			rect.size = Vector2.zero;
			PopupWindow.Show(rect, new ImproveMixturePopupWindow());
		}

        void SaveAll()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Mixture", "Saving All...", 0.0f);

                graph.SaveAllTextures();
				graph.UpdateLinkedVariants();

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
			EditorApplication.delayCall += graphView.processor.Run;
		}
	}
}