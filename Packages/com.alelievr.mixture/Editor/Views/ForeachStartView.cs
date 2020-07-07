#if MIXTURE_EXPERIMENTAL
using UnityEngine;
using UnityEditor.UIElements;
using GraphProcessor;

namespace Mixture
{
	[NodeCustomEditor(typeof(ForeachStart))]
	public class ForeachStartView : MixtureNodeView
	{
		ForeachStart		foreachNode => nodeTarget as ForeachStart;

        public override void OnCreated()
        {
            // TODO: create all other ndoes
			var pos = nodeTarget.position.position + new Vector2(300, 0);
            var endView = owner.AddNode(BaseNode.CreateFromType(typeof(ForeachEnd), pos));
			var group = new Group("Foreach", pos);
			group.innerNodeGUIDs.Add(nodeTarget.GUID);
			group.innerNodeGUIDs.Add(endView.nodeTarget.GUID);
			owner.AddGroup(group);
        }

		public override void Enable()
		{
			var indexField = new IntegerField { label = "index", value = foreachNode.index };
			foreachNode.onProcessed += () => indexField.SetValueWithoutNotify(foreachNode.index);
			controlsContainer.Add(indexField);

			var countField = new IntegerField { label = "count", value = foreachNode.count };
			foreachNode.onProcessed += () => countField.SetValueWithoutNotify(foreachNode.count);
			controlsContainer.Add(countField);
		}
	}
}
#endif