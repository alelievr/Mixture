using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[Documentation(@"
Binds the textures of a material in parameter. This node can be used for pre-viz with a mesh in the scene for faster iterations.
")]

	[System.Serializable, NodeMenuItem("Utils/Material Binder")]
	public class MaterialBinder : MixtureNode
	{
        [Input]
        public List<object> exposedProperties = new List<object>();

        public Material targetMaterial;

		public override string	name => "Material Binder (prototype)";

		public override bool showDefaultInspector => true;

        [CustomPortBehavior(nameof(exposedProperties))]
		public IEnumerable< PortData > ListMaterialBindings(List< SerializableEdge > edges)
		{
			if (targetMaterial == null)
				yield break;

			var s = targetMaterial.shader;
			var textureNames = targetMaterial.GetTexturePropertyNames();

			foreach (var textureName in textureNames)
			{
				int index = s.FindPropertyIndex(textureName);

				var flags = s.GetPropertyAttributes(index);
				if (flags.Any(f => f == "HideInInspector" || f == "NonModifiableTextureData"))
					continue;
				
				yield return new PortData{
					displayName = s.GetPropertyDescription(index),
					identifier = s.GetPropertyName(index),
					displayType = TextureUtils.GetTypeFromDimension(s.GetPropertyTextureDimension(index)),
                };
			}
		}

		[CustomPortInput(nameof(exposedProperties), typeof(object))]
		protected void GetMaterialInputs(List< SerializableEdge > edges)
		{
			if (targetMaterial == null)
				return;

			var s = targetMaterial.shader;

			foreach (var edge in edges)
			{
				string propName = edge.inputPort.portData.identifier;
				targetMaterial.SetTexture(propName, edge.passThroughBuffer as Texture);
			}
		}
    }
}
