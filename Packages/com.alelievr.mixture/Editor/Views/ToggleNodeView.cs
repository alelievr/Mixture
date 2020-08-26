using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[NodeCustomEditor(typeof(ToggleNode))]
	public class ToggleNodeView : MixtureNodeView
	{
		ToggleNode		toggleNode;

		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);
			toggleNode = nodeTarget as ToggleNode;

			var toggle = new Toggle() {
				label = "State",
				value = toggleNode.state
			};
			toggle.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Toggle State " + e.newValue);
				NotifyNodeChanged();
				toggleNode.state = e.newValue;
			});

			controlsContainer.Add(toggle);
		}
	}
}