using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Mixture
{
    public class NodeInspectorSettingsPopupWindow : PopupWindowContent
    {
        MixtureNodeInspectorObjectEditor inspector;

        public static readonly int width = 220;
        public static readonly int height = 70;

        public override Vector2 GetWindowSize()
        {
            return new Vector2(width, height);
        }

        public NodeInspectorSettingsPopupWindow(MixtureNodeInspectorObjectEditor inspector)
        {
            this.inspector = inspector;
        }

        public override void OnGUI(Rect rect)
        {
            EditorGUI.BeginChangeCheck();
            var options = inspector.nodeWithPreviews.Select(n => n.name).ToArray();

            EditorGUIUtility.labelWidth = 70;
            inspector.filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", inspector.filterMode);
            inspector.exposure = EditorGUILayout.Slider("Exposure", inspector.exposure, -12, 12);
            EditorGUILayout.BeginHorizontal();
            bool r = GUILayout.Toggle((inspector.channels & PreviewChannels.R) != 0, "R", EditorStyles.toolbarButton);
            bool g = GUILayout.Toggle((inspector.channels & PreviewChannels.G) != 0, "G", EditorStyles.toolbarButton);
            bool b = GUILayout.Toggle((inspector.channels & PreviewChannels.B) != 0, "B", EditorStyles.toolbarButton);
            bool a = GUILayout.Toggle((inspector.channels & PreviewChannels.A) != 0, "A", EditorStyles.toolbarButton);
            inspector.channels = (r ? PreviewChannels.R : 0) |
					             (g ? PreviewChannels.G : 0) |
					             (b ? PreviewChannels.B : 0) |
					             (a ? PreviewChannels.A : 0);
            EditorGUILayout.EndHorizontal();
            // EditorGUIUtility.labelWidth = 0;
            // inspector.compareSlider = EditorGUILayout.Slider(inspector.compareSlider, 0, 1);

            if (EditorGUI.EndChangeCheck())
                inspector.Repaint();
        }
    }
}