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
	[NodeCustomEditor(typeof(ColorNode))]
	public class ColorNodeView : MixtureNodeView
	{
		ColorNode		colorNode;

		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);
			colorNode = nodeTarget as ColorNode;

			var colorField = new ColorField() {
				label = "Color",
				value = colorNode.color,
			};
			colorField.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Color " + e.newValue);
				NotifyNodeChanged();
				colorNode.color = e.newValue;
			});

			controlsContainer.Add(colorField);
		}
	}
}