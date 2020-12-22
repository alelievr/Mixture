using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;
using UnityEngine.Rendering;
using System.Linq;

namespace Mixture
{
	[NodeCustomEditor(typeof(Levels))]
	public class LevelsNodeView : MixtureNodeView 
	{
		Levels				levelsNode;

		// Workaround to update the sliders we have in the inspector / node
		// When serialization issues are fixed, we could have a drawer for min max and avoid to manually write the UI for it
		List<MinMaxSlider>	sliders = new List<MinMaxSlider>();

		public override void Enable(bool fromInspector)
		{
			levelsNode = nodeTarget as Levels;

			base.Enable(fromInspector);

			var slider = new MinMaxSlider("Luminance", levelsNode.min, levelsNode.max, 0, 1);
			sliders.Add(slider);
			slider.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Changed Luminance remap");
				levelsNode.min = e.newValue.x;
				levelsNode.max = e.newValue.y;
				foreach (var s in sliders)
					if (s != null && s.parent != null)
						s.SetValueWithoutNotify(e.newValue);
				NotifyNodeChanged();
			});
			controlsContainer.Add(slider);

			var mode = this.Q<EnumField>();

			mode.RegisterValueChangedCallback((m) => {
				UpdateMinMaxSliderVisibility((Levels.Mode)m.newValue);
			});
			UpdateMinMaxSliderVisibility(levelsNode.mode);

			// Compute histogram only when the inspector is selected
			if (fromInspector)
			{
				owner.graph.afterCommandBufferExecuted += UpdateHistogram;
				controlsContainer.RegisterCallback<DetachFromPanelEvent>(e => {
					owner.graph.afterCommandBufferExecuted -= UpdateHistogram;
				});
			}

			void UpdateHistogram()
			{
				if (levelsNode.output != null)
				{
					var cmd = CommandBufferPool.Get("Update Histogram");
					HistogramUtility.ComputeHistogram(cmd, levelsNode.output, levelsNode.histogramData);
					Graphics.ExecuteCommandBuffer(cmd);
				}
			}

			UpdateHistogram();

			void UpdateMinMaxSliderVisibility(Levels.Mode mode)
			{
				if (mode == Levels.Mode.Automatic)
					slider.style.display = DisplayStyle.None;
				else
					slider.style.display = DisplayStyle.Flex;
			}

			if (fromInspector)
			{
				var histogram = new HistogramView(levelsNode.histogramData, owner);
				controlsContainer.Add(histogram);
			}
		}
	}
}