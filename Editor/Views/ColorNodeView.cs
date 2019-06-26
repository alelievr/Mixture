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
	public class ColorNodeView : ConstNodeView
	{
		ColorNode		colorNode;

		protected override int nodeWidth {get { return 340; } }

		public override void Enable()
		{
			base.Enable();
			colorNode = nodeTarget as ColorNode;

			var colorField = new ColorField() {
				label = "Color",
				value = colorNode.color
			};
			colorField.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Color " + e.newValue);
				colorNode.color = e.newValue;

			});

			propertyEditorUI.Add(colorField);
		}

	}
}