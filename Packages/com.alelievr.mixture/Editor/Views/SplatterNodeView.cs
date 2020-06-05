using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeCustomEditor(typeof(SplatterNode))]
	public class SplatterNodeView : FixedShaderNodeView
	{
		public override void Enable()
		{
			base.Enable();

			BuildScatterUI();
		}

		void BuildScatterUI()
		{
			// FieldFactory.CreateField(typeof())
			// controlsContainer.Add();
		}
	}
}