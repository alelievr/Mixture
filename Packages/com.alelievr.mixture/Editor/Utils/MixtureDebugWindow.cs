using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Mixture
{
	public class MixtureDebugWindow : EditorWindow
	{
        [MenuItem("Window/Analysis/Mixture Debugger")]
        public static void Open()
        {
            var debugWin = EditorWindow.GetWindow<MixtureDebugWindow>(false, "Mixture Debugger");

            debugWin.Show();
        }

        public void OnEnable()
        {

        }

        public void OnGUI()
        {
            EditorGUILayout.LabelField("Running mixture instances: (Static)", EditorStyles.boldLabel);

            // TODO: keep track of all opened mixture window to show a Focus button
            var mixtureWindows = Resources.FindObjectsOfTypeAll<MixtureGraphWindow>();

            foreach (var view in MixtureUpdater.views)
            {
                if (view.graph.isRealtime)
                    return;

                var window = mixtureWindows.FirstOrDefault(w => w.view == view);

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(view.graph.name);
                    if (window == null)
                        EditorGUILayout.LabelField("Can't find the window for this static mixture !");
                    else
                    {
                        if (GUILayout.Button("Focus"))
                        {
                            window.Show();
                            window.Focus();
                        }
                    }
                }
            }
        }
    }
}