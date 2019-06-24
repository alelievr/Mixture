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
	[NodeCustomEditor(typeof(TextureNode))]
	public class TextureNodeView : BaseNodeView
	{
		VisualElement	textureEditorUI;
		TextureNode		textureNode;


		public override void Enable()
		{
			textureNode = nodeTarget as TextureNode;

			var title = new Label("Properties");
			title.style.unityFontStyleAndWeight = FontStyle.Bold;
			title.style.fontSize = 14;
			title.style.marginBottom = 6;

			textureEditorUI = new VisualElement();
			textureEditorUI.style.paddingBottom = 8;
			textureEditorUI.style.paddingLeft = 8;
			textureEditorUI.style.paddingTop = 8;
			textureEditorUI.style.paddingRight = 8;

			textureEditorUI.Add(title);

			var textureField = new ObjectField() {
				label = "Texture",
				objectType = typeof(Texture2D)
			};
			textureField.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Texture " + e.newValue);
				textureNode.texture = (Texture2D)e.newValue;
			});

			textureEditorUI.Add(textureField);
			controlsContainer.Add(textureEditorUI);
			controlsContainer.style.backgroundColor = new StyleColor(new Color(.16f, .16f, .16f));
			controlsContainer.style.borderTopWidth = 1;
			controlsContainer.style.borderColor = new StyleColor(new Color(.12f, .12f, .12f));

			style.width = 340;
		}

	}
}