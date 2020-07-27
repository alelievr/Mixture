using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using GraphProcessor;

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
			owner.Connect(endView.inputPortViews[0], outputPortViews[0]);
        }

		public override void Enable()
		{
			var indexLabel = new Label { text = "Current Index: " + forNode.index };
			forNode.onProcessed += () => indexLabel.text = "Current Index: " + forNode.index;
			controlsContainer.Add(indexLabel);
        }
	}
}