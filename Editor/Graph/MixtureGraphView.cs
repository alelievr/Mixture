﻿using UnityEngine.UIElements;
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
		public MixtureProcessor	processor { get; private set; }
		public new MixtureGraph	graph => base.graph as MixtureGraph;

		public MixtureGraphView(EditorWindow window) : base(window)
		{
			initialized += Initialize;
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
				if (!p.portType.IsReallyAssignableFrom(startPort.portType))
					return false;

				//Check if the edge already exists
				if (portView.GetEdges().Any(e => e.input == startPort || e.output == startPort))
					return false;

				return true;
			}));

			return compatiblePorts;
		}

		void Initialize()
		{
			RegisterCallback< KeyDownEvent >(KeyCallback);

			processor = new MixtureProcessor(graph);
			computeOrderUpdated += processor.UpdateComputeOrder;
			graph.onOutputTextureUpdated += () => processor.Run();

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