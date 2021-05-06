using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;

namespace Mixture
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public class MixtureDebugPanel
    {
#if UNITY_EDITOR
        static MixtureDebugPanel()
        {
            UnityEditor.EditorApplication.delayCall += LoadDebugPanel;
        }
#endif

        static DebugUI.Widget[] mixtureDebugItems;
        const string mixturePanel = "Mixture";

        [RuntimeInitializeOnLoadMethod]
        static void LoadDebugPanel()
        {
            var d = DebugManager.instance;
            var list = new List<DebugUI.Widget>();

            // TODO: actual useful information: GPU/CPU time, update count per second, ect.

            // First row for volume info
            float timer = 0.0f, refreshRate = 0.2f;
            var table = new DebugUI.Table() { displayName = "Graph Name", isReadOnly = true };

            foreach (var kp in MixtureGraphProcessor.processorInstances)
            {
                var graph = kp.Key;
                var processors = kp.Value;

                var row = new DebugUI.Table.Row()
                {
                    displayName = String.IsNullOrEmpty(graph.name) ? "(No Name)" : graph.name,
                };

                // One column
                row.children.Add( new DebugUI.Value{ displayName = "Processor Count", getter = () => processors.Count.ToString() } );

                // Another one
                row.children.Add(new DebugUI.Value { displayName = "Update Count", getter = () => graph.mainOutputTexture.updateCount});
    
                table.children.Add(row);
            }

            if (MixtureGraphProcessor.processorInstances.Count == 0)
            {
                list.Add(new DebugUI.Value{ displayName = "Graph List", getter = () => {
                        if (Time.time - timer < refreshRate)
                            return "";
                        timer = Time.time;
                        RefreshMixtureDebug(null, false);

                        return "No Mixture Currently In Use";
                    }
                });
            }
            else
                list.Add(table);

            var panel = DebugManager.instance.GetPanel(mixturePanel, true);
            panel.flags = DebugUI.Flags.None;
            panel.children.Add(list.ToArray());
            mixtureDebugItems = list.ToArray();
        }

        static void RefreshMixtureDebug<T>(DebugUI.Field<T> field, T value)
        {
            var panel = DebugManager.instance.GetPanel(mixturePanel);
            if (panel != null)
                panel.children.Remove(mixtureDebugItems);
            LoadDebugPanel();
        }
    }
}