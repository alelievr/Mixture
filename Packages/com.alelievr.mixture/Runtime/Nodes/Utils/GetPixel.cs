using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Mixture
{
	// [System.Serializable, NodeMenuItem("Utils/Get Pixel")]
	public class GetPixel : MixtureNode
	{
		[Input("Texture")]
		public Texture	texture;
		[Input("UVs"), SerializeField]
		public Vector4	uv;

		[Output]
		public Vector4	output;
		
		public override bool showDefaultInspector => true;

		public override string name => "Get Pixel";

		public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			OutputDimension.Texture2D,
		};

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (texture == null)
				return false;
			
			int pixelX = (int)(texture.width * uv.x);
			int pixelY = (int)(texture.height * uv.y);

			pixelX = Mathf.Clamp(pixelX, 0, texture.width - 1);
			pixelY = Mathf.Clamp(pixelY, 0, texture.height - 1);

			if (texture is Texture2D t)
				output = t.GetPixel(pixelX, pixelY);
			else if (texture is RenderTexture rt)
			{
				// This seems to not be working while the CRTs are processes :(
				int depth = texture.dimension == TextureDimension.Cube ? 6 : 1;
				var request = AsyncGPUReadback.Request(texture, 0, 0, texture.width, 0, texture.height, 0, depth, (r) => {
					ReadPixel(r);
				});

				request.Update();

				request.WaitForCompletion();
			}
			// TODO: texture 3D and cubemaps with GPU async readback

			return true;
		}

		void ReadPixel(AsyncGPUReadbackRequest request)
		{
			if (request.hasError)
			{
				Debug.LogError("Can't readback the texture from GPU");
				return ;
			}

			output = request.GetData<Color>(0)[0];
		}
	}
}