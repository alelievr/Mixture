using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
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
            public const string saveAllText = "Save";
			public const string parameterViewsText = "Parameters";
			public static GUIContent documentation = new GUIContent("Documentation", MixtureEditorUtils.documentationIcon);
			public static GUIContent bugReport = new GUIContent("Bug Report", MixtureEditorUtils.bugIcon);
			public static GUIContent featureRequest = new GUIContent("Feature Request", MixtureEditorUtils.featureRequestIcon);
			public static GUIContent improveMixture = new GUIContent("Improve Mixture", MixtureEditorUtils.featureRequestIcon);
			public static GUIContent discord = new GUIContent("Discord", MixtureEditorUtils.discordIcon);
			public static GUIContent focusText = new GUIContent("Fit View");
			public static GUIContent settingsIcon = new GUIContent(MixtureEditorUtils.settingsIcon24);
			static GUIStyle _improveButtonStyle = null;
			public static GUIStyle improveButtonStyle => _improveButtonStyle == null ? _improveButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft } : _improveButtonStyle;
		}

		enum TextureType
		{
			[InspectorName("Type: 2D")]
			Type2D = OutputDimension.Texture2D,
			[InspectorName("Type: 3D")]
			Type3D = OutputDimension.Texture3D,
			[InspectorName("Type: Cubemap")]
			TypeCubemap = OutputDimension.CubeMap,
		}

		enum Resolution
		{
			[InspectorName("Resolution: 32")]
			Res32 = POTSize._32,
			[InspectorName("Resolution: 64")]
			Res64 = POTSize._64,
			[InspectorName("Resolution: 128")]
			Res128 = POTSize._128,
			[InspectorName("Resolution: 256")]
			Res256 = POTSize._256,
			[InspectorName("Resolution: 512")]
			Res512 = POTSize._512,
			[InspectorName("Resolution: 1024")]
			Res1024 = POTSize._1024,
			[InspectorName("Resolution: 2048")]
			Res2048 = POTSize._2048,
			[InspectorName("Resolution: 4096")]
			Res4096 = POTSize._4096,
			[InspectorName("Resolution: 8192")]
			Res8192 = POTSize._8192,
			[InspectorName("Custom")]
			Custom = POTSize.Custom,
		}

		public class ImproveMixturePopupWindow : PopupWindowContent
		{
			public static readonly int width = 150;

			public override Vector2 GetWindowSize()
			{
				return new Vector2(width, 124);
			}

			public override void OnGUI(Rect rect)
			{
				if (GUILayout.Button(Styles.documentation, Styles.improveButtonStyle))
					Application.OpenURL(@"https://alelievr.github.io/Mixture/");
				if (GUILayout.Button(Styles.bugReport, Styles.improveButtonStyle))
					Application.OpenURL(@"https://github.com/alelievr/Mixture/issues/new?assignees=alelievr&labels=bug&template=bug_report.md&title=%5BBUG%5D");
				if (GUILayout.Button(Styles.featureRequest, Styles.improveButtonStyle))
					Application.OpenURL(@"https://github.com/alelievr/Mixture/issues/new?assignees=alelievr&labels=enhancement&template=feature_request.md&title=");
				if (GUILayout.Button(Styles.discord, Styles.improveButtonStyle))
					Application.OpenURL(@"https://discord.gg/DGxZRP3qeg");
			}
		}

		protected override void AddButtons()
		{
			// Left buttons
			AddButton(Styles.processButtonText, Process, left: true);

			ToggleRealtime(graph.realtimePreview);
			AddToggle(Styles.realtimePreviewToggleText, graph.realtimePreview, ToggleRealtime, left: true);

			if (graph.type != MixtureGraphType.Realtime)
				AddButton(Styles.saveAllText, SaveAll);
			
			AddSeparator(5);

			AddButton("Show In Project", ShowInProject);

			AddSeparator(5);

			AddButton(Styles.focusText, () => graphView.FrameAll());

			// Right buttons

			AddCustom(DrawResolutionAndDimensionFields, left: false);

			AddFlexibleSpace(left: false);

			AddToggle(Styles.parameterViewsText, graph.isParameterViewOpen, ToggleParameterView, left: false);

			AddButton(Styles.settingsIcon, () => {}, left: false);

			AddDropDownButton(Styles.improveMixture, ShowImproveMixtureWindow, left: false);
		}

		void ShowInProject()
		{
			EditorGUIUtility.PingObject(graph.mainOutputTexture);
			ProjectWindowUtil.ShowCreatedAsset(graph.mainOutputTexture);
		}

		void DrawResolutionAndDimensionFields()
		{
			EditorGUILayout.EnumPopup(Resolution.Res2048, EditorStyles.toolbarDropDown, GUILayout.Width(116));
			EditorGUILayout.EnumPopup(TextureType.Type2D, EditorStyles.toolbarDropDown, GUILayout.Width(114));
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