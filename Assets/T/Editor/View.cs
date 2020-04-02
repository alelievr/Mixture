using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using UnityEditor.Experimental.GraphView;

namespace Mixture
{
    [NodeCustomEditor(typeof(Suibgraph))]
	public class SuibgraphView : MixtureNodeView
    {
        public override void Enable()
        {
            var view = new BaseGraphView(MixtureGraphWindow.FindObjectOfType<MixtureGraphWindow>());
            var mixtureGraph = new MixtureGraph();
            mixtureGraph.AddNode(BaseNode.CreateFromType<TextureNode>(Vector2.zero));
            view.Initialize(mixtureGraph);
            controlsContainer.style.width = 500;
            controlsContainer.style.height = 300;
            controlsContainer.Add(view);
        }
    }
}