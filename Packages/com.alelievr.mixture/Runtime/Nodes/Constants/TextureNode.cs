using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
[Documentation(@"
The Texture node can accept any type of texture in parameter (2D, 3D, 2DArray, Cube, CubeArray, RenderTexture).
The output type of the node will update according to the type of texture provided. In case the texture type changes, the output edges may be destroyed.
")]

	[System.Serializable, NodeMenuItem("Constants/Texture")]
	public class TextureNode : MixtureNode
	{
		[Output(name = "Texture"), SerializeField]
		public Texture texture;

		public override bool 	hasSettings => false;
		public override string	name => "Texture";
        public override Texture previewTexture => texture;
		public override bool	showDefaultInspector => true;

		[CustomPortBehavior(nameof(texture))]
		IEnumerable<PortData> OutputTextureType(List<SerializableEdge> edges)
		{
			var dim = (texture is RenderTexture rt) ? rt.dimension : texture?.dimension;
			yield return new PortData
			{
				displayName = "Texture",
				displayType = dim == null ? typeof(Texture) : TextureUtils.GetTypeFromDimension(dim.Value),
				identifier = nameof(texture),
				acceptMultipleEdges = true,
			};
		}
    }
}