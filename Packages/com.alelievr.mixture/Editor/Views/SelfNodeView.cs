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
	[NodeCustomEditor(typeof(SelfNode))]
	public class SelfNodeView : MixtureNodeView
	{
		SelfNode		selfNode;


		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);
			selfNode = nodeTarget as SelfNode;

            controlsContainer.Add(new Button(ResetTexture) { text = "Reset"});
		}

		void ResetTexture()
		{
			selfNode.ResetOutputTexture();
			NotifyNodeChanged();
		}
	}
}