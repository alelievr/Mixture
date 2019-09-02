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

		protected new void OnEnable()
		{
			base.OnEnable();
			graphUnloaded += g => MixtureUpdater.RemoveGraphToProcess(g as MixtureGraph);
		}

		protected override void InitializeWindow(BaseGraph graph)
		{
            var mixture = (graph as MixtureGraph);
            bool realtime = mixture.isRealtime;
			titleContent = new GUIContent($"Mixture {(realtime ? "(RT) " : "")}- {mixture.name}", MixtureUtils.windowIcon);

			var graphView = new MixtureGraphView(this);

			rootView.Add(graphView);

			graphView.Add(new MixtureToolbar(graphView));
		}

		void OnDestroy()
		{
			MixtureUpdater.RemoveGraphToProcess(graphView as MixtureGraphView);
		}
	}
}