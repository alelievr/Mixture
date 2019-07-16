using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;

namespace Mixture
{
	public class MixtureGraphWindow : BaseGraphWindow
	{
		// Currently the only way to open a graph is to use an asset
		// [MenuItem("Window/Mixture")]
		public static BaseGraphWindow Open()
		{
			var graphWindow = GetWindow< MixtureGraphWindow >();

			graphWindow.Show();

			return graphWindow;
		}

		protected override void InitializeWindow(BaseGraph graph)
		{
			titleContent = new GUIContent("Mixture Graph", MixtureUtils.icon);

			var graphView = new MixtureGraphView(this);

			rootView.Add(graphView);

			graphView.Add(new MixtureToolbar(graphView));
		}
	}
}