using UnityEngine;
using UnityEditor;

namespace Mixture
{
    public class InlineTextureDrawer : MixturePropertyDrawer
    {
        bool visibleInInspector = true;

        public InlineTextureDrawer() {}
        public InlineTextureDrawer(string v)
        {
            visibleInInspector = v != "HideInNodeInspector";
        }

        protected override void DrawerGUI (Rect position, MaterialProperty prop, string label, MaterialEditor editor, MixtureGraph graph, MixtureNodeView nodeView)
        {
            if (!visibleInInspector)
                return;

            if (!(prop.textureValue is Texture) && prop.textureValue != null)
                prop.textureValue = null;
            
            Texture value = (Texture)EditorGUI.ObjectField(position, prop.displayName, prop.textureValue, typeof(Texture),false);

            if (GUI.changed)
                prop.textureValue = value;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
            => visibleInInspector ? base.GetPropertyHeight(prop, label, editor) : -EditorGUIUtility.standardVerticalSpacing;
    }
}