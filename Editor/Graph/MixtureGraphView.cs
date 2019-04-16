using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using GraphProcessor;
using System;
using System.Linq;

public class MixtureToolbarGraphView : BaseGraphView
{
	public MixtureToolbarGraphView()
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
		// Create an output node if it does not exists
		if (!graph.nodes.Any(n => n is OutputNode))
		{
			AddNode(BaseNode.CreateFromType< OutputNode >(Vector2.zero));
		}
	}

	void CreateNodeOfType(Type type, Vector2 position)
	{
		RegisterCompleteObjectUndo("Added " + type + " node");
		AddNode(BaseNode.CreateFromType(type, position));
	}

	// TODO: override the delete function to prevent deleting the output node
}