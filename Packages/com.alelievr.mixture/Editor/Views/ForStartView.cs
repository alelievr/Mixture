using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[NodeCustomEditor(typeof(ForStart))]
	public class ForStartView : MixtureNodeView
	{
		ForStart		forNode => nodeTarget as ForStart;

        public override void OnCreated()
        {
			var pos = nodeTarget.position.position + new Vector2(300, 0);
            var endView = owner.AddNode(BaseNode.CreateFromType(typeof(ForEnd), pos));
			var group = new Group("For Loop", pos);
			group.innerNodeGUIDs.Add(nodeTarget.GUID);
			group.innerNodeGUIDs.Add(endView.nodeTarget.GUID);
			owner.AddGroup(group);
			owner.Connect(endView.inputPortViews[0], outputPortViews.FirstOrDefault(p => p.portName == "Output"));
        }

		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);
			controlsContainer.Add(AddControlField(nameof(ForStart.inputCount), "Iter"));
			var indexLabel = new Label { text = "Current Index: " + forNode.index };
			forNode.onProcessed += () => indexLabel.text = "Current Index: " + forNode.index;
			controlsContainer.Add(indexLabel);
        }
	}
}