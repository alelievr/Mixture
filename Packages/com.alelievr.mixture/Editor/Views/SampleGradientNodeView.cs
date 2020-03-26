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
	[NodeCustomEditor(typeof(SampleGradientNode))]
	public class SampleGradientNodeView : MixtureNodeView
	{
        SampleGradientNode sampleGradientNode;

		public override void Enable()
		{
			base.Enable();
			sampleGradientNode = nodeTarget as SampleGradientNode;

			var gradientField = new GradientField() {
                value = sampleGradientNode.gradient
			};
            gradientField.style.height = 32;

			gradientField.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Gradient");
				sampleGradientNode.gradient = (Gradient)e.newValue;
				NotifyNodeChanged();
			});

			controlsContainer.Add(gradientField);
		}
	}
}