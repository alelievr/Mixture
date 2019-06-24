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
	[NodeCustomEditor(typeof(VectorNode))]
	public class VectorNodeView : BaseNodeView
	{
		VisualElement	textureEditorUI;
		VectorNode		vectorNode;

		const int nodeWidth = 340;

		public override void Enable()
		{
			vectorNode = nodeTarget as VectorNode;

			textureEditorUI = new VisualElement();
			textureEditorUI.style.paddingBottom = 8;
			textureEditorUI.style.paddingLeft = 8;
			textureEditorUI.style.paddingTop = 8;
			textureEditorUI.style.paddingRight = 8;


			var textureField = new Vector4Field() {
				label = "Vector",
				value = vectorNode.vector
			};
			textureField.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Vector " + e.newValue);
				vectorNode.vector = e.newValue;

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