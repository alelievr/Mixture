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
	public class TextureNodeView : MixtureNodeView
	{
		TextureNode		textureNode;

		Image preview;

		public override void Enable()
		{
			base.Enable();
			textureNode = nodeTarget as TextureNode;
			preview = new Image();

			var textureField = new ObjectField() {
				label = "Texture",
				objectType = typeof(Texture2D),
				value = textureNode.texture
			};
			textureField.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Texture " + e.newValue);
				textureNode.texture = (Texture2D)e.newValue;
				UpdatePreviewImage();
			});

			propertyEditorUI.Add(textureField);
			controlsContainer.Add(preview);
			UpdatePreviewImage();
		}

		void UpdatePreviewImage()
		{
			if(textureNode.texture != null)
			{
				preview.image = textureNode.texture;
				float ratio = (float)textureNode.texture.height / textureNode.texture.width;
				if(ratio > 1.0f)
				{
					preview.scaleMode = ScaleMode.ScaleToFit;
					preview.style.height = nodeTarget.nodeWidth;
				}
				else
				{
					preview.scaleMode = ScaleMode.StretchToFill;
					preview.style.height = nodeTarget.nodeWidth * ratio;
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