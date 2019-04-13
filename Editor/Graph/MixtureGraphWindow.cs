using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;

public class MixtureGraphWindow : BaseGraphWindow
{
	// Currenly the only way to open a graph is to use an asset
	// [MenuItem("Window/Mixture")]
	public static BaseGraphWindow Open()
	{
		var graphWindow = GetWindow< MixtureGraphWindow >();

		graphWindow.Show();

		return graphWindow;
	}

	protected override void InitializeWindow(BaseGraph graph)
	{
		titleContent = new GUIContent("Mixture Graph");

		var graphView = new MixtureToolbarGraphView();

		rootView.Add(graphView);

		graphView.Add(new MixtureToolbar(graphView));
	}
}
