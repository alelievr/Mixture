using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using System.Linq;

namespace Mixture
{
    // TODO: move this elsewhere
    static class MixtureUpdater
    {
        public static HashSet< MixtureGraphView >  views = new HashSet< MixtureGraphView >();
        static HashSet< MixtureGraph >          needsProcess = new HashSet<MixtureGraph>();
        static MixtureUpdater()
        {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }

        public static void AddGraphToProcess(MixtureGraphView view)
        {
            views.Add(view);
        }

        public static void RemoveGraphToProcess(MixtureGraph graph) => RemoveGraphToProcess(views.FirstOrDefault(v => v.graph == graph));
        public static void RemoveGraphToProcess(MixtureGraphView view) => views.Remove(view);

        public static void EnqueueGraphProcessing(MixtureGraph graph) => needsProcess.Add(graph);

        static double updateEditorTime = 0;
        public static void Update()
        {
            // When the editor is not focused we disable the realtime preview
			if (!InternalEditorUtility.isApplicationActive)
				return;

            float updateTimeMillis = 1.0f / Screen.currentResolution.refreshRate;
            if (updateEditorTime > Time.realtimeSinceStartupAsDouble)
                updateEditorTime = 0;
            if (Time.realtimeSinceStartupAsDouble - updateEditorTime < updateTimeMillis && updateEditorTime > 0)
                return;

            foreach (var view in views)
            {
                if (view.graph == null)
                    continue;
                // TODO: check if view is visible
                if ((view.graph.type != MixtureGraphType.Realtime && view.graph.realtimePreview) || needsProcess.Contains(view.graph))
                {
                    view.processor.Run();
                    view.MarkDirtyRepaint();
                    needsProcess.Remove(view.graph);
                }
            }
        }
    }
}