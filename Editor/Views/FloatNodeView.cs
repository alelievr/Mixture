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
	[NodeCustomEditor(typeof(FloatNode))]
	public class FloatNodeView : BaseNodeView
	{
		VisualElement	textureEditorUI;
		FloatNode		floatNode;

		const int nodeWidth = 250;

		public override void Enable()
		{
			floatNode = nodeTarget as FloatNode;

			textureEditorUI = new VisualElement();
			textureEditorUI.style.paddingBottom = 8;
			textureEditorUI.style.paddingLeft = 8;
			textureEditorUI.style.paddingTop = 8;
			textureEditorUI.style.paddingRight = 8;


			var textureField = new FloatField() {
				label = "Vector",
				value = floatNode.Float
			};
			textureField.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Vector " + e.newValue);
				floatNode.Float = e.newValue;

			});

			textureEditorUI.Add(textureField);
			controlsContainer.Add(textureEditorUI);


			controlsContainer.style.backgroundColor = new StyleColor(new Color(.16f, .16f, .16f));
			controlsContainer.style.borderTopWidth = 1;
			controlsContainer.style.borderColor = new StyleColor(new Color(.12f, .12f, .12f));

			style.width = nodeWidth;
		}

	}
}