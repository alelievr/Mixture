using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UIElements;
using GraphProcessor;

namespace Mixture
{
	[NodeCustomEditor(typeof(GradientNode))]
	public class GradientNodeView : MixtureNodeView
	{
		GradientNode		gradientNode;

		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);
			gradientNode = nodeTarget as GradientNode;

			var gradientField = new GradientField() {
                value = gradientNode.gradient,
				colorSpace = ColorSpace.Gamma,
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