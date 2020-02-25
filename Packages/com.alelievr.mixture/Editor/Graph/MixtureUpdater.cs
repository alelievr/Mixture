using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;

namespace Mixture
{
    // TODO: move this elsewhere
    static class MixtureUpdater
    {
        public static List< MixtureGraphView > views = new List< MixtureGraphView >();
        static MixtureUpdater()
        {
            EditorApplication.update += Update;
        }

        public static void AddGraphToProcess(MixtureGraphView view)
        {
            views.Add(view);
        }

        public static void RemoveGraphToProcess(MixtureGraph graph) => RemoveGraphToProcess(views.Find(v => v.graph == graph));
        public static void RemoveGraphToProcess(MixtureGraphView view) => views.Remove(view);

        public static void Update()
        {
            // When the editor is not focused we disable the realtime preview
			if (!InternalEditorUtility.isApplicationActive)
				return;

            views.RemoveAll(v => v?.graph == null);

            // TODO: check if view is visible
            foreach (var view in views)
            {
                view.processor.Run();
                view.MarkDirtyRepaint();
            }
        }
    }
}