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
	[NodeCustomEditor(typeof(VectorNode))]
	public class VectorNodeView : ConstNodeView
	{
		VectorNode		vectorNode;

		protected override int nodeWidth {get { return 340; } }

		public override void Enable()
		{
			base.Enable();
			vectorNode = nodeTarget as VectorNode;

			var vectorField = new Vector4Field() {
				label = "Vector",
				value = vectorNode.vector
			};
			vectorField.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Vector " + e.newValue);
				vectorNode.vector = e.newValue;

			});

			propertyEditorUI.Add(vectorField);
		}

	}
}