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
	[NodeCustomEditor(typeof(RandomRangeNode))]
	public class RandomRangeNodeView : MixtureNodeView
	{
		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);

			var node = nodeTarget as RandomRangeNode;

            var min = new FloatField("Min")
            {
                value = node.min
            };
            min.RegisterValueChangedCallback(e =>
            {
                owner.RegisterCompleteObjectUndo("Updated Min " + e.newValue);
                node.min = e.newValue;
				NotifyNodeChanged();
            });

            var max = new FloatField("Max")
            {
                value = node.max
            };
            max.RegisterValueChangedCallback(e =>
            {
                owner.RegisterCompleteObjectUndo("Updated Max " + e.newValue);
                node.max = e.newValue;
				NotifyNodeChanged();
            });


            controlsContainer.Add(min);
            controlsContainer.Add(max);
        }
    }
}