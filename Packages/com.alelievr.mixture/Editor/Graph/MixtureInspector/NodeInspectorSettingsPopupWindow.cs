using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Mixture
{
    public class NodeInspectorSettingsPopupWindow : PopupWindowContent
    {
        MixtureNodeInspectorObjectEditor inspector;
        MixtureNodeInspectorObject target;

        public static readonly int width = 260;
        public static readonly int height = 200;

        public override Vector2 GetWindowSize()
        {
            return new Vector2(width, height);
        }

        public NodeInspectorSettingsPopupWindow(MixtureNodeInspectorObjectEditor inspector)
        {
            this.inspector = inspector;
            this.target = inspector.mixtureInspector;
        }

        public override void OnGUI(Rect rect)
        {
            EditorGUI.BeginChangeCheck();
            var options = inspector.nodeWithPreviews.Select(n => n.name).ToArray();

            EditorGUIUtility.labelWidth = 90;
            target.filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", target.filterMode);
            target.exposure = EditorGUILayout.Slider("Exposure", target.exposure, -12, 12);
            EditorGUILayout.BeginHorizontal();
            bool r = GUILayout.Toggle((target.channels & PreviewChannels.R) != 0, "R", EditorStyles.toolbarButton);
            bool g = GUILayout.Toggle((target.channels & PreviewChannels.G) != 0, "G", EditorStyles.toolbarButton);
            bool b = GUILayout.Toggle((target.channels & PreviewChannels.B) != 0, "B", EditorStyles.toolbarButton);
            bool a = GUILayout.Toggle((target.channels & PreviewChannels.A) != 0, "A", EditorStyles.toolbarButton);
            target.channels = (r ? PreviewChannels.R : 0) |
					             (g ? PreviewChannels.G : 0) |
					             (b ? PreviewChannels.B : 0) |
					             (a ? PreviewChannels.A : 0);
            EditorGUILayout.EndHorizontal();

            var previewTexture = inspector.nodeWithPreviews.FirstOrDefault();
            int maxMip = previewTexture != null ? previewTexture.previewTexture.mipmapCount : 1;
            EditorGUI.BeginDisabledGroup(maxMip == 1);
            target.mipLevel = EditorGUILayout.Slider("Mip Level", target.mipLevel, 0, maxMip - 1);
            EditorGUI.EndDisabledGroup();

            target.alwaysRefresh = EditorGUILayout.Toggle("Always Refresh", target.alwaysRefresh);

            target.preserveAspect = EditorGUILayout.Toggle("Keep Aspect", target.preserveAspect);
            
            EditorGUILayout.LabelField("3D view", EditorStyles.boldLabel);

            target.texture3DPreviewMode = (MixtureNodeInspectorObject.Texture3DPreviewMode)EditorGUILayout.EnumPopup("Mode", target.texture3DPreviewMode);

            switch (target.texture3DPreviewMode)
            {
                default:
                case MixtureNodeInspectorObject.Texture3DPreviewMode.Volumetric:
                    target.texture3DDensity = EditorGUILayout.Slider("Density", target.texture3DDensity, 0, 1);
                    break;
                case MixtureNodeInspectorObject.Texture3DPreviewMode.DistanceFieldNormal:
                    target.texture3DDistanceFieldOffset = EditorGUILayout.FloatField("SDF Offset", target.texture3DDistanceFieldOffset);
                    break;
                case MixtureNodeInspectorObject.Texture3DPreviewMode.DistanceFieldColor:
                    target.texture3DDistanceFieldOffset = EditorGUILayout.FloatField("SDF Offset", target.texture3DDistanceFieldOffset);
                    target.sdfChannel = (MixtureNodeInspectorObject.SDFChannel)EditorGUILayout.EnumPopup("SDF Offset", target.sdfChannel);
                    break;
            }

            if (EditorGUI.EndChangeCheck())
                inspector.Repaint();
        }
    }
}