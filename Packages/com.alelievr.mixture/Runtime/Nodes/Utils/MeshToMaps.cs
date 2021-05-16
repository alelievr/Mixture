using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[Documentation(@"
This node allows to extract the information in a mesh and output it as a texture.

This process is done using the UV of the mesh to flatten it and output it's attribtues. You can select which mesh attribute you want to output with the Output Map field.
")]

	[System.Serializable, NodeMenuItem("Mesh/Mesh To Maps"), NodeMenuItem("Mesh/Extract Mesh Maps")]
	public class MeshToMaps : MixtureNode 
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

		[Input, ShowAsDrawer, Tooltip("The mesh from where the attributes will be extracted")]
		public MixtureMesh mesh;

		[Tooltip("Select which mesh attribute you want to output")]
		public MapType outputMap;

		[ShowInInspector, Tooltip("In case the mesh has multiple sub-meshes, you can select which one to render with this field")]
		public int submeshIndex;

		[ShowInInspector, Tooltip("Enable Conservative rasterization when rendering the mesh. It can help to keep small details in the mesh.")]
		public bool conservative = false;

		[Output]
        public CustomRenderTexture output;

		public override string name => "Mesh To Maps";

		public override bool	showDefaultInspector => true;
		public override Texture previewTexture => output;
		protected override MixtureSettings defaultSettings => Get2DOnlyRTSettings(base.defaultSettings);
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
            if (!base.ProcessNode(cmd) || mesh?.mesh == null)
                return false;

            // Update temp target in case settings changes
			UpdateTempRenderTexture(ref output);

            // Insert your code here 
			cmd.SetRenderTarget(output);
			cmd.ClearRenderTarget(false, true, Color.clear, 1.0f);
			materialProperties.SetFloat("_Mode", (int)outputMap);
			var mat = GetTempMaterial("Hidden/Mixture/MeshToMaps");
			mat.SetFloat("_Conservative", conservative ? 1.0f : 0.0f);
			cmd.DrawMesh(mesh.mesh, mesh.localToWorld, mat, submeshIndex, 0, materialProperties);

			return true;
		}

        protected override void Disable()
		{
			base.Disable();
		}
	}
}