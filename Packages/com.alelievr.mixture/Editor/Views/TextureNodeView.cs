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

        public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);
			textureNode = nodeTarget as TextureNode;


			var textureField = new ObjectField() {
				label = "Texture",
				objectType = typeof(Texture2D),
				value = textureNode.texture
			};
			textureField.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Texture " + e.newValue);
				textureNode.texture = (Texture2D)e.newValue;
				NotifyNodeChanged();
			});

			controlsContainer.Add(textureField);
		}

	}
}