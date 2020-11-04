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
			};
			UpdateGradientColorSpace();
            gradientField.style.height = 32;

			owner.graph.outputNode.onTempRenderTextureUpdated += UpdateGradientColorSpace;

			gradientField.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Gradient");
				gradientNode.gradient = (Gradient)e.newValue;
				NotifyNodeChanged();
                gradientNode.UpdateTexture();
			});

			void UpdateGradientColorSpace()
			{
				var graphFormat = owner.graph.outputNode.rtSettings.GetGraphicsFormat(owner.graph);
				gradientField.colorSpace = GraphicsFormatUtility.IsSRGBFormat(graphFormat) ? ColorSpace.Gamma : ColorSpace.Linear;
				gradientField.MarkDirtyRepaint();
				gradientField.value = gradientField.value;
			}

			controlsContainer.Add(gradientField);
		}
	}
}