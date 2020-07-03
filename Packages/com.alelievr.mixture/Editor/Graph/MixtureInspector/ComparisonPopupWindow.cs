using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Mixture
{
    public class ComparisonPopupWindow : PopupWindowContent
    {
        bool toggle1 = true;
        bool toggle2 = true;
        bool toggle3 = true;

        MixtureNodeInspectorObjectEditor inspector;

        public static readonly int width = 200;
        public static readonly int height = 100;

        public override Vector2 GetWindowSize()
        {
            return new Vector2(width, height);
        }

        public ComparisonPopupWindow(MixtureNodeInspectorObjectEditor inspector)
        {
            this.inspector = inspector;
        }

        public override void OnGUI(Rect rect)
        {
            EditorGUI.BeginChangeCheck();
            var options = inspector.nodeWithPreviews.Select(n => n.name).ToArray();

            EditorGUIUtility.labelWidth = 80;
            inspector.comparisonTextureIndex = EditorGUILayout.Popup("Compare With", inspector.comparisonTextureIndex, options);
            EditorGUIUtility.labelWidth = 0;
            inspector.compareSlider = EditorGUILayout.Slider(inspector.compareSlider, 0, 1);

            if (EditorGUI.EndChangeCheck())
                inspector.Repaint();
        }

        public override void OnOpen()
        {
        }

        public override void OnClose()
        {
        }
    }
}