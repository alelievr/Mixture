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
	[NodeCustomEditor(typeof(GradientNode))]
	public class GradientNodeView : MixtureNodeView
	{
		GradientNode		gradientNode;

		public override void Enable()
		{
			base.Enable();
			gradientNode = nodeTarget as GradientNode;

			var gradientField = new GradientField() {
                value = gradientNode.gradient
			};
            gradientField.style.height = 32;

			gradientField.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Gradient");
				gradientNode.gradient = (Gradient)e.newValue;
				NotifyNodeChanged();
                gradientNode.UpdateTexture();
			});

			controlsContainer.Add(gradientField);
		}
	}
}