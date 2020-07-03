using UnityEngine.UIElements;
using GraphProcessor;

namespace Mixture
{
	[NodeCustomEditor(typeof(RandomColorNode))]
	public class RandomColorNodeView : MixtureNodeView
	{
		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);

			var node = nodeTarget as RandomColorNode;

            var h = new MinMaxSlider("H", node.minHue, node.maxHue, 0.0f, 1.0f);
            var s = new MinMaxSlider("S", node.minSat, node.maxSat, 0.0f, 1.0f);
            var v = new MinMaxSlider("V", node.minValue, node.maxValue, 0.0f, 1.0f);

            h.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Hue " + e.newValue);
                node.minHue = e.newValue.x;
                node.maxHue = e.newValue.y;
				NotifyNodeChanged();
            });
            
            s.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Saturation " + e.newValue);
                node.minSat = e.newValue.x;
                node.maxSat = e.newValue.y;
				NotifyNodeChanged();
            });
            
            v.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Value " + e.newValue);
                node.minValue = e.newValue.x;
                node.maxValue = e.newValue.y;
				NotifyNodeChanged();
            });

			controlsContainer.Add(h);
			controlsContainer.Add(s);
			controlsContainer.Add(v);
		}
	}
}