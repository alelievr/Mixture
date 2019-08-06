using UnityEngine;
using UnityEditor;

namespace Mixture
{
    public class InlineTextureDrawer : MixturePropertyDrawer
    {
        protected override void DrawerGUI (Rect position, MaterialProperty prop, string label, MaterialEditor editor, MixtureGraph graph, MixtureNodeView nodeView)
        {
            if (!(prop.textureValue is Texture) && prop.textureValue != null)
                prop.textureValue = null;
            
            Texture value = (Texture)EditorGUI.ObjectField(position, prop.displayName, prop.textureValue, typeof(Texture),false);

            if (GUI.changed)
                prop.textureValue = value;
        }
    }
}