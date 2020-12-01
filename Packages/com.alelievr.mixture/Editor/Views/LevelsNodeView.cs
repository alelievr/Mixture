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
	[NodeCustomEditor(typeof(Levels))]
	public class LevelsNodeView : MixtureNodeView 
	{
		Levels		levelsNode;

		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);
			levelsNode = nodeTarget as Levels;

			var histogram = new HistogramView();
			histogram.SetHistogramBuffer(levelsNode.histogram);
			controlsContainer.Add(histogram);
		}
	}
}