using UnityEditor;
using UnityEngine;

namespace Mixture
{
    [CustomPropertyDrawer(typeof(MixtureMesh))]
    public class MixtureMeshDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var meshProp = property.FindPropertyRelative(nameof(MixtureMesh.mesh));
            var matrixProp = property.FindPropertyRelative(nameof(MixtureMesh.localToWorld));
            EditorGUI.BeginProperty(position, GUIContent.none, property);
            meshProp.objectReferenceValue = EditorGUI.ObjectField(position, meshProp.objectReferenceValue, typeof(Mesh), false);
            EditorGUI.EndProperty();
        }
    }
}