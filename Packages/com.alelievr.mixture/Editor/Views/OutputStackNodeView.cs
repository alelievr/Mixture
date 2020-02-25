using GraphProcessor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System.Linq;

namespace Mixture
{
    [CustomStackNodeView(typeof(OutputStackNode))]
    public class OutputStackNodeView : BaseStackNodeView
    {
        new OutputStackNode stackNode;

        public OutputStackNodeView(OutputStackNode stackNode) : base(stackNode)
        {
            this.stackNode = stackNode;

            // We can't delete the output stack
            capabilities &= ~Capabilities.Deletable;
        }

        public override void Initialize(BaseGraphView graphView)
        {
            base.Initialize(graphView);

            var addExternalOutput = new Button(AddExternalOutput){
                text = "Add External Output"
            };

            addExternalOutput.ClearClassList();
            addExternalOutput.AddToClassList("stack-node-placeholder");
            addExternalOutput.AddToClassList("unity-button");

            // Add the button at the bottom of the content section
            this.Q("stackNodeContainers").Add(addExternalOutput);
        }

        void AddExternalOutput()
        {
            owner.RegisterCompleteObjectUndo("Added New External Output");

            var newNode = BaseNode.CreateFromType<ExternalOutputNode>(Vector2.zero);
            owner.AddNode(newNode);
            stackNode.nodeGUIDs.Add(newNode.GUID);

            // Add the external output to the stack
            AddElement(owner.nodeViewsPerNode[newNode]);
        }

        protected override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            bool isOutputView = false;

            // We don't accept anything except the output node of the graph
            if (element is OutputNodeView output)
            {
                isOutputView = output.nodeTarget == (owner.graph as MixtureGraph).outputNode;
                isOutputView |= element is ExternalOutputNodeView;
            }

            return isOutputView;
        }
    }
}