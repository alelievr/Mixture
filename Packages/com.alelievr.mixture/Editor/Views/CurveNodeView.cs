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
	[NodeCustomEditor(typeof(CurveNode))]
	public class CurveNodeView : MixtureNodeView
	{
		CurveNode		curveNode;

		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);
			curveNode = nodeTarget as CurveNode;

			var gradientField = new CurveField() {
                value = curveNode.curve
			};
            gradientField.style.height = 128;

			gradientField.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Curve");
				curveNode.curve = (AnimationCurve)e.newValue;
				NotifyNodeChanged();
                curveNode.UpdateTexture();
			});

			controlsContainer.Add(gradientField);
		}
	}
}