using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[Documentation(@"
Renders a mesh using the material specified in the 'material' field.
")]

	[System.Serializable, NodeMenuItem("Mesh/Render Mesh")]
	public class RenderMesh : MixtureNode 
	{
		public enum MapType
		{
			UV,
			Position,
			Normal,
			Tangent,
			BiTangent,
			PrimitiveId,
			VertexColor,
		}

		[Input, ShowAsDrawer, Tooltip("The mesh to be rendered")]
		public MixtureMesh mesh;

        [Input, ShowAsDrawer, Tooltip("The material used to render the mesh")]
        public Material material;

		[ShowInInspector, Tooltip("In case the mesh has multiple sub-meshes, you can select which one to render with this field")]
		public int submeshIndex;

		[Output]
        public CustomRenderTexture output;

		public override string name => "Render Mesh";

		public override bool	showDefaultInspector => true;
		public override Texture previewTexture => output;
		protected override MixtureRTSettings defaultRTSettings => Get2DOnlyRTSettings(base.defaultRTSettings);
		public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			OutputDimension.Texture2D,
		};

		MaterialPropertyBlock materialProperties;

		protected override void Enable()
		{
			UpdateTempRenderTexture(ref output);
			materialProperties = new MaterialPropertyBlock();
		}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
            if (!base.ProcessNode(cmd) || mesh == null || material == null)
                return false;

            // Update temp target in case settings changes
			UpdateTempRenderTexture(ref output);

            // Insert your code here 
			cmd.SetRenderTarget(output);
			cmd.ClearRenderTarget(false, true, Color.clear, 1.0f);
			cmd.DrawMesh(mesh.mesh, mesh.localToWorld, material, submeshIndex, 0, materialProperties);

			return true;
		}

        protected override void Disable()
		{
			base.Disable();
		}
	}
}