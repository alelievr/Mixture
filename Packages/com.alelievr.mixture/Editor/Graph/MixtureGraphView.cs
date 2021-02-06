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

		public MixtureNodeInspectorObject mixtureNodeInspector => nodeInspector as MixtureNodeInspectorObject;

		public MixtureGraphView(EditorWindow window) : base(window)
		{
			initialized += Initialize;
			Undo.undoRedoPerformed += ReloadGraph;
			nodeDuplicated += OnNodeDuplicated;

			SetupZoom(0.05f, 32f);
		}

		public override List< Port > GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
		{
			var compatiblePorts = new List< Port >();
			PortView startPortView = startPort as PortView;

			compatiblePorts.AddRange(ports.ToList().Where(p => {
				var portView = p as PortView;

				if (p.direction == startPort.direction)
					return false;

				if (p.node == startPort.node)
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

		protected override NodeInspectorObject CreateNodeInspectorObject()
		{
			var inspector = ScriptableObject.CreateInstance<MixtureNodeInspectorObject>();
			inspector.name = "";
			inspector.hideFlags = HideFlags.HideAndDontSave ^ HideFlags.NotEditable;

			return inspector;
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
			evt.menu.AppendAction("Help/Reimport Main Asset", a => ReimportMainAsset(), DropdownMenuAction.Status.Normal);
		}

		void ReimportMainAsset()
		{
			EditorUtility.SetDirty(AssetDatabase.LoadAssetAtPath<Texture>(graph.mainAssetPath));
			AssetDatabase.ImportAsset(graph.mainAssetPath, ImportAssetOptions.DontDownloadFromCacheServer | ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
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
				if (asset != graph.mainOutputTexture)
					asset.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			}
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		void OnNodeDuplicated(BaseNode sourceNode, BaseNode newNode)
		{
			if (newNode is ShaderNode s)
			{
				var oldShaderNode = sourceNode as ShaderNode;
				var duplicatedMaterial = new Material(oldShaderNode.material);
				duplicatedMaterial.hideFlags = oldShaderNode.material.hideFlags;

				s.material = duplicatedMaterial;
				graph.AddObjectToGraph(duplicatedMaterial);
			}
		}

		void Initialize()
		{
			RegisterCallback< KeyDownEvent >(KeyCallback);

			processor = MixtureGraphProcessor.GetOrCreate(graph);
			computeOrderUpdated += () => {
				processor.UpdateComputeOrder();
				UpdateNodeColors();
			};
			graph.onOutputTextureUpdated += () => ProcessGraph();
			graph.onGraphChanges += changes => {
				if (changes.addedEdge != null || changes.removedEdge != null
					|| changes.addedNode != null || changes.removedNode != null || changes.nodeChanged != null)
				{
					EditorApplication.delayCall += () => {
						ProcessGraph(changes.nodeChanged ?? changes.addedNode);
						MarkDirtyRepaint();
					};
				}
			};

			// Run the processor when we open the graph
			ProcessGraph();
		}

		public void ProcessGraph(BaseNode sourceNode = null)
		{
			if (sourceNode == null)
				processor?.Run();
			else
				processor?.RunFromNode(sourceNode);

			// Update the inspector in case a node was selected.
			if (Selection.activeObject is MixtureNodeInspectorObject)
			{
				var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
				foreach (var win in windows)
				{
					if (win.GetType().Name.Contains("Inspector"))
						win.Repaint();
				}
			}
		}

		void ReloadGraph()
		{
			graph.outputNode = null;
			ProcessGraph();
		}

		Color[] nestingLevel = new Color[]
		{
			new Color(0.06167674f, 0.1060795f, 0.1698113f),
			new Color(0.1509434f, 0.06763973f, 0.1494819f),
			new Color(0.05764706f, 0.098f, 0.08358824f),
		};

		void UpdateNodeColors()
		{
			// Get processing info from the processor
			foreach (var view in nodeViews)
			{
				// view.titleContainer.style.color = new StyleColor(StyleKeyword.Initial);
				if (processor.info.forLoopLevel.TryGetValue(view.nodeTarget, out var level))
				{
					if (level > 0)
					{
						level = Mathf.Max(level - 1, 0) % nestingLevel.Length;
						var c = nestingLevel[level];
						c *= 2;
						view.titleContainer.style.backgroundColor = c;
					}
				}
			}

			// Update groups too:
			foreach (var view in groupViews)
			{
				var startNodeGUID = view.group.innerNodeGUIDs.FirstOrDefault(guid => graph.nodesPerGUID.ContainsKey(guid) && graph.nodesPerGUID[guid] is ILoopStart);
				if (startNodeGUID != null)
				{
					if (processor.info.forLoopLevel.TryGetValue(graph.nodesPerGUID[startNodeGUID], out var level))
					{
						if (level > 0)
						{
							level = Mathf.Clamp(level - 1, 0, nestingLevel.Length);
							if (level >= 1)
								view.BringToFront();
							var c = nestingLevel[level];
							view.UpdateGroupColor(c);
						}
					}
				}
			}
		}

		public void CreateNodeOfType(Type type, Vector2 position)
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