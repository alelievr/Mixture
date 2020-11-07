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
	public class FloatNodeView : MixtureNodeView
	{
		FloatNode		floatNode;

		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);
			
			floatNode = nodeTarget as FloatNode;
			var textureField = new FloatField() {
				label = "Vector",
				value = floatNode.Float
			};
			textureField.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Vector " + e.newValue);
				NotifyNodeChanged();
				floatNode.Float = e.newValue;

			});

			controlsContainer.Add(textureField);
		}

	}
}