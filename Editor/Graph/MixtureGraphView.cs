using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using GraphProcessor;
using System;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Mixture
{
	public class MixtureGraphView : BaseGraphView
	{
		// For now we will let the processor in the graph view
		public MixtureProcessor	processor { get; private set; }
		public new MixtureGraph	graph => base.graph as MixtureGraph;

		public MixtureGraphView(EditorWindow window) : base(window)
		{
			initialized += Initialize;
		}

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			evt.menu.AppendSeparator();

			foreach (var nodeMenuItem in NodeProvider.GetNodeMenuEntries())
			{
				var mousePos = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
				Vector2 nodePosition = mousePos;
				evt.menu.AppendAction("Create/" + nodeMenuItem.Key,
					(e) => CreateNodeOfType(nodeMenuItem.Value, nodePosition),
					DropdownMenuAction.AlwaysEnabled
				);
			}

			base.BuildContextualMenu(evt);
		}

		void Initialize()
		{
			RegisterCallback< KeyDownEvent >(KeyCallback);

			processor = new MixtureProcessor(graph);
			computeOrderUpdated += processor.UpdateComputeOrder;

			// Run the processor when we open the graph
			processor.Run();
		}

		void CreateNodeOfType(Type type, Vector2 position)
		{
			RegisterCompleteObjectUndo("Added " + type + " node");
			AddNode(BaseNode.CreateFromType(type, position));
		}

		void KeyCallback(KeyDownEvent k)
		{
			// Handle mixture shortcuts
			switch (k.keyCode)
			{
				case KeyCode.P:
					processor.Run();
					break ;
			}
		}
	}
}