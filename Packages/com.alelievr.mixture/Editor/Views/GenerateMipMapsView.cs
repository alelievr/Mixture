using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using GraphProcessor;
using System.Collections.Generic;
using System.IO;
using System;

namespace Mixture
{
	[NodeCustomEditor(typeof(GenerateMipMaps))]
	public class GenerateMipMapsView : ShaderNodeView 
	{
		GenerateMipMaps		genMipMapNode => nodeTarget as GenerateMipMaps;

		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);
			VisualElement shaderSettings = base.shaderSettings;

			var modeField = this.Q(nameof(GenerateMipMaps.mode)) as EnumField;
			modeField.RegisterValueChangedCallback(e => UpdateShaderSettingsVisibility());
			UpdateShaderSettingsVisibility();

			void UpdateShaderSettingsVisibility()
			{
				shaderSettings.style.display = genMipMapNode.mode == GenerateMipMaps.Mode.Custom ? DisplayStyle.Flex : DisplayStyle.None;
				ForceUpdatePorts();
			}
		}
	}
}