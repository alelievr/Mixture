using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[NodeCustomEditor(typeof(TextureNode))]
	public class TextureNodeView : MixtureNodeView
	{
		TextureNode		textureNode => nodeTarget as TextureNode;

		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);
            var textureField = this.Q(className: "unity-object-field") as ObjectField;

			var potConversionSettings = this.Q(nameof(TextureNode.POTMode));
			UpdatePOTSettingsVisibility(textureNode.textureAsset);

			// TODO: watch for texture asset changes (need the scripted importer thing)

            textureField.RegisterValueChangedCallback(e => {
				if (e.newValue is Texture t && t != null)
					UpdatePOTSettingsVisibility(t);
                ForceUpdatePorts();
            });

			void UpdatePOTSettingsVisibility(Texture t)
			{
				bool isPOT = true;

				if (t != null)
					isPOT = textureNode.IsPowerOf2(t);

				potConversionSettings.style.display = isPOT ? DisplayStyle.None : DisplayStyle.Flex;
			}
		}
	}
}