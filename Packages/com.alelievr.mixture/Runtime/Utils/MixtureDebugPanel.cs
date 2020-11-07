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
        static MixtureDebugPanel() => LoadDebugPanel();

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
            var table = new DebugUI.Table() { displayName = "Name", isReadOnly = true };
            var header = new DebugUI.Table.Row()
            {
                displayName = "",
                children = { new DebugUI.Value() { displayName = "Processor Count",
                    getter = () => {
                        // This getter is called first at each render
                        // It is used to update the volumes
                        if (Time.time - timer < refreshRate)
                            return "";
                        timer = Time.time;
                        RefreshMixtureDebug(null, false);
                        return "";
                    }
                } }
            };
            // header.opened = true;
            table.children.Add(header);


            foreach (var kp in MixtureGraphProcessor.processorInstances)
            {
                var graph = kp.Key;
                var processors = kp.Value;

                var row = new DebugUI.Table.Row()
                {
                    displayName = graph.name,
                    children = {
                        new DebugUI.Value() {
                            displayName = "Processor Count",
                            getter = () => {
                                                
                                return processors.Count.ToString();
                            }
                        }
                    }
                };

                table.children.Add(row);
            }
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