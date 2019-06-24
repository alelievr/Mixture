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

		Image preview;

		const int nodeWidth = 340;

		public override void Enable()
		{
			textureNode = nodeTarget as TextureNode;

			textureEditorUI = new VisualElement();
			textureEditorUI.style.paddingBottom = 8;
			textureEditorUI.style.paddingLeft = 8;
			textureEditorUI.style.paddingTop = 8;
			textureEditorUI.style.paddingRight = 8;
			preview = new Image();

			var textureField = new ObjectField() {
				label = "Texture",
				objectType = typeof(Texture2D),
				value = textureNode.texture
			};
			textureField.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Texture " + e.newValue);
				textureNode.texture = (Texture2D)e.newValue;
				UpdatePreview();
			});

			textureEditorUI.Add(textureField);
			controlsContainer.Add(textureEditorUI);
			controlsContainer.Add(preview);
			UpdatePreview();

			controlsContainer.style.backgroundColor = new StyleColor(new Color(.16f, .16f, .16f));
			controlsContainer.style.borderTopWidth = 1;
			controlsContainer.style.borderColor = new StyleColor(new Color(.12f, .12f, .12f));

			style.width = nodeWidth;
		}

		void UpdatePreview()
		{
			if(textureNode.texture != null)
			{
				preview.image = textureNode.texture;
				float ratio = (float)textureNode.texture.height / textureNode.texture.width;
				if(ratio > 1.0f)
				{
					preview.scaleMode = ScaleMode.ScaleToFit;
					preview.style.height = nodeWidth;
				}
				else
				{
					preview.scaleMode = ScaleMode.StretchToFill;
					preview.style.height = (float) nodeWidth * ratio;
				}
			} 
			else
			{
				preview.image = null;
				preview.style.height = 0;
			}

		}

	}
}