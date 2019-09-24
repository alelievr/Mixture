using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;

namespace Mixture
{
	public class MixtureGraphWindow : BaseGraphWindow
	{
		public static BaseGraphWindow Open(MixtureGraph graph)
		{
			// Focus the window if the graph is already opened
			var mixtureWindows = EditorWindow.FindObjectsOfType<MixtureGraphWindow>();
			foreach (var mixtureWindow in mixtureWindows)
			{
				if (mixtureWindow.graph == graph)
				{
					mixtureWindow.Focus();
					return mixtureWindow;
				}
			}

			var graphWindow = EditorWindow.CreateWindow< MixtureGraphWindow >();

			graphWindow.Show();

			graphWindow.InitializeGraph(graph);

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