using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System;
using System.Linq;
using UnityEditor;

namespace Mixture
{
	public class MixtureGraphView : BaseGraphView
	{
		// For now we will let the processor in the graph view
		public MixtureGraphProcessor	processor { get; private set; }
		public new MixtureGraph	graph => base.graph as MixtureGraph;

		public MixtureGraphView(EditorWindow window) : base(window)
		{
			initialized += Initialize;
			Undo.undoRedoPerformed += ReloadGraph;

			SetupZoom(0.05f, 8f);
		}

		public override List< Port > GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
		{
			var compatiblePorts = new List< Port >();
			PortView startPortView = startPort as PortView;

			compatiblePorts.AddRange(ports.ToList().Where(p => {
				var portView = p as PortView;

				if (p.direction == startPort.direction)
					return false;

				//Check if there is custom adapters for this assignation
				if (CustomPortIO.IsAssignable(startPort.portType, p.portType))
					return true;

				// Allow connection between RenderTexture and all texture types:
				Type startType = startPortView.portData.displayType ?? startPortView.portType;
				Type endType = portView.portData.displayType ?? portView.portType;
				if (startType == typeof(RenderTexture))
				{
					if (endType.IsSubclassOf(typeof(Texture)))
						return true;
				}
				if (endType == typeof(RenderTexture))
				{
					if (startType.IsSubclassOf(typeof(Texture)))
						return true;
				}

				//Check for type assignability
				if (!BaseGraph.TypesAreConnectable(startPort.portType, p.portType))
					return false;

				//Check if the edge already exists
				if (portView.GetEdges().Any(e => e.input == startPort || e.output == startPort))
					return false;

				return true;
			}));

			return compatiblePorts;
		}

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			base.BuildContextualMenu(evt);

			// Disable the Delete option if there is an output node view selected
			if (selection.Any(s => s is OutputNodeView && !(s is ExternalOutputNodeView)))
			{
				int deleteIndex = evt.menu.MenuItems().FindIndex(m => (m as DropdownMenuAction)?.name == "Delete");

				if (deleteIndex != -1)
				{
					evt.menu.RemoveItemAt(deleteIndex);
					evt.menu.InsertAction(deleteIndex, "Delete", a => {}, DropdownMenuAction.Status.Disabled);
				}
			}

			// Debug option:
			evt.menu.AppendAction("Help/Show All SubAssets", a => ShowAllSubAssets(), DropdownMenuAction.Status.Normal);
			evt.menu.AppendAction("Help/Hide All SubAssets", a => HideAllSubAssets(), DropdownMenuAction.Status.Normal);
		}

		void ShowAllSubAssets()
		{
			AssetDatabase.SaveAssets();
			foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(graph.mainAssetPath))
				asset.hideFlags = HideFlags.None;
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		void HideAllSubAssets()
		{
			AssetDatabase.SaveAssets();
			foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(graph.mainAssetPath))
			{
				if (asset != graph.outputTexture)
					asset.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			}
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		void Initialize()
		{
			RegisterCallback< KeyDownEvent >(KeyCallback);

			processor = new MixtureGraphProcessor(graph);
			computeOrderUpdated += processor.UpdateComputeOrder;
			graph.onOutputTextureUpdated += () => processor.Run();
			graph.onGraphChanges += _ => {
				this.schedule.Execute(() => ProcessGraph()).ExecuteLater(10);
				MarkDirtyRepaint();
			};

			// Run the processor when we open the graph
			ProcessGraph();
		}

		public void ProcessGraph() => processor?.Run();

		void ReloadGraph()
		{
			graph.outputNode = null;
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
					ProcessGraph();
					break ;
			}
		}
	}
}