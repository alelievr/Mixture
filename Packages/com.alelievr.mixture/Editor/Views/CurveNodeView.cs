using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using GraphProcessor;
using System;

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

			controlsContainer.styleSheets.Add(Resources.Load<StyleSheet>("MixtureCurveColors"));

			var modeField = new EnumField("Mode", curveNode.mode);

			controlsContainer.Add(modeField as VisualElement);

			var rCurve = CreateCurveField(() => curveNode.redCurve, c => curveNode.redCurve = c);
			var gCurve = CreateCurveField(() => curveNode.greenCurve, c => curveNode.greenCurve = c);
			var bCurve = CreateCurveField(() => curveNode.blueCurve, c => curveNode.blueCurve = c);
			var aCurve = CreateCurveField(() => curveNode.alphaCurve, c => curveNode.alphaCurve = c);

			rCurve.AddToClassList("red");
			gCurve.AddToClassList("green");
			bCurve.AddToClassList("blue");
			aCurve.AddToClassList("white");

			controlsContainer.Add(rCurve);
			controlsContainer.Add(gCurve);
			controlsContainer.Add(bCurve);
			controlsContainer.Add(aCurve);

			if (fromInspector)
				controlsContainer.Add(AddControlField(nameof(CurveNode.evaluationRange), "Evaluation Range", false, () => {
                curveNode.UpdateTexture();
					NotifyNodeChanged();
				}));

			modeField.RegisterValueChangedCallback(e => {
				curveNode.mode = (CurveNode.CurveOutputMode)e.newValue;
				owner.RegisterCompleteObjectUndo("Change Curve Mode");
				UpdateVisibleCurves();
                curveNode.UpdateTexture();
				NotifyNodeChanged();
			});
			UpdateVisibleCurves();

			var p = new CustomStyleProperty<Color>("--unity-curve-color");

			void UpdateVisibleCurves()
			{
				rCurve.style.display = DisplayStyle.None;
				gCurve.style.display = DisplayStyle.None;
				bCurve.style.display = DisplayStyle.None;
				aCurve.style.display = DisplayStyle.None;

				switch (curveNode.mode)
				{
					case CurveNode.CurveOutputMode.RRRR:
					case CurveNode.CurveOutputMode.R:
						rCurve.style.display = DisplayStyle.Flex;
						break;
					case CurveNode.CurveOutputMode.RG:
						rCurve.style.display = DisplayStyle.Flex;
						gCurve.style.display = DisplayStyle.Flex;
						break;
					case CurveNode.CurveOutputMode.RGB:
						rCurve.style.display = DisplayStyle.Flex;
						gCurve.style.display = DisplayStyle.Flex;
						bCurve.style.display = DisplayStyle.Flex;
						break;
					case CurveNode.CurveOutputMode.RGBA:
						rCurve.style.display = DisplayStyle.Flex;
						gCurve.style.display = DisplayStyle.Flex;
						bCurve.style.display = DisplayStyle.Flex;
						aCurve.style.display = DisplayStyle.Flex;
						break;
				}
			}
		}


		CurveField CreateCurveField(Func<AnimationCurve> getter, Action<AnimationCurve> setter)
		{
			var curveField = new CurveField() {
                value = getter(),
				renderMode = CurveField.RenderMode.Mesh,
			};
            curveField.style.height = 128;

			curveField.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Curve");
				setter((AnimationCurve)e.newValue);
                curveNode.UpdateTexture();
				NotifyNodeChanged();
			});

			return curveField;
		}
	}
}