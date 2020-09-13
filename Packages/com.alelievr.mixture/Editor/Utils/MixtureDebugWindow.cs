using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Mixture
{
	public class MixtureDebugWindow : EditorWindow
	{
        Vector2 scrollPosition;

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
            using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPosition))
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

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Running mixture instances: (Runtime)", EditorStyles.boldLabel);

                // TODO: keep track of all opened mixture window to show a Focus button
                var graphs = Resources.FindObjectsOfTypeAll<MixtureGraph>();

                foreach (var graph in graphs)
                {
                    if (!graph.isRealtime)
                        return;

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(graph.name);
                        var path = AssetDatabase.GetAssetPath(graph);
                        EditorGUILayout.LabelField(path);
                        if (GUILayout.Button("Select"))
                        {
                            var mainAsset = AssetDatabase.LoadAssetAtPath<Texture>(path);
                            EditorGUIUtility.PingObject(mainAsset);
                            Selection.activeObject = mainAsset;
                        }
                    }
                }

                EditorGUILayout.Space();

                EditorGUILayout.LabelField($"Currently Loaded Custom Render Textures", EditorStyles.boldLabel);
                foreach (var crt in CustomTextureManager.customRenderTextures.ToList())
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"name: {crt.name}");
                        EditorGUILayout.LabelField($"HashCode: {crt.GetHashCode()}");
                        if (GUILayout.Button("Select"))
                            Selection.activeObject = crt;
                        if (GUILayout.Button("Unload"))
                            Resources.UnloadAsset(crt);
                    }
                }

                EditorGUILayout.Space();

                EditorGUILayout.LabelField($"Mixture Processors", EditorStyles.boldLabel);
                foreach (var kp in MixtureGraphProcessor.processorInstances)
                {
                    var graph = kp.Key;

                    foreach (var processor in kp.Value)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField($"Processor: {processor.GetHashCode()}");
                            EditorGUILayout.LabelField($"Target Graph: {processor.graph.name}");
                        }
                    }
                }

                scrollPosition = scroll.scrollPosition;
            }
        }
    }
}