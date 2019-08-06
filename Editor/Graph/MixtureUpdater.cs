using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Mixture
{
    // TODO: move this elsewhere
    static class MixtureUpdater
    {
        static List< MixtureGraphView > views = new List< MixtureGraphView >();
        static MixtureUpdater()
        {
            EditorApplication.update += Update;
        }

        public static void AddGraphToProcess(MixtureGraphView view)
        {
            views.Add(view);
        }

        public static void RemoveGraphToProcess(MixtureGraphView view)
        {
            views.Remove(view);
        }

        public static void Update()
        {
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