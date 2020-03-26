using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using GraphProcessor;
using UnityEditor;

namespace Mixture
{
    public class PinnedViewBoard : PinnedElementView
    {
        BaseGraphView           graphView;

        public PinnedViewBoard() => title = "Pinned Views";

        public VisualElement    container;

        public static PinnedViewBoard instance;

        Dictionary< VisualElement, VisualElement > views = new Dictionary<VisualElement, VisualElement>();

        protected override void Initialize(BaseGraphView graphView)
        {
            this.graphView = graphView;

            container = new ScrollView(ScrollViewMode.Vertical);

            content.Add(container);

            instance = this;
        }

        public void Add(MixtureNodeView node, VisualElement view, string name)
        {
            VisualElement v = new VisualElement();
            v.Add(new Label(name));
            v.Add(view);
            
            v.RegisterCallback< MouseDownEvent >(e => {
                if (e.clickCount == 2)
                    node.UnpinView();
            });

            views[view] = v;
            container.Add(v);
        }

        public new void Remove(VisualElement view)
        {
            views[view].Remove(view);
            container.Remove(views[view]);
            views.Remove(view);
        }

        public bool HasView(VisualElement view) => views.ContainsKey(view);
    }
}