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
	public abstract class ConstNodeView : BaseNodeView
	{
		protected VisualElement	propertyEditorUI;

		protected virtual int nodeWidth { get { return 250; } }
		const string stylesheetName = "MixtureCommon";

		public override void Enable()
		{
			var stylesheet = Resources.Load<StyleSheet>(stylesheetName);
			if(!styleSheets.Contains(stylesheet))
				styleSheets.Add(stylesheet);
			
			propertyEditorUI = new VisualElement();
			controlsContainer.Add(propertyEditorUI);

			propertyEditorUI.AddToClassList("PropertyEditorUI");
			controlsContainer.AddToClassList("ControlsContainer");
			style.width = nodeWidth;
		}

	}
}